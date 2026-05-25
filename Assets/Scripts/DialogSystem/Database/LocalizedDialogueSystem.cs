using System.Collections.Generic;
using UnityEngine;

public enum Language { Russian, English }

public class LocalizedDialogueSystem : MonoBehaviour
{
    private readonly Dictionary<string, string> _dialogues = new();

    public static LocalizedDialogueSystem Instance { get; private set; }

    public static Language CurrentLanguage = Language.English;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDialogues();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public string GetDialogueText(string id)
    {
        return _dialogues.ContainsKey(id) ? _dialogues[id] : "[Missing Dialogue]";
    }

    public void LoadDialogues()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("dialogues");

        if (jsonFile == null)
        {
            Debug.LogError("Failed to load dialogues.json from Resources!");
            return;
        }

        LocalizedDialogueDatabase database = JsonUtility.FromJson<LocalizedDialogueDatabase>(jsonFile.text);

        _dialogues.Clear();

        foreach (var dialogue in database.dialogues)
        {
            string text = CurrentLanguage == Language.Russian ? dialogue.russian : dialogue.english;

            _dialogues[dialogue.id] = text;
        }
    }
}
