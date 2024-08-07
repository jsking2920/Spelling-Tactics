using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public string unitName = "Asp";
    public string letter = "A";
    [HideInInspector] public int tileX;
    [HideInInspector] public int tileY;
    public int baseMovement = 1;
    [HideInInspector]public int movement = 0;
    public bool isEnemy = false;
    [HideInInspector] public bool hasMoved = false;
    [HideInInspector] public bool hasAttacked = false;

    [HideInInspector] public bool isSelected = false; // set by unit manager

    [SerializeField] private GameObject activeFriendlyHighlight;
    [SerializeField] private GameObject activeEnemyHighlight;

    [HideInInspector] public int currentHP = 8;
    public int maxHP = 8;
    public int baseAttack = 1;

    private void Start()
    {
        movement = 0;
        currentHP = maxHP;
    }

    public void SetActiveHighlight(bool b)
    {
        if (b)
        {
            if (isEnemy)
            {
                activeEnemyHighlight.SetActive(true);
                activeFriendlyHighlight.SetActive(false);
            }
            else
            {
                activeEnemyHighlight.SetActive(false);
                activeFriendlyHighlight.SetActive(true);
            }
        }
        else
        {
            activeEnemyHighlight.SetActive(false);
            activeFriendlyHighlight.SetActive(false);
        }
    }

    public void OnMove()
    {
        hasMoved = true;

        if (hasAttacked)
        {
            SetActiveHighlight(false);
        }
    }

    public void OnAttack()
    {
        hasAttacked = true;

        if (hasMoved)
        {
            SetActiveHighlight(false);
        }
    }

    public void OnNewRound()
    {
        hasMoved = false;
        hasAttacked = false;
        movement = 0;
        SetActiveHighlight(false);
    }

    private void OnMouseOver()
    {
        UIManager.Instance.SetInfoPanel(unitName, "HP: " + currentHP + "/" + maxHP + "\nBase Movement: " + baseMovement + "\nBase Attack: " + baseAttack);
    }

    private void OnMouseExit()
    {
        UIManager.Instance.ClearInfoPanel();
    }

    private void OnMouseUpAsButton()
    {
        if (GameManager.Instance.state != GameState.PlayerActions) return;

        if (isSelected)
        {
            UnitManager.Instance.OnUnitDeselect();
        }
        else
        {
            UnitManager.Instance.OnUnitSelected(this);
        }
    }
}
