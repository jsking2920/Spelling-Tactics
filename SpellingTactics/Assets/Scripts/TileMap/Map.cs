using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Map
{
    public string csvFileName = "Map";
    public GameObject[] unitPrefabs;
    public Vector2Int[] unitSpawns;
}
