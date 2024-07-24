using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordEntryField : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
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
        inputField.text = "";
    }

    private void btn_SubmitButton()
    {
        SubmitWord(inputField.text);
    }
}
