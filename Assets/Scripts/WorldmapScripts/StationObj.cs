using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Station", menuName = "Scriptable Objects/Station")]

public class StationObj : ScriptableObject
{
    public string id = "";
    public string stationName = "";
    public string description = "";
    public List<StationObj> neigbours = new List<StationObj>();

    public List<UBahnLine> uBahnLines = new List<UBahnLine>();

    public double lon;
    public double lat;

}
