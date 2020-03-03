using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;



public class MapManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap[] maps;
    [SerializeField]
    TileBase[] tiles = null;


    public void SetMap(string mapCode, (int x, int y) offset)
    {
        var layerCodes = mapCode.Split(new char[1] { '@' }, StringSplitOptions.RemoveEmptyEntries);

        for (var k = 0; k < layerCodes.Length; k++)
        {
            var mapInfo = ParseMapcode(layerCodes[k]);

            for (var i = 0; i < mapInfo.GetLength(0); i++)
            {
                for (var j = 0; j < mapInfo.GetLength(1); j++)
                {
                    if (mapInfo[i, j] == -1) continue;
                    maps[k].SetTile(new Vector3Int(i + offset.x, j + offset.y, 0), tiles[mapInfo[i, j]]);
                }
            }
        }
    }

    public string CreateCode(Tilemap data, (int startX, int startY, int endX, int endY) range)
    {
        var output = $"{range.endX - range.startX}x{range.endY - range.startY}?";

        for (var i = range.startX; i <= range.endX; i++) {
            for (var j = range.startY; j <= range.endY; j++)
            {
                output += Array.IndexOf(tiles, data.GetTile(new Vector3Int(i, j, 0))) + 1 + "&";
            }
        }

        return output;
    }

    public Vector3[] GetPlayerSpawnPositions(TileLayer layer)
    {
        (int startX, int startY, int endX, int endY) range = (0,0,maps[0].size.x,maps[0].size.y);

        var list = new List<Vector3>();
        var data = maps[(int)layer];
        for (var i = range.startX; i <= range.endX; i++)
        {
            for(var j = range.startY; j <= range.endY; j++)
            {
                var tilePos = new Vector3Int(i, j, 0);
                var tileIndex = Array.IndexOf(tiles, data.GetTile(tilePos));
                
                if (tileIndex == 62 || tileIndex == 65)
                {
                    data.SetTile(tilePos, null);
                    list.Add(data.CellToWorld(tilePos));
                }
            }
        }

        return list.ToArray();
    }

    private int[,] ParseMapcode(string mapCode)
    {
        var mapSizeString = mapCode.Substring(0, mapCode.IndexOf('?'));
        var mapSizeX = int.Parse(mapSizeString.Substring(0, mapSizeString.IndexOf('x'))) + 1;
        var mapSizeY = int.Parse(mapSizeString.Substring(mapSizeString.IndexOf('x') + 1, mapSizeString.Length - mapSizeString.IndexOf('x') - 1).ToString()) + 1;
        var mapObjects = mapCode.Split(new char[2] { '&', '?' }, System.StringSplitOptions.RemoveEmptyEntries);
        var output = new int[mapSizeX, mapSizeY];

        for (var i = 1; i < mapObjects.Length; i++)
        {
            try
            {
                output[(i - 1) / mapSizeY, (i - 1) % mapSizeY] = int.Parse(mapObjects[i]) - 1;
            }
            catch (Exception)
            {
                Debug.Log(i - 1);
                Debug.Log(mapObjects.Length);
            }
        }

        return output;
    }
}

public static class MapUtil
{
    public static Vector3 ToWorld(this Vector3Int tilePos)
    {
        return (Vector3)tilePos * 0.56f - new Vector3(8.61f, 4.8f);
    }
}



public enum TileLayer
{
    Wall,
    Land,
    Deco,
    Obstacle
}