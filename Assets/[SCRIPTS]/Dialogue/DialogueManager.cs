using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Febucci.UI;

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
    private TypewriterByCharacter typewriter;
    
    private Transform actualSpeaker;
    private bool _isTalking = false;
    
    [SerializeField] private List<ShipController> _shipsList = new List<ShipController>();
    
    

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        typewriter = dialogueText.GetComponent<TypewriterByCharacter>();


    }

    private void Start()
    {
        _shipsList = ShipManager.Instance.GetAllships();
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
       // TouchManager.Instance.SetInteractionEnabled(false);
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
        
        typewriter.ShowText(dialogueData.lines[index].text);
        
        DialogueLine line = dialogueData.lines[index];
        leftImage.color = line.leftSpeaker ? new Color(1,1,1,1) : new Color(1,1,1,0.5f);
        rightImage.color = line.leftSpeaker ? new Color(1,1,1,0.5f) : new Color(1,1,1,1);
        actualSpeaker = line.leftSpeaker ? leftImage.transform : rightImage.transform;
        speakerNameText.text = line.speakerName;
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
        ShipsSpawned();
        TurnManager.Instance.DialogueEnded();
    }
    

    private void ShipsSpawned()
    {
        var shipsSpawners = FindObjectsOfType<ShipSpawner>();
        foreach (var shipSpawner in shipsSpawners)
        {
            shipSpawner.SpawnShip();
        }
    }
    public void CharacterTalking()
    {
        if(actualSpeaker == null) return;
        if (actualSpeaker.TryGetComponent(out Animation anim))
        {
            anim.Play();
        }
        else
        {
            Debug.LogError("MISSING ANIMATION COMPONENT");
        }
    }
    private IEnumerator ScaleCharacter()
    {
        _isTalking = true;
        yield return new WaitForSeconds(0.02f);
        _isTalking = false;
    }
    
}
