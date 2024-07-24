using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

    [SerializeField] private TileMap tileMap;

    public Unit selectedUnit { get; private set; } = null;

    public bool unitSelectable = true;

    public List<Unit> friendlyUnits;
    public List<Unit> enemyUnits;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnUnitsForMap(Map map)
    {
        for (int i = 0; i < map.unitSpawns.Length; i++)
        {
            Unit newUnit = SpawnUnit(map.unitPrefabs[i], map.unitSpawns[i].x, map.unitSpawns[i].y);
            tileMap.tiles[map.unitSpawns[i].x, map.unitSpawns[i].y].occupyingUnit = newUnit;
            if (newUnit.isEnemy) enemyUnits.Add(newUnit);
            else friendlyUnits.Add(newUnit);
        }
    }

    private Unit SpawnUnit(GameObject unitPrefab, int tileX, int tileY)
    {
        Unit newUnit = Instantiate(unitPrefab, tileMap.GetWorldPosFromTileCoord(tileX, tileY), Quaternion.identity, tileMap.unitParent).GetComponent<Unit>();
        newUnit.tileX = tileX;
        newUnit.tileY = tileY;
        return newUnit;
    }

    public void OnUnitSelected(Unit unit)
    {
        if (selectedUnit != null)
        {
            OnUnitDeselect();
        }
        UIManager.Instance.SetSelectedUnitInfo(unit);
        tileMap.OnUnitSelected(unit);
        selectedUnit = unit;
        unit.isSelected = true;
    }

    public void OnUnitDeselect()
    {
        UIManager.Instance.ClearSelectedUnitInfo();
        tileMap.OnUnitDeselected(selectedUnit);
        selectedUnit.isSelected = false;
        selectedUnit = null;
    }

    public void MoveSelectedUnit(List<Tile> path)
    {
        StartCoroutine(Co_MoveUnit(path, selectedUnit));
    }
    private IEnumerator Co_MoveUnit(List<Tile> path, Unit unit)
    {
        unitSelectable = false;
        OnUnitDeselect();

        Tile target = path[path.Count - 1];

        unit.transform.position = tileMap.GetWorldPosFromTileCoord(target.tileX, target.tileY);

        path[0].occupyingUnit = null;
        target.occupyingUnit = unit;
        unit.tileX = target.tileX;
        unit.tileY = target.tileY;

        yield return null;

        unitSelectable = true;
    }
}
