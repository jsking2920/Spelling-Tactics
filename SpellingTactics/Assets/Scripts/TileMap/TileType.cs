using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// To create a new type of tile:
// 1. Add to enum below
// 2. Create a new prefab variant of "GenericTile"
// 3. Create a new SO in GameData/Tiles
// 4. Add that SO to list in TileMap game object

[CreateAssetMenu(fileName = "TyleTypeSO", menuName = "ScriptableObjects/TileTypeScriptableObject", order = 1)]
public class TileTypeScriptableObject : ScriptableObject
{
    public TileType tileType;
    public string tileName;
    public GameObject tilePrefab;
    public bool isTraversable;
    public int traversalCost;
}

public enum TileType
{
    Empty = 0,
    Grass = 1,
    Mountain = 2,
    Water = 3,
    Hills = 4
}
