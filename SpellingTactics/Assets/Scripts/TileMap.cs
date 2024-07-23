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

        // Initialize every tile as grass
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                tiles[x, y] = 1;
            }
        }

        // TODO: set up a system to build maps from text files
        tiles[0, 0] = 0;
        tiles[0, 1] = 0;
        tiles[0, 2] = 0;
        tiles[0, 4] = 0;
        tiles[1, 4] = 0;
        tiles[0, 5] = 0;
        tiles[1, 4] = 0;
        tiles[0, 6] = 0;
        tiles[1, 6] = 0;
        tiles[0, 7] = 0;

        tiles[4, 0] = 3;
        tiles[4, 1] = 3;
        tiles[4, 2] = 3;
        tiles[4, 3] = 3;
        tiles[4, 4] = 3;
        tiles[4, 5] = 3;
        tiles[4, 6] = 3;
        tiles[4, 7] = 3;

        tiles[7, 7] = 2;
        tiles[8, 7] = 2;
        tiles[7, 8] = 2;
        tiles[8, 8] = 2;

        // Initialize every tile as empty
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                Instantiate(tileTypes[tiles[x, y]].tilePrefab, new Vector3(x, 0, y), Quaternion.identity, transform);
            }
        }
    }
}
