using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public string unitName = "Asp";
    public string letter = "A";
    public int tileX;
    public int tileY;
    public int movement = 5;

    public bool isSelected = false; // set by unit manager

    private void OnMouseOver()
    {
        UIManager.Instance.SetInfoPanel("unitName", "Movement: " + movement + "\nCoord: " + tileX + ", " + tileY);
    }

    private void OnMouseExit()
    {
        UIManager.Instance.ClearInfoPanel();
    }

    private void OnMouseUpAsButton()
    {
        if (!UnitManager.Instance.unitSelectable) return;

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
