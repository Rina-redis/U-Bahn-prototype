using System;
using System.Collections.Generic;
using System.Text;

[Serializable]
public class Station
{
    public string Id;
    public string StationName;
    public string Description { get; }
    public float Wealth;
    public List<Station> Neighbours { get; }
    public List<UBahnLine> uBahnLines { get; }

    public int StationTier = 0;

    public double lon = 0;

    public double lat = 0;
    public Station(StationObj obj)
    {
        Neighbours = new List<Station>();
        Id = obj.id;
        StationName = obj.stationName;
        Description = obj.description;
        uBahnLines = obj.uBahnLines;
        lon = obj.lon;
        lat = obj.lat;
    }

    public float SendGoods()
    {
        return Wealth / 10;
    }

    public void RecieveGoods(float recievedGoods)
    {
        Wealth += recievedGoods;
    }

    public void LoadData(float w, float[] tierReqs)
    {
        Wealth = w;
        for (int i = 0; i < tierReqs.Length; ++i)
        {
            StationTier = i;
            if (Wealth < tierReqs[i])
            {
                StationTier = i - 1;
                break;
            }
        }
            Wealth = w;
    }

    public override string ToString()
    {
        StringBuilder str = new StringBuilder();
        str.Append("Station: ").Append(StationName).Append("\n    Description: ").Append(Description).Append("\n    Wealth: ").Append(Wealth).Append("\n    Neighbours: \n");
        foreach (Station station in Neighbours)
        {
            str.Append("        ").Append(station.StationName).Append("\n").Append("\n");
        }
        return str.ToString();
    }

}
