using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public DialogueData dialogueData;
    public GameObject dialoguePanel;
    public Image leftImage;
    public Image rightImage;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public Image backgroundDarkener;
    private int index;
    private bool active;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (!active) return;
        if (Input.GetMouseButtonDown(0))
        {
            NextLine();
        }
    }

    public bool HasDialogue()
    {
        return dialogueData != null && dialogueData.lines != null && dialogueData.lines.Count > 0;
    }

    public void StartDialogue()
    {
        if (!HasDialogue()) return;
        active = true;
        StartCoroutine(StartDialogueCoroutine());
    }

    private IEnumerator StartDialogueCoroutine()
    {
        yield return null;
        SetAllHealthBars(false);
        TouchManager.Instance.SetInteractionEnabled(false);
        dialoguePanel.SetActive(true);
        backgroundDarkener.gameObject.SetActive(true);
        leftImage.sprite = dialogueData.leftSprite;
        rightImage.sprite = dialogueData.rightSprite;
        index = 0;
        ShowLine();
    }

    public void ShowLine()
    {
        if (index >= dialogueData.lines.Count)
        {
            EndDialogue();
            return;
        }
        DialogueLine line = dialogueData.lines[index];
        leftImage.color = line.leftSpeaker ? new Color(1,1,1,1) : new Color(1,1,1,0.5f);
        rightImage.color = line.leftSpeaker ? new Color(1,1,1,0.5f) : new Color(1,1,1,1);
        speakerNameText.text = line.speakerName;
        dialogueText.text = line.text;
    }

    public void NextLine()
    {
        if (!active) return;
        index++;
        ShowLine();
    }

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        backgroundDarkener.gameObject.SetActive(false);
        active = false;
        SetAllHealthBars(true);
        TouchManager.Instance.SetInteractionEnabled(true);
        TurnManager.Instance.DialogueEnded();
    }
    
    private void SetAllHealthBars(bool visible)
    {
        ShipController[] ships = FindObjectsOfType<ShipController>();
        foreach (ShipController ship in ships)
        {
            ship.SetHealthBarVisible(visible);
        }
    }
}
