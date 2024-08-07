using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public TileMap map;
    public TileType tileType;
    public int tileX;
    public int tileY;
    public List<Tile> neighbors;

    [SerializeField] private GameObject lightHighlightObject;
    [SerializeField] private GameObject darkHighlightObject;
    [SerializeField] private GameObject attackHighlightObject;

    public Unit occupyingUnit = null;

    private void Start()
    {
        SetHighlight(HighlightState.None);
    }

    private void OnMouseEnter()
    {
        UIManager.Instance.SetInfoPanel(map.tileTypes[tileType].tileName, "Traversal Cost: " + map.tileTypes[tileType].traversalCost.ToString() + "\nCoord: " + tileX + ", " + tileY);
    }

    private void OnMouseExit()
    {
        UIManager.Instance.ClearInfoPanel();
    }

    private void OnMouseUpAsButton()
    {
        map.OnTileClicked(this);
    }

    public enum HighlightState
    {
        None,
        Light,
        Dark,
        Attack
    }

    public void SetHighlight(HighlightState state)
    {
        switch (state)
        {
            case HighlightState.None:
                lightHighlightObject.SetActive(false);
                darkHighlightObject.SetActive(false);
                attackHighlightObject.SetActive(false);
                break;
            case HighlightState.Light:
                lightHighlightObject.SetActive(true);
                darkHighlightObject.SetActive(false);
                attackHighlightObject.SetActive(false);
                break;
            case HighlightState.Dark:
                lightHighlightObject.SetActive(false);
                darkHighlightObject.SetActive(true);
                attackHighlightObject.SetActive(false);
                break;
            case HighlightState.Attack:
                lightHighlightObject.SetActive(false);
                darkHighlightObject.SetActive(false);
                attackHighlightObject.SetActive(true);
                break;
        }
    }
}
