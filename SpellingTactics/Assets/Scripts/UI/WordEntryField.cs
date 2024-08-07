using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordEntryField : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI inputFieldPreviewText;
    [SerializeField] private Button submitButton;

    void Start()
    {
        //inputField.onSelect.AddListener();
        inputField.onSubmit.AddListener(SubmitWord);
        //inputField.onValueChanged.AddListener();

        submitButton.onClick.AddListener(btn_SubmitButton);
    }

    private void SubmitWord(string word)
    {
        word = word.ToUpper();

        if (GameManager.Instance.usedWords.Contains(word))
        {
            inputFieldPreviewText.text = "Already used that one...";
        }
        else if (word == "" || !WordDictionary.Instance.IsWordValid(word))
        {
            inputFieldPreviewText.text = "That's not a word...";
        }
        else
        {
            UIManager.Instance.OnValidWordSubmitted(word);
            inputField.interactable = false;
            submitButton.interactable = false;
            inputFieldPreviewText.text = "Enter a word...";
        }

        inputField.text = "";
    }

    private void btn_SubmitButton()
    {
        SubmitWord(inputField.text);
    }

    // TODO: Make OnNewRound an event that gets subscribed to
    public void OnNewRound()
    {
        inputField.interactable = true;
        submitButton.interactable = true;
    }
}
