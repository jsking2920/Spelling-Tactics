using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class WordDictionary : MonoBehaviour
{
    public static WordDictionary instance;

    private HashSet<string> words;

    private string fullWordListFilename = "full_wordlist";
    private string dirtyWordListFilename = "dirty_wordlist";
    private string cleanedWordListFilename = "cleaned_wordlist";

    private void Awake()
    {
        instance = this;
        Initialize();
    }

    #region Initialization
    private void Initialize()
    {
        TextAsset wordlist = Resources.Load<TextAsset>(cleanedWordListFilename);
        if (wordlist == null)
        {
            CleanAndWriteWordlistToFile();
            Debug.LogError("Please restart the game to use new word list file");
        }
        words = wordlist.text.Split(new char[] { '\n' }).ToHashSet();
    }

    // Will run automatically in editor and then you can just use the file it spits out in the final build
    // WILL NOT WORK IN BUILT VERSION (no resources folder to write to)
    private void CleanAndWriteWordlistToFile()
    {
        Debug.Log("Creating cleaned wordlist txt file named: " + cleanedWordListFilename);

        TextAsset wordlist = Resources.Load<TextAsset>(fullWordListFilename);
        TextAsset dirtyWordlist = Resources.Load<TextAsset>(dirtyWordListFilename);

        HashSet<string> allWords = wordlist.text.Split("\r\n").ToHashSet();
        HashSet<string> dirtyWords = dirtyWordlist.text.Split('\n').ToHashSet();

        if (allWords == null || dirtyWords == null)
        {
            Debug.LogError("Couldn't find a word list!!");
            return;
        }

        StreamWriter newFile = File.CreateText("Assets/Resources/" + cleanedWordListFilename + ".txt");
        foreach(string word in allWords)
        {
            if (!dirtyWords.Contains(word))
            {
                newFile.Write(word + "\n");
            }
        }
        newFile.Close();
    }
    #endregion

    public bool IsWordValid(string word)
    {
        return words.Contains(word);
    }
}
