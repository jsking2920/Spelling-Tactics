using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unity.VisualScripting.Member;

// Based on: https://www.youtube.com/watch?v=2xXLhbQnHV4
public class TileMap : MonoBehaviour
{
    // Array of all Tile Type SOs, only used to initialize the dictionary below
    [SerializeField] private TileTypeScriptableObject[] tileTypeObjects;
    // CSV file in Assets/Resources. Needs to be rectangular. Values should be ints that correspond to TileType enum
    public string mapFileName;

    public Dictionary<TileType, TileTypeScriptableObject> tileTypes;
    private Tile[,] tiles; // will contain null values for empty spaces in grid

    [SerializeField] private Transform tileParent;
    public Transform unitParent;

    private List<Tile> currentSelectedPath = null;

    private void Start()
    {
        InitializeTileTypeDict();
        InstantiateMap();
        InitializeTileNeighbors();
    }

    #region Map Initialization
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
                    GameObject newTileObject = Instantiate(tileTypes[mapData[x, y]].tilePrefab, GetWorldPosFromTileCoord(x, y), Quaternion.identity, tileParent);
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

    // Initializes list of neighbors for each tile for pathfinding. Currently set up for orthogonal movement
    private void InitializeTileNeighbors()
    {
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                if (tiles[x, y] != null)
                {
                    tiles[x, y].neighbors = new List<Tile>();

                    if (x > 0 && tiles[x - 1, y] != null)
                        tiles[x, y].neighbors.Add(tiles[x - 1, y]);
                    if (y > 0 && tiles[x, y - 1] != null)
                        tiles[x, y].neighbors.Add(tiles[x, y - 1]);
                    if (x < tiles.GetLength(0) - 1 && tiles[x + 1, y] != null)
                        tiles[x, y].neighbors.Add(tiles[x + 1, y]);
                    if (y < tiles.GetLength(1) - 1 && tiles[x, y + 1] != null)
                        tiles[x, y].neighbors.Add(tiles[x, y + 1]);
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
    #endregion

    #region Pathfinding
    // Simple, unoptimized Dijkstra's pathfinding
    // TODO: Convert to A*
    public List<Tile> FindPath(Tile source, Tile target)
    {
        Dictionary<Tile, float> dist = new Dictionary<Tile, float>(); // Distance to each tile from source
        Dictionary<Tile, Tile> prev = new Dictionary<Tile, Tile>(); // Used to trace current path being tested

        List<Tile> unvisited = new List<Tile>();

        // Initially consider all tiles univisited and infinitely far away
        foreach (Tile tile in tiles)
        {
            if (tile != null)
            {
                unvisited.Add(tile);
                dist[tile] = Mathf.Infinity;
                prev[tile] = null;
            }
        }

        dist[source] = 0;

        while (unvisited.Count > 0)
        {
            // Find current closest unvisited tile
            Tile closestTile = null;
            foreach (Tile tile in unvisited)
            {
                if (closestTile == null || dist[tile] < dist[closestTile])
                {
                    closestTile = tile;
                }
            }

            // Found path to target
            if (closestTile == target) break;

            unvisited.Remove(closestTile);

            // Look at each of that tiles neighbors 
            foreach (Tile tile in closestTile.neighbors)
            {
                // Using manhattan distance because we're only allowing orthogonal movement. Could use euclidean distance or Chebyshev distance otherwise
                float d = dist[closestTile] + ManhattanDistance(tile, closestTile);

                if (d < dist[tile])
                {
                    dist[tile] = d;
                    prev[tile] = closestTile;
                }
            }
        }

        // No possible path from source to target
        if (prev[target] == null)
        {
            return null;
        }

        List<Tile> path = new List<Tile>();

        Tile cur = target;
        while (cur != null)
        {
            path.Add(cur);
            cur = prev[cur];
        }
        path.Reverse();

        return path;
    }

    private int ManhattanDistance(int x1, int y1, int x2, int y2)
    {
        return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);
    }

    private int ManhattanDistance(Tile source, Tile target)
    {
        return ManhattanDistance(source.tileX, source.tileY, target.tileX, target.tileY);
    }

    private int ManhattanDistance(Unit source, Tile target)
    {
        return ManhattanDistance(source.tileX, source.tileY, target.tileX, target.tileY);
    }
    #endregion

    public Vector3 GetWorldPosFromTileCoord(int tileX, int tileY)
    {
        return new Vector3(tileX, 0, tileY);
    }

    public void OnTileClicked(Tile tile)
    {
        if (UnitManager.Instance.selectedUnit != null)
        {
            if (currentSelectedPath != null && tile == currentSelectedPath[currentSelectedPath.Count - 1])
            {
                UnitManager.Instance.MoveSelectedUnit(currentSelectedPath);
            }
            else
            {
                Tile source = tiles[UnitManager.Instance.selectedUnit.tileX, UnitManager.Instance.selectedUnit.tileY];

                if (ManhattanDistance(source, tile) <= UnitManager.Instance.selectedUnit.movement)
                {
                    List<Tile> path = FindPath(source, tile);

                    if (path != null)
                    {
                        OnPathSelected(path);
                    }
                }
            }
        }
    }

    public void OnPathSelected(List<Tile> path)
    {
        if (currentSelectedPath != null)
        {
            foreach (Tile t in currentSelectedPath)
            {
                int dist = ManhattanDistance(UnitManager.Instance.selectedUnit, t);
                if (dist <= UnitManager.Instance.selectedUnit.movement && dist > 0)
                {
                    t.SetHighlight(Tile.HighlightState.Light);
                }
                else
                {
                    t.SetHighlight(Tile.HighlightState.None);
                }
            }
        }

        foreach (Tile t in path)
        {
            t.SetHighlight(Tile.HighlightState.Dark);
        }
        path[0].SetHighlight(Tile.HighlightState.None);
        currentSelectedPath = path;
    }

    public void OnUnitSelected(Unit unit)
    {
        SetHighlightInRangeOfUnit(unit, Tile.HighlightState.Light);
    }

    public void OnUnitDeselected(Unit unit)
    {
        currentSelectedPath = null;
        SetHighlightInRangeOfUnit(unit, Tile.HighlightState.None);
    }

    public void SetHighlightInRangeOfUnit(Unit unit, Tile.HighlightState state)
    {
        Tile source = tiles[unit.tileX, unit.tileY];

        // TODO: There's a better way of doing this
        foreach (Tile tile in tiles)
        {
            if (tile != null)
            {
                if (tileTypes[tile.tileType].isTraversable && ManhattanDistance(source, tile) <= unit.movement)
                {
                    tile.SetHighlight(state);
                }
            }
        }
        source.SetHighlight(Tile.HighlightState.None);
    }
}
