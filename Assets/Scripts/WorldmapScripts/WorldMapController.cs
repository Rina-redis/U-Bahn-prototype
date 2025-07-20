using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using System;

public class WorldMapController : MonoBehaviour
{
    public static WorldMapController instance;

    public TrainController currTrain = null;

    public Station currentStation = null;

    [SerializeField] private StationTierData tierConfig;

    public HashSet<Station> map = new HashSet<Station>();


    private Coroutine _stationChecker;

    public bool temp = false;

    [SerializeField] private float CheckStationDelay;
    public bool isOnStation { get; private set; } = false;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        DontDestroyOnLoad(this);
        var allStationObjects = Resources.LoadAll<StationObj>("Stations");

        // Initialising map object
        HashSet<Station> tempMap = new HashSet<Station>();
        foreach (StationObj obj in allStationObjects)
        {
            tempMap.Add(new Station(obj));
        }

        while (tempMap.Count > 0)
        {
            Station station = tempMap.First();
            List<Station> neighbours = tempMap.Where(st => allStationObjects.FirstOrDefault(obj => obj.id == station.Id).neigbours.Select(obj => obj.id).Contains(st.Id)).ToList();
            station.Neighbours.AddRange(neighbours);
            foreach (Station neighbour in neighbours)
            {
                neighbour.Neighbours.Add(station);
            }
            map.Add(station);
            tempMap.Remove(station);
        }
        LoadMapData();

        // remove it later
        //StartChecking();

    }

    private void Start()
    {
        StartChecking();
    }

    private void LoadMapData()
    {
        //Load data from server
        // Warning mock shit ahead
        List<string> ids = map.Select(station => station.Id).ToList();
        Dictionary<string, float> stationScores = new Dictionary<string, float>();

        foreach (string id in ids)
        {
            stationScores.Add(id, UnityEngine.Random.Range(0, 1000));
        }
        //--------------------------------------------------

        foreach (Station station in map)
        {
            float stationValue;
            if (stationScores.TryGetValue(station.Id, out stationValue))
            {
                station.LoadData(stationValue, tierConfig.tierReqs);
                //Debug.Log(stationValue);
            }
            else
            {
                Debug.LogError("Wrong Id in loded map data");
            }
        }
    }

    public void LoadCurrentTrain()
    {
        ScriptableRoute route;
        // route = NetworkManager.LoadCurrTrainData()
        // mock shit:
        route = Resources.Load<ScriptableRoute>("Routes");
        // end mockshit

        List<Station> realRoute = new List<Station>();
        Station realCurrentStation = map.FirstOrDefault(st => st.Id == route.currStation.id);
        if (realCurrentStation == null)
        {
            Debug.LogError("Wrong current station id, check your network vibes");
            return;
        }

        for (int i = 0; i < route.routeObj.Count; ++i)
        {
            Station realStation = map.FirstOrDefault(st => st.Id == route.routeObj[i].id);
            if (realCurrentStation == null)
            {
                Debug.LogError("Wrong current station id, check your network vibes");
                return;
            }
            realRoute.Add(realStation);
        }

        currTrain = new TrainController(realRoute, realCurrentStation);
    }

    public Station GetStationById(string id)
    {
        return map.FirstOrDefault(station => station.Id == id);
    }


    public void StartChecking()
    {
        if (_stationChecker != null)
        {
            Debug.LogError("Someone is still checking(in silence)");
        }
        _stationChecker = StartCoroutine(CheckForStation());
    }

    public void StopChecking()
    {
        if (_stationChecker != null)
        {
            StopCoroutine(_stationChecker);
        }
        _stationChecker = null;
    }

    public HashSet<Station> getMap()
    {
        return map;
    }

    IEnumerator CheckForStation()
    {
        while (true)
        {
            // if check for station 
            string nearestStationId = GpsMvgManager.instance.IsOnStation();
            if (nearestStationId !=String.Empty) // if station is near
            {
                if (!isOnStation)
                {
                    //mock
                    isOnStation = true;
                    ConnectToStation(instance.GetStationById(nearestStationId));
                }
            }
            else
            {
                //Debug.Log("not on station");
                if (isOnStation)
                {
                    isOnStation = false;
                    DisconnectFromStation();
                }
            }
            yield return new WaitForSecondsRealtime(CheckStationDelay);
        }
    }
    private void ConnectToStation(Station nearStation)
    {
        currentStation = nearStation;
        ShopManager.instance?.InitShopForStation(nearStation);
        UIMasterController.instance.RebuildAll();
    }

    private void DisconnectFromStation()
    {
        currentStation = null;
        ShopManager.instance?.InitShopForStation(null);
        UIMasterController.instance.RebuildAll();

    }

    public void SetTrueTemp()
    {
        temp = true;
    }

    public void SetFalseTemp()
    {
        temp = false;
    }
}
