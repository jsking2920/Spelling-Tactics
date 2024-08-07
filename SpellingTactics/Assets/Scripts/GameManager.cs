using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    WordEntry,
    PlayerActions,
    EnemyAction,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [HideInInspector] public GameState state = GameState.WordEntry;

    [HideInInspector] public Dictionary<Unit, int> activeFriendlyUnits = null;
    [HideInInspector] public Dictionary<Unit, int> activeEnemyUnits = null;
    [HideInInspector] public string activeWord = "";

    [HideInInspector] public List<string> usedWords;

    private void Awake()
    {
        Instance = this;

        usedWords = new List<string>();
    }

    public void OnValidWordSubmitted(string word)
    {
        activeEnemyUnits = new Dictionary<Unit, int>();
        activeFriendlyUnits = new Dictionary<Unit, int>();

        Dictionary<Unit, int> activeUnits = UnitManager.Instance.GetUnitsWithinWord(word);

        foreach (Unit u in activeUnits.Keys)
        {
            if (u.isEnemy)
            {
                activeEnemyUnits[u] = activeUnits[u];
            }
            else
            {
                activeFriendlyUnits[u] = activeUnits[u];
            }
            u.movement = activeUnits[u] + u.baseMovement;
        }

        activeWord = word;
        usedWords.Add(word);
        state = GameState.PlayerActions;

        foreach (Unit u in activeUnits.Keys)
        {
            u.SetActiveHighlight(true);
        }
    }

    public void OnNewRound()
    {
        state = GameState.EnemyAction;
        UnitManager.Instance.TakeEnemyTurns();
        
        state = GameState.WordEntry;

        UnitManager.Instance.OnNewRound();

        foreach (Unit u in activeFriendlyUnits.Keys)
        {
            u.OnNewRound();
        }
        foreach (Unit u in activeEnemyUnits.Keys)
        {
            u.OnNewRound();
        }

        activeFriendlyUnits = null;
        activeEnemyUnits = null;
        activeWord = "";
    }

    public bool IsUnitActive(Unit u)
    {
        return activeEnemyUnits.ContainsKey(u) || activeFriendlyUnits.ContainsKey(u);
    }
}
