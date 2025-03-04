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
    
    private TouchManager TouchManager;
    
    private float _campUpdateDelay = 0.5f;
    
    private bool _endGame = false;
    
    public GameManager.GameWinCondition gameWinCondition;
    
    [SerializeField] private bool _gameStarted = false;
    

    public bool IsPlayerTurn() { return _isPlayerTurn; }
    public bool IsEnemyTurn() { return _isEnemyTurn; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public bool IsEndGame()
    {
        return _endGame;
    }

    private void Start()
    {
        gameWinCondition = GameManager.Instance.gameWinCondition;
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
        
        TouchManager = FindFirstObjectByType<TouchManager>();
    }

    private IEnumerator WaitForDialogueEnd()
    {
        while (DialogueManager.Instance.dialoguePanel.activeSelf)
            yield return null;
        DialogueEnded();
        StartPlayerTurn();
        UnlockButtonTurn();
    }

    public void DialogueEnded()
    {
        StartCoroutine(PhaseTransition("PlayerPhase", () =>
        {
            _isPlayerTurn = true;
            if (ResetTurnManager.Instance != null)
                ResetTurnManager.Instance.RecordStartingPositions();
            _turnButton.interactable = false;
            _gameStarted = true;
        }));
    }

    public void StartPlayerTurn()
    {
        UnlockButtonTurn();
        _isPlayerTurn = true;
        if (ResetTurnManager.Instance != null)
            ResetTurnManager.Instance.RecordStartingPositions();
        if (phaseAnimator != null)
        {
            PlayAnimation("PlayerPhase", true);
        }
    }
    
    private void PlayAnimation(string trigger, bool addShake = false)
    {
        if (phaseAnimator != null)
            phaseAnimator.SetTrigger(trigger);

        if (addShake)
        {
            StartCoroutine(ShakePhase());
        }
    }
    
    private IEnumerator ShakePhase()
    {
        yield return new WaitForSeconds(0.3f);
        ShakeManager.instance.ShakeCamera(0.15f, 0.15f);
    }

    public void EndPlayerTurn()
    {
        _isPlayerTurn = false;
        LockButtonTurn();
        TouchManager.Reset();
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
            PlayAnimation("EnemyPhase", true);
        else
            UpdateText("Enemy Turn", Color.red);
        _enemyTurn = 0;
        _isEnemyTurn = true;
    }

    public void EndEnemyTurn()
    {
        TouchManager.Reset();
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
        if (!_gameStarted) return;
        CheckEndGame();
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
    
    public ShipController GetEnemyShip()
    {
        return ShipManager.Instance.GetActualAllyShip(_enemyTurn);
    }

    public void LockButtonTurn()
    {
        _turnButton.interactable = false;
    }

    public void UnlockButtonTurn()
    {
        _turnButton.interactable = true;
    }
    private IEnumerator WaitForCampUpdate()
    {
        yield return new WaitForSeconds(_campUpdateDelay);
        _actualisedCamp = true;
    }

    private bool CheckEndGame()
    {
        if (gameWinCondition != GameManager.GameWinCondition.destroyAll)
        { return false;}
        
        var ally = ShipManager.Instance.GetAllyShipsOrinalCamp();
        var enemy = ShipManager.Instance.GetEnemyShipsOrinalCamp();
        if (enemy.Count == 0)
        {
            EndGame();
            Victory();
            return true;
        }
        if (ally.Count == 0)
        {
            EndGame();
            Defeat();
            return true;
        }
        return false;
    }

    private void LockEverything()
    {
        _isPlayerTurn = false;
        _isEnemyTurn = false; 
        LockButtonTurn();
    }
    public void Victory()
    {
        LockButtonTurn();
        if (phaseAnimator != null)
            PlayAnimation("Win");
    }

    public void Defeat()
    {
        LockButtonTurn();
        if (phaseAnimator != null)
            PlayAnimation("Defeat");
    }

    private void EndGame()
    {
        _isPlayerTurn = false;
        _isEnemyTurn = false;
        _endGame = true;
        LockButtonTurn();
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
