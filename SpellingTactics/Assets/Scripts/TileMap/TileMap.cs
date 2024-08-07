using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Based on: https://www.youtube.com/watch?v=2xXLhbQnHV4
public class TileMap : MonoBehaviour
{
    // Array of all Tile Type SOs, only used to initialize the dictionary below
    [SerializeField] private TileTypeScriptableObject[] tileTypeObjects;
    // CSV file in Assets/Resources. Needs to be rectangular. Values should be ints that correspond to TileType enum
    [SerializeField] private Map currentMap;

    public Dictionary<TileType, TileTypeScriptableObject> tileTypes;
    [HideInInspector] public Tile[,] tiles; // will contain null values for empty spaces in grid

    [SerializeField] private Transform tileParent;
    public Transform unitParent;

    private List<Tile> currentSelectedPath = null;

    private void Start()
    {
        InitializeTileTypeDict();
        InstantiateMap();
        InitializeTileNeighbors();
        UnitManager.Instance.SpawnUnitsForMap(currentMap);
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
        
        TextAsset csv = Resources.Load<TextAsset>(currentMap.csvFileName);

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
    public List<Tile> FindPath(Unit unit, Tile target)
    {
        if (unit == null || target == null) return null;
        if (tiles[unit.tileX, unit.tileY] == target) return null;
        if (unit.movement == 0) return null;
        if (!tileTypes[target.tileType].isTraversable || target.occupyingUnit != null) return null;

        Tile source = tiles[unit.tileX, unit.tileY];

        Dictionary<Tile, float> dist = new Dictionary<Tile, float>(); // Distance to each tile from source
        Dictionary<Tile, Tile> prev = new Dictionary<Tile, Tile>(); // Used to trace current path being tested

        List<Tile> unvisited = new List<Tile>();

        // TODO: I think we can narrow down what tiles we look at here (in range, not occupied, traversable, etc)
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
            // TODO: Can we narrow down which tiles we actually consider here?
            foreach (Tile tile in closestTile.neighbors)
            {
                if (tile != null)
                {
                    // Using manhattan distance because we're only allowing orthogonal movement. Could use euclidean distance or Chebyshev distance otherwise
                    float d = dist[closestTile] + CostToEnterTile(unit, tile);

                    if (d < dist[tile])
                    {
                        dist[tile] = d;
                        prev[tile] = closestTile;
                    }
                }
            }
        }

        // No possible path from source to target
        if (prev[target] == null || dist[target] > unit.movement)
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

    public int ManhattanDistance(int x1, int y1, int x2, int y2)
    {
        return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);
    }

    public int ManhattanDistance(Tile source, Tile target)
    {
        return ManhattanDistance(source.tileX, source.tileY, target.tileX, target.tileY);
    }

    public int ManhattanDistance(Unit source, Tile target)
    {
        return ManhattanDistance(source.tileX, source.tileY, target.tileX, target.tileY);
    }

    public int ManhattanDistance(Unit source, Unit target)
    {
        return ManhattanDistance(source.tileX, source.tileY, target.tileX, target.tileY);
    }

    private int CostToEnterTile(Unit unit, Tile tile)
    {
        bool canMoveThrough = true;

        if ((tile.occupyingUnit != null && tile.occupyingUnit.isEnemy != unit.isEnemy) || !tileTypes[tile.tileType].isTraversable)
        {
            canMoveThrough = false;
        }

        if (canMoveThrough)
        {
            return tileTypes[tile.tileType].traversalCost;
        }
        else
        {
            return 100000;
        }
    }
    #endregion

    public Vector3 GetWorldPosFromTileCoord(int tileX, int tileY)
    {
        return new Vector3(tileX, 0, tileY);
    }

    public bool IsAdjacent(int x1, int y1, int x2, int y2)
    {
        if (x1 == x2 && Mathf.Abs(y1 - y2) == 1)
        {
            return true;
        }
        else if (y1 == y2 && Mathf.Abs(x1 - x2) == 1)
        {
            return true;
        }
        return false;
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
                if (UnitManager.Instance.selectedUnit.hasMoved) return;

                Tile source = tiles[UnitManager.Instance.selectedUnit.tileX, UnitManager.Instance.selectedUnit.tileY];

                if (tileTypes[tile.tileType].isTraversable && tile.occupyingUnit == null && ManhattanDistance(source, tile) <= UnitManager.Instance.selectedUnit.movement)
                {
                    List<Tile> path = FindPath(UnitManager.Instance.selectedUnit, tile);

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
                if (FindPath(UnitManager.Instance.selectedUnit, t) != null)
                {
                    if (!UnitManager.Instance.selectedUnit.hasAttacked && ManhattanDistance(path[0], t) == 1)
                    {
                        t.SetHighlight(Tile.HighlightState.Attack);
                    }
                    else
                    {
                        t.SetHighlight(Tile.HighlightState.Light);
                    }
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
        SetHighlightInRangeOfUnit(unit);
    }

    public void OnUnitDeselected(Unit unit)
    {
        if (currentSelectedPath != null)
        {
            foreach (Tile t in currentSelectedPath)
            {
                t.SetHighlight(Tile.HighlightState.None);
            }
        }
        currentSelectedPath = null;
        ClearTileHighlights();
    }

    public void SetHighlightInRangeOfUnit(Unit unit)
    {
        if (!unit.hasMoved)
        {
            // TODO: There's a better way of doing this
            foreach (Tile tile in tiles)
            {
                if (tile != null)
                {
                    if (FindPath(unit, tile) != null)
                    {
                        tile.SetHighlight(Tile.HighlightState.Light);
                    }
                }
            }
        }
        if (!unit.hasAttacked)
        {
            tiles[unit.tileX - 1, unit.tileY]?.SetHighlight(Tile.HighlightState.Attack);
            tiles[unit.tileX + 1, unit.tileY]?.SetHighlight(Tile.HighlightState.Attack);
            tiles[unit.tileX, unit.tileY - 1]?.SetHighlight(Tile.HighlightState.Attack);
            tiles[unit.tileX, unit.tileY + 1]?.SetHighlight(Tile.HighlightState.Attack);
        }
    }

    public void ClearTileHighlights()
    {
        foreach (Tile tile in tiles)
        {
            if (tile != null)
            {
                tile.SetHighlight(Tile.HighlightState.None);
            }
        }
    }
}
