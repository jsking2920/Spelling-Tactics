using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

    [SerializeField] private TileMap tileMap;

    public Unit selectedUnit { get; private set; } = null;
    [SerializeField] GameObject unitPrefab;

    public bool unitSelectable = true;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnUnit(3, 6);
    }

    private void SpawnUnit(int tileX, int tileY)
    {
        Unit newUnit = Instantiate(unitPrefab, tileMap.GetWorldPosFromTileCoord(tileX, tileY), Quaternion.identity, tileMap.unitParent).GetComponent<Unit>();
        newUnit.tileX = tileX;
        newUnit.tileY = tileY;
    }

    public void OnUnitSelected(Unit unit)
    {
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
        unit.tileX = target.tileX;
        unit.tileY = target.tileY;
        yield return null;
        unitSelectable = true;
    }
}
