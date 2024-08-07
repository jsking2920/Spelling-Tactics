using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

    [SerializeField] private TileMap tileMap;

    public Unit selectedUnit { get; private set; } = null;

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
            Tile spawnTile = tileMap.tiles[map.unitSpawns[i].x, map.unitSpawns[i].y];
            if (spawnTile == null || !tileMap.tileTypes[spawnTile.tileType].isTraversable || spawnTile.occupyingUnit != null)
            {
                Debug.LogError("Bad spawn location!");
            }

            Unit newUnit = SpawnUnit(map.unitPrefabs[i], map.unitSpawns[i].x, map.unitSpawns[i].y);
            spawnTile.occupyingUnit = newUnit;
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
        if (!GameManager.Instance.IsUnitActive(unit))
        {
            return;
        }

        if (selectedUnit != null)
        {
            if (unit.isEnemy)
            {
                if (tileMap.IsAdjacent(unit.tileX, unit.tileY, selectedUnit.tileX, selectedUnit.tileY))
                {
                    // Player has a friendly active unit selected, and they just clicked on an adjacent active enemy unit
                    OnUnitAttack(selectedUnit, unit);
                    return;
                }
                else
                {
                    return;
                }
            }
            else
            {
                // Player clicked on a non selected friendly active unit, select it instead after deselecting current unit
                OnUnitDeselect();
            }
        }

        if (!unit.isEnemy)
        {
            UIManager.Instance.SetSelectedUnitInfo(unit);
            tileMap.OnUnitSelected(unit);
            selectedUnit = unit;
            unit.isSelected = true;
        }
    }

    public void OnUnitDeselect()
    {
        UIManager.Instance.ClearSelectedUnitInfo();
        tileMap.OnUnitDeselected(selectedUnit);
        selectedUnit.isSelected = false;
        selectedUnit = null;
    }

    public void OnUnitAttack(Unit attacker, Unit defender)
    {
        attacker.OnAttack();
        tileMap.ClearTileHighlights();

        // defender takes damage == attacker.baseAttack * # of defenders letters in active word
        int multiplier = 0;
        if (defender.isEnemy)
            multiplier = GameManager.Instance.activeEnemyUnits[defender];
        else
            multiplier = GameManager.Instance.activeFriendlyUnits[defender];

        // TODO: Add better cleanup logic and game state handling for damage and death
        defender.currentHP -= attacker.baseAttack * multiplier;
        if (defender.currentHP <= 0)
        {
            if (defender.isEnemy)
                GameManager.Instance.activeEnemyUnits.Remove(defender);
            else
                GameManager.Instance.activeFriendlyUnits.Remove(defender);
            tileMap.tiles[defender.tileX, defender.tileY].occupyingUnit = null;
            Destroy(defender.gameObject);
        }
    }

    public void OnNewRound()
    {
        if (selectedUnit != null)
        {
            OnUnitDeselect();
        }
    }

    public void TakeEnemyTurns()
    {
        foreach (Unit u in GameManager.Instance.activeEnemyUnits.Keys)
        {
            Unit target = GetClosestUnit(u, GameManager.Instance.activeFriendlyUnits);
            if (target == null) continue;

            Vector2Int curCoords = new Vector2Int(u.tileX, u.tileY);
            Vector2Int targetTilecoord1 = new Vector2Int(target.tileX-1, target.tileY);
            Vector2Int targetTilecoord2 = new Vector2Int(target.tileX+1, target.tileY);
            Vector2Int targetTilecoord3 = new Vector2Int(target.tileX, target.tileY-1);
            Vector2Int targetTilecoord4 = new Vector2Int(target.tileX, target.tileY+1);

            if (curCoords == targetTilecoord1 || curCoords == targetTilecoord2 || curCoords == targetTilecoord3 || curCoords == targetTilecoord4)
            {
                OnUnitAttack(u, target);
                continue;
            }

            if (CanReachTile(u, targetTilecoord1))
            {
                MoveEnemyUnit(u, tileMap.FindPath(u, tileMap.tiles[targetTilecoord1.x, targetTilecoord1.y]));
                OnUnitAttack(u, target);
            }
            else if (CanReachTile(u, targetTilecoord2))
            {
                MoveEnemyUnit(u, tileMap.FindPath(u, tileMap.tiles[targetTilecoord2.x, targetTilecoord2.y]));
                OnUnitAttack(u, target);
            }
            else if (CanReachTile(u, targetTilecoord3))
            {
                MoveEnemyUnit(u, tileMap.FindPath(u, tileMap.tiles[targetTilecoord3.x, targetTilecoord3.y]));
                OnUnitAttack(u, target);
            }
            else if (CanReachTile(u, targetTilecoord4))
            {
                MoveEnemyUnit(u, tileMap.FindPath(u, tileMap.tiles[targetTilecoord4.x, targetTilecoord4.y]));
                OnUnitAttack(u, target);
            }
        }
    }

    private Unit GetClosestUnit(Unit source, Dictionary<Unit, int> targets)
    {
        int closestDistance = 100000;
        Unit closestUnit = null;

        foreach (Unit target in targets.Keys)
        {
            if (tileMap.ManhattanDistance(source, target) < closestDistance)
            {
                closestDistance = tileMap.ManhattanDistance(source, target);
                closestUnit = target;
            }
        }

        return closestUnit;
    }

    private bool CanReachTile(Unit source, Vector2Int tileCoords)
    {
        if (tileMap.tiles[tileCoords.x, tileCoords.y] != null && tileMap.FindPath(source, tileMap.tiles[tileCoords.x, tileCoords.y]) != null)
        {
            return true;
        }
        return false;
    }

    public void MoveSelectedUnit(List<Tile> path)
    {
        selectedUnit.OnMove();
        StartCoroutine(Co_MoveUnit(path, selectedUnit));
    }

    private IEnumerator Co_MoveUnit(List<Tile> path, Unit unit)
    {
        //TODO: Could put animation here but would have to make sure you can't mess with things while a unit is moving
        OnUnitDeselect();

        Tile target = path[path.Count - 1];

        unit.transform.position = tileMap.GetWorldPosFromTileCoord(target.tileX, target.tileY);

        path[0].occupyingUnit = null;
        target.occupyingUnit = unit;
        unit.tileX = target.tileX;
        unit.tileY = target.tileY;

        yield return null;
    }

    private void MoveEnemyUnit(Unit unit, List<Tile> path)
    {
        Tile target = path[path.Count - 1];

        unit.transform.position = tileMap.GetWorldPosFromTileCoord(target.tileX, target.tileY);

        path[0].occupyingUnit = null;
        target.occupyingUnit = unit;
        unit.tileX = target.tileX;
        unit.tileY = target.tileY;
    }

    public Dictionary<Unit, int> GetUnitsWithinWord(string s)
    {
        string word = s.ToUpper();

        Dictionary<string, int> letterCount = new Dictionary<string, int>();
        for (int i = 0; i < word.Length; i++)
        {
            string c = word.Substring(i, 1);
            if (letterCount.ContainsKey(c))
            {
                letterCount[c] += 1;
            }
            else
            {
                letterCount[c] = 1;
            }
        }
        for (int i = 1; i < word.Length; i++)
        {
            string doubleLetter = word.Substring(i - 1, 2);
            if (letterCount.ContainsKey(doubleLetter))
            {
                letterCount[doubleLetter] += 1;
            }
            else
            {
                letterCount[doubleLetter] = 1;
            }
        }

        Dictionary<Unit, int> units = new Dictionary<Unit, int>();

        foreach (Unit unit in friendlyUnits)
        {
            if (letterCount.ContainsKey(unit.letter))
            {
                units[unit] = letterCount[unit.letter];
            }
        }
        foreach (Unit unit in enemyUnits)
        {
            if (letterCount.ContainsKey(unit.letter))
            {
                units[unit] = letterCount[unit.letter];
            }
        }

        return units;
    }
}
