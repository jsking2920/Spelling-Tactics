using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject infoPanelTextHolder;
    [SerializeField] private TextMeshProUGUI infoPanelTextHeader;
    [SerializeField] private TextMeshProUGUI infoPanelTextContent;

    [SerializeField] private GameObject selectedUnitTextHolder;
    [SerializeField] private TextMeshProUGUI selectedUnitTextHeader;
    [SerializeField] private TextMeshProUGUI selectedUnitTextContent;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ClearInfoPanel();
        ClearSelectedUnitInfo();
    }

    public void SetSelectedUnitInfo(Unit unit)
    {
        selectedUnitTextHeader.text = unit.letter;
        selectedUnitTextContent.text = unit.unitName + "\nMovement: " + unit.movement;
        selectedUnitTextHolder.SetActive(true);
    }

    public void ClearSelectedUnitInfo()
    {
        selectedUnitTextHolder.SetActive(false);
    }

    public void SetInfoPanel(string header, string content = "")
    {
        infoPanelTextHeader.text = header;
        infoPanelTextContent.text = content;
        infoPanelTextHolder.SetActive(true);
    }

    public void ClearInfoPanel()
    {
        infoPanelTextHolder.SetActive(false);
    }

    public void btn_Quit()
    {
        Application.Quit();
    }

    public void btn_Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
