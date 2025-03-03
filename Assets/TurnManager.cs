using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    private bool _isPlayerTurn = false;
    private bool _isEnemyTurn = false;
    public static TurnManager Instance;
    [SerializeField] private TextMeshProUGUI _turnText;
    [SerializeField] private Button _turnButton;
    [SerializeField] private Animator phaseAnimator;
    private int _enemyTurn;
    private bool _waitingForEnemy = false;
    private bool _actualisedCamp = false;
    private float _campUpdateDelay = 0.5f;

    public bool IsPlayerTurn() { return _isPlayerTurn; }
    public bool IsEnemyTurn() { return _isEnemyTurn; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.HasDialogue())
        {
            DialogueManager.Instance.StartDialogue();
            StartCoroutine(WaitForDialogueEnd());
        }
        else
        {
            StartCoroutine(PhaseTransition("PlayerPhase", () =>
            {
                if (ResetTurnManager.Instance != null)
                    ResetTurnManager.Instance.RecordStartingPositions();
                _turnButton.interactable = false;
                UpdateText("Player Turn", Color.green);
            }));
        }
    }

    private IEnumerator WaitForDialogueEnd()
    {
        while (DialogueManager.Instance.dialoguePanel.activeSelf)
            yield return null;
        DialogueEnded();
    }

    public void DialogueEnded()
    {
        StartCoroutine(PhaseTransition("PlayerPhase", () =>
        {
            _isPlayerTurn = true;
            if (ResetTurnManager.Instance != null)
                ResetTurnManager.Instance.RecordStartingPositions();
            _turnButton.interactable = false;
            UpdateText("Player Turn", Color.green);
        }));
    }

    public void StartPlayerTurn()
    {
        _isPlayerTurn = true;
        if (ResetTurnManager.Instance != null)
            ResetTurnManager.Instance.RecordStartingPositions();
        if (phaseAnimator != null)
            phaseAnimator.SetTrigger("PlayerPhase");
        else
            UpdateText("Player Turn", Color.green);
    }

    public void EndPlayerTurn()
    {
        _isPlayerTurn = false;
        _turnButton.interactable = false;
        if (!CheckEndGame())
            StartEnemyTurn();
    }

    public void StartEnemyTurn()
    {
        _actualisedCamp = false;
        StartCoroutine(WaitForCampUpdate());
        ShipManager.Instance.ChangeShipsCamp();
        if (phaseAnimator != null)
            phaseAnimator.SetTrigger("EnemyPhase");
        else
            UpdateText("Enemy Turn", Color.red);
        _enemyTurn = 0;
        _isEnemyTurn = true;
    }

    public void EndEnemyTurn()
    {
        _isEnemyTurn = false;
        ShipManager.Instance.ResetAllShips();
        if (!CheckEndGame())
        {
            ShipManager.Instance.ChangeShipsCamp();
            StartPlayerTurn();
        }
    }

    public void CheckUnlockButton()
    {
        if (_isPlayerTurn)
            _turnButton.interactable = true;
    }

    public void EnemyEndATurn()
    {
        _enemyTurn++;
        _waitingForEnemy = false;
    }

    private void Update()
    {
        if (_isEnemyTurn)
        {
            if (!_waitingForEnemy && _actualisedCamp)
            {
                if (_enemyTurn >= ShipManager.Instance.GetActualAllyShips().Count)
                {
                    EndEnemyTurn();
                    _waitingForEnemy = false;
                    return;
                }
                _waitingForEnemy = true;
                if (ShipManager.Instance.GetActualAllyShip(_enemyTurn).TryGetComponent(out Enemy enemy))
                    enemy.SetMyTurn();
                else
                    Debug.LogError("MISSING ENEMY COMPONENT");
            }
        }
    }

    private IEnumerator WaitForCampUpdate()
    {
        yield return new WaitForSeconds(_campUpdateDelay);
        _actualisedCamp = true;
    }

    private bool CheckEndGame()
    {
        var ally = ShipManager.Instance.GetAllyShipsOrinalCamp();
        var enemy = ShipManager.Instance.GetEnemyShipsOrinalCamp();
        if (enemy.Count == 0)
        {
            if (phaseAnimator != null)
                phaseAnimator.SetTrigger("Victory");
            else
                UpdateText("VICTORY", Color.green);
            return true;
        }
        if (ally.Count == 0)
        {
            if (phaseAnimator != null)
                phaseAnimator.SetTrigger("Defeat");
            else
                UpdateText("DEFEAT", Color.red);
            return true;
        }
        return false;
    }

    private void UpdateText(string text, Color color)
    {
        _turnText.text = text;
        _turnText.color = color;
    }

    public IEnumerator PhaseTransition(string triggerName, Action onComplete)
    {
        if (phaseAnimator != null)
        {
            phaseAnimator.SetTrigger(triggerName);
            AnimatorStateInfo state = phaseAnimator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(state.length);
        }
        onComplete?.Invoke();
    }
}
