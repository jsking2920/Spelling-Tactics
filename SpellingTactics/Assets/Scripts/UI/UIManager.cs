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

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ClearInfoPanel();
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
