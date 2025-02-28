using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public bool leftSpeaker;
    public string speakerName;
    [TextArea] public string text;
}