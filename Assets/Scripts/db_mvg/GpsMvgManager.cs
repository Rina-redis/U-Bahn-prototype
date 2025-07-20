using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class GpsMvgManager : MonoBehaviour
{

    public static GpsMvgManager instance;
    public static float maxDistance = 100f, minSpeed = 20f, maxTimeToGo = 2f;
    private float latitude, longitude;
    private float speed = 0.0f;
    private int loop_delay = 1;
    private bool isOnStation = false;

    public TextAsset stationsJson;

    private List<Station> stations;

    private const string ApiUrl = "https://www.mvg.de/api/bgw-pt/v3/routes";

    public event Action<bool, string, string> OnHasStartedChanged;
    public event Action<bool, string, string> OnHasStoppedChanged;

    // NEW: event wykrycia nowej stacji
    public event Action<Station> OnNewStationDetected;
    private HashSet<string> knownStationIds = new HashSet<string>();

    private Station originStation;
    private Station destinationStation;

    private bool hasStarted = false;
    private bool hasStopped = false;

    private float prevLat;
    private float prevLon;
    private float prevTime;

    private Station startedOriginStation;
    private Station startedDestinationStation;

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
        stations = StationFinder.LoadStationsFromJson(stationsJson.text);

        foreach (var s in stations)
        {
            knownStationIds.Add(s.stationGlobalID);
        }

        StartCoroutine(StartLocationService());
        StartCoroutine(CheckForNewStations());
    }

    IEnumerator StartLocationService()
    {
        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogWarning("GPS failed to initialize.");
            yield break;
        }

        prevLat = Input.location.lastData.latitude;
        prevLon = Input.location.lastData.longitude;
        prevTime = Time.time;

        while (true)
        {
            yield return UpdateLocationAndCheckStatus();
            yield return new WaitForSeconds(loop_delay);
        }
    }

    private IEnumerator CheckForNewStations()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

            var currentStations = StationFinder.LoadStationsFromJson(stationsJson.text);
            foreach (var s in currentStations)
            {
                if (!knownStationIds.Contains(s.stationGlobalID))
                {
                    knownStationIds.Add(s.stationGlobalID);
                    stations.Add(s);
                    OnNewStationDetected?.Invoke(s);
                    Debug.Log($"NEW STATION DETECTED: {s.stationName} ({s.stationGlobalID})");
                }
            }
        }
    }

    private IEnumerator UpdateLocationAndCheckStatus()
    {
        var data = Input.location.lastData;
        latitude = data.latitude;
        longitude = data.longitude;

        float currentTime = Time.time;
        float deltaTime = currentTime - prevTime;

        float moveDistance = StationFinder.HaversineDistance(prevLat, prevLon, latitude, longitude);
        speed = (deltaTime > 0f) ? (moveDistance / deltaTime) * 3.6f : 0f;

        prevLat = latitude;
        prevLon = longitude;
        prevTime = currentTime;

        StationFinder finder = new StationFinder(latitude, longitude, stations);
        var nearestStation = finder.FindNearestStation();

        float distanceToStation = StationFinder.HaversineDistance(latitude, longitude, nearestStation.lat, nearestStation.@long);

        if (distanceToStation <= maxDistance)
        {
            isOnStation = true;

            destinationStation = stations.FirstOrDefault(s => s.stationName == nearestStation.stationName);

            originStation = null;
            if (destinationStation != null && destinationStation.connections != null && destinationStation.connections.Length > 0)
            {
                string originName = destinationStation.connections[0];
                originStation = stations.FirstOrDefault(s => s.stationName == originName);
            }

            if (destinationStation != null && originStation != null)
            {
                yield return StartCoroutine(FetchArrivalTime(
                    originStation.stationGlobalID,
                    destinationStation.stationGlobalID,
                    DateTime.UtcNow,
                    false,
                    "SCHIFF,UBAHN",
                    (arrivalTime) =>
                    {
                        if (arrivalTime != DateTime.MinValue)
                        {
                            var now = DateTime.UtcNow;
                            var minutesDiff = (arrivalTime - now).TotalMinutes;

                            bool newHasStarted = isOnStation && speed >= minSpeed && minutesDiff >= 0 && minutesDiff <= maxTimeToGo;
                            if (newHasStarted != hasStarted)
                            {
                                hasStarted = newHasStarted;
                                if (hasStarted)
                                {
                                    startedOriginStation = originStation;
                                    startedDestinationStation = destinationStation;
                                }
                                else
                                {
                                    startedOriginStation = null;
                                    startedDestinationStation = null;
                                }
                                OnHasStartedChanged?.Invoke(hasStarted,
                                    startedOriginStation?.stationGlobalID ?? "",
                                    startedDestinationStation?.stationGlobalID ?? "");
                            }

                            bool newHasStopped = isOnStation && speed < minSpeed;
                            if (newHasStopped != hasStopped)
                            {
                                hasStopped = newHasStopped;
                                OnHasStoppedChanged?.Invoke(hasStopped,
                                    startedOriginStation?.stationGlobalID ?? "",
                                    startedDestinationStation?.stationGlobalID ?? "");
                            }
                        }
                        else
                        {
                            if (hasStarted)
                            {
                                hasStarted = false;
                                startedOriginStation = null;
                                startedDestinationStation = null;
                                OnHasStartedChanged?.Invoke(false, "", "");
                            }
                            if (hasStopped)
                            {
                                hasStopped = false;
                                OnHasStoppedChanged?.Invoke(false, "", "");
                            }
                        }
                    }
                ));
            }
        }
        else
        {
            isOnStation = false;

            if (hasStarted)
            {
                hasStarted = false;
                startedOriginStation = null;
                startedDestinationStation = null;
                OnHasStartedChanged?.Invoke(false, "", "");
            }
            if (hasStopped)
            {
                hasStopped = false;
                OnHasStoppedChanged?.Invoke(false, "", "");
            }
        }
    }

    public string IsOnStation()
    {
        if (isOnStation && originStation != null && !string.IsNullOrEmpty(originStation.stationGlobalID))
        {
            return originStation.stationGlobalID;
        }
        return String.Empty;
    }

    public IEnumerator FetchArrivalTime(
        string originStationId,
        string destinationStationId,
        DateTime routingDateTime,
        bool isArrivalTime,
        string transportTypes,
        Action<DateTime> onArrivalTimeReceived)
    {
        string query = $"originStationGlobalId={originStationId}" +
                       $"&destinationStationGlobalId={destinationStationId}" +
                       $"&routingDateTime={routingDateTime:yyyy-MM-ddTHH:mm:ss.fffZ}" +
                       $"&routingDateTimeIsArrival={isArrivalTime.ToString().ToLower()}" +
                       $"&transportTypes={transportTypes}";

        string requestUrl = $"{ApiUrl}?{query}";

        using UnityWebRequest request = UnityWebRequest.Get(requestUrl);
        request.SetRequestHeader("Accept", "application/json, text/plain, */*");
        request.SetRequestHeader("User-Agent", "Mozilla/5.0 (UnityWebRequest)");
        request.SetRequestHeader("Accept-Language", "pl-PL,pl;q=0.9");

        yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
#else
        if (request.isNetworkError || request.isHttpError)
#endif
        {
            onArrivalTimeReceived?.Invoke(DateTime.MinValue);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            DateTime arrivalTime = ParseArrivalTimeFromJson(jsonResponse);
            onArrivalTimeReceived?.Invoke(arrivalTime);
        }
    }

    private DateTime ParseArrivalTimeFromJson(string json)
    {
        string wrappedJson = "{\"routes\":" + json + "}";
        RoutesWrapper wrapper = JsonUtility.FromJson<RoutesWrapper>(wrappedJson);

        if (wrapper?.routes != null && wrapper.routes.Count > 0)
        {
            var firstRoute = wrapper.routes[0];
            if (firstRoute.parts != null && firstRoute.parts.Count > 0)
            {
                var firstPart = firstRoute.parts[0];
                if (!string.IsNullOrEmpty(firstPart.to.plannedDeparture) &&
                    DateTime.TryParse(firstPart.to.plannedDeparture, out DateTime arrival))
                {
                    return arrival;
                }
            }
        }
        return DateTime.MinValue;
    }

    [Serializable]
    public class Station
    {
        public string stationName;
        public string stationGlobalID;
        public float lat;
        public float @long;
        public string[] connections;
    }

    private class StationFinder
    {
        private float currentLat;
        private float currentLon;
        private List<Station> stations;

        public StationFinder(float lat, float lon, List<Station> stations)
        {
            currentLat = lat;
            currentLon = lon;
            this.stations = stations;
        }

        public Station FindNearestStation()
        {
            return stations.OrderBy(s => HaversineDistance(currentLat, currentLon, s.lat, s.@long)).FirstOrDefault();
        }

        public static float HaversineDistance(float lat1, float lon1, float lat2, float lon2)
        {
            float R = 6371000;
            float dLat = Mathf.Deg2Rad * (lat2 - lat1);
            float dLon = Mathf.Deg2Rad * (lon2 - lon1);
            float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                      Mathf.Cos(Mathf.Deg2Rad * lat1) * Mathf.Cos(Mathf.Deg2Rad * lat2) *
                      Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
            float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
            return R * c;
        }

        public static List<Station> LoadStationsFromJson(string json)
        {
            return JsonUtilityWrapper.FromJsonListWrapped<Station>(json, "data");
        }
    }

    public static class JsonUtilityWrapper
    {
        [Serializable]
        private class Wrapper<T>
        {
            public List<T> data;
        }

        public static List<T> FromJsonListWrapped<T>(string json, string rootField = "data")
        {
            if (!json.TrimStart().StartsWith("{"))
            {
                json = $"{{\"{rootField}\":{json}}}";
            }

            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.data;
        }
    }

    [Serializable]
    private class RoutesWrapper
    {
        public List<Route> routes;
    }

    [Serializable]
    private class Route
    {
        public long uniqueId;
        public List<RoutePart> parts;
    }

    [Serializable]
    private class RoutePart
    {
        public StationInfo from;
        public StationInfo to;
    }

    [Serializable]
    private class StationInfo
    {
        public float latitude;
        public float lonfude;
        public string stationGlobalId;
        public int stationDivaId;
        public int platform;
        public bool platformChanged;
        public string place;
        public string name;
        public string plannedDeparture;
        public string[] transportTypes;
        public bool isViaStop;
        public string surroundingPlanLink;
        public string occupancy;
        public bool hasZoomData;
        public bool hasOutOfOrderEscalator;
        public bool hasOutOfOrderElevator;
    }
}
