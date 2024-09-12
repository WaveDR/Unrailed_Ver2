using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapData 
{
    public string mapDataName;
    public int mapId;
    public MyArr<int>[] mapData;

    public Vector2Int[] creaturePos = new Vector2Int[4];

    public MapData(string mapDataName, int mapId, MyArr<int>[] mapData)
    {
        this.creaturePos = new Vector2Int[4];

        this.mapDataName = mapDataName;
        this.mapId = mapId;
        this.mapData = mapData;
    }
}


[System.Serializable]
public class MapsData
{
    public List<MapData> mapsData = new List<MapData>();

    public void Add(MapData mapData)
    {
        mapsData.Add(mapData);
    }
}
