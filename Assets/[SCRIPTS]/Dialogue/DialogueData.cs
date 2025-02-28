using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueData", menuName = "Dialogue/DialogueData")]
public class DialogueData : ScriptableObject
{
    public Sprite leftSprite;
    public Sprite rightSprite;
    public List<DialogueLine> lines;
}