using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class GpsMvgManager : MonoBehaviour
{
    public static float maxDistance = 100f, minSpeed = 20f, maxTimeToGo = 2f;
    private float latitude, longitude;
    private float speed = 0.0f;
    private int loop_delay = 1; 
    private bool isMoving = false;

    public TextAsset stationsJson;

    private List<Station> stations;

    private const string ApiUrl = "https://www.mvg.de/api/bgw-pt/v3/routes";

    public event Action<bool, Station, Station> OnIsMovingChanged;

    private Station originStation;
    private Station destinationStation;

    void Start()
    {
        stationsJson = Resources.Load<TextAsset>("connections");
        stations = StationFinder.LoadStationsFromJson(stationsJson.text);
        StartCoroutine(StartLocationService());
    }

    private static string GetCurrentDateTime()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    IEnumerator StartLocationService()
    {
        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            //Debug.Log("Initializing GPS...");
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            //Debug.Log("GPS failed.");
            yield break;
        }

        float prevLat = Input.location.lastData.latitude;
        float prevLon = Input.location.lastData.longitude;
        float prevTime = Time.time;

        while (true)
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

            //Debug.Log($"Speed: {speed:F2} km/h");
            //Debug.Log($"Coordinates: Lat: {latitude:F6}, Lon: {longitude:F6}");

            StationFinder finder = new StationFinder(latitude, longitude, stations);
            var nearestStation = finder.FindNearestStation();
            //Debug.Log($"Nearest station: {nearestStation.stationName}");

            float distanceToStation = StationFinder.HaversineDistance(latitude, longitude, nearestStation.lat, nearestStation.@long);

            if (distanceToStation <= maxDistance)
            {

                //Debug.Log($"In the range of station. Distance: {distanceToStation:F2} m");

                bool newIsMoving = speed >= minSpeed;

                destinationStation = stations.FirstOrDefault(s => s.stationName == nearestStation.stationName);

                originStation = null;
                if (destinationStation != null && destinationStation.connections != null && destinationStation.connections.Length > 0)
                {
                    string originName = destinationStation.connections[0];
                    originStation = stations.FirstOrDefault(s => s.stationName == originName);
                }

                if (newIsMoving != isMoving)
                {
                    isMoving = newIsMoving;
                    OnIsMovingChanged?.Invoke(isMoving, originStation, destinationStation);
                }

                if (destinationStation != null && originStation != null)
                {
                    StartCoroutine(FetchArrivalTime(
                        originStationId: originStation.stationGlobalID,
                        destinationStationId: destinationStation.stationGlobalID,
                        routingDateTime: DateTime.UtcNow,
                        isArrivalTime: false,
                        transportTypes: "SCHIFF,UBAHN",
                        onArrivalTimeReceived: (arrivalTime) =>
                        {
                            if (arrivalTime != DateTime.MinValue)
                            {
                                var now = DateTime.UtcNow;
                                var minutesDiff = (arrivalTime - now).TotalMinutes;

                                if (minutesDiff >= 0 && minutesDiff <= maxTimeToGo)
                                {
                                    //Debug.Log($"Arrival time (≤2 min): {arrivalTime:HH:mm:ss}");
                                }
                                else
                                {
                                    //Debug.Log($"Next connection too late: {arrivalTime:HH:mm:ss}");
                                }
                            }
                            else
                            {
                                //Debug.Log("No arrival time data received.");
                            }
                        }
                    ));
                }
            }
            else
            {
                //Debug.Log($"Not in the range of station. Distance: {distanceToStation:F2} m");

                if (isMoving)
                {
                    isMoving = false;
                    OnIsMovingChanged?.Invoke(isMoving, originStation, destinationStation);
                }
            }

            //Debug.Log($"Is moving: {isMoving}");
            //Debug.Log($"Current time: {GetCurrentDateTime()}");

            yield return new WaitForSeconds(loop_delay);
        }
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
                       $"&routingDateTime={routingDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}" +
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
            //Debug.LogError($"❌ Error HTTP: {request.responseCode} - {request.error}");
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
                if (!string.IsNullOrEmpty(firstPart.to.plannedDeparture))
                {
                    if (DateTime.TryParse(firstPart.to.plannedDeparture, out DateTime arrival))
                    {
                        return arrival;
                    }
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

        public override string ToString()
        {
            string linesStr = connections != null ? string.Join(", ", connections) : "no connections";
            return $"{stationName} ({lat}, {@long}) - connections: {linesStr}";
        }
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
            Station nearest = null;
            float minDistance = float.MaxValue;

            foreach (var station in stations)
            {
                float dist = HaversineDistance(currentLat, currentLon, station.lat, station.@long);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = station;
                }
            }
            return nearest;
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
        public float longitude;
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
