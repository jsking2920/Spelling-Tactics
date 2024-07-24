using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Based on: https://www.youtube.com/watch?v=2xXLhbQnHV4
public class TileMap : MonoBehaviour
{
    // Array of all Tile Type SOs, only used to initialize the dictionary below
    [SerializeField] private TileTypeScriptableObject[] tileTypeObjects;
    // CSV file in Assets/Resources. Needs to be rectangular. Values should be ints that correspond to TileType enum
    public string mapFileName;

    public Dictionary<TileType, TileTypeScriptableObject> tileTypes;
    private Tile[,] tiles;

    private void Start()
    {
        InitializeTileTypeDict();
        InstantiateMap();
    }

    private void InitializeTileTypeDict()
    {
        tileTypes = new Dictionary<TileType, TileTypeScriptableObject>();
        foreach (TileTypeScriptableObject tile in tileTypeObjects)
        {
            tileTypes[tile.tileType] = tile;
        }
        tileTypes[TileType.Empty] = null;
    }

    private void InstantiateMap()
    {
        tiles = new Tile[10, 10];

        TileType[,] mapData = ReadMapCSV();

        for (int x = 0; x < mapData.GetLength(0); x++)
        {
            for (int y = 0; y < mapData.GetLength(1); y++)
            {
                if (mapData[x, y] != TileType.Empty)
                {
                    GameObject newTileObject = Instantiate(tileTypes[mapData[x, y]].tilePrefab, new Vector3(x, 0, y), Quaternion.identity, transform);
                    Tile newTile = newTileObject.GetComponent<Tile>();
                    newTile.tileType = mapData[x, y];
                    newTile.map = this;
                    newTile.tileX = x;
                    newTile.tileY = y;
                    tiles[x, y] = newTile;
                }
                else
                {
                    tiles[x, y] = null;
                }
            }
        }
    }

    private TileType[,] ReadMapCSV()
    {
        List<string[]> csvData = new List<string[]>();
        
        TextAsset csv = Resources.Load<TextAsset>(mapFileName);

        string[] rows = csv.text.Split(new char[] { '\n' });

        foreach (string row in rows)
        {
            string[] values = row.Split(new char[] { ',' });
            csvData.Add(values);
        }

        // Assumes csv data is rectangular in shape
        TileType[,] mapData = new TileType[csvData.Count, csvData[0].Length];

        for (int x = 0; x < csvData.Count; x++)
        {
            for (int y = 0; y < csvData[0].Length; y++)
            {
                mapData[x, y] = (TileType)int.Parse(csvData[x][y]);
            }
        }

        return mapData;
    }
}
