using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TileMap map;
    public TileType tileType;
    public int tileX;
    public int tileY;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        
    }

    private void OnMouseEnter()
    {
        UIManager.Instance.SetInfoPanel(map.tileTypes[tileType].tileName, map.tileTypes[tileType].traversalCost.ToString());
    }

    private void OnMouseExit()
    {
        UIManager.Instance.ClearInfoPanel();
    }

    private void OnMouseUpAsButton()
    {
        print("here");
    }
}
