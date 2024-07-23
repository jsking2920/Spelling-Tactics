using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Based on: https://www.youtube.com/watch?v=2xXLhbQnHV4
public class TileMap : MonoBehaviour
{
    [SerializeField] private TileType[] tileTypes;
    private int[,] tiles; // each int correlates to one of the tile types in above array

    private int mapSizeX = 10;
    private int mapSizeY = 10;

    private void Start()
    {
        InstantiateTiles();
    }

    private void InstantiateTiles()
    {
        tiles = new int[10, 10];

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                tiles[x, y] = 0;
                Instantiate(tileTypes[tiles[x, y]].tilePrefab, new Vector3(x, 0, y), Quaternion.identity, transform);
            }
        }
    }
}
