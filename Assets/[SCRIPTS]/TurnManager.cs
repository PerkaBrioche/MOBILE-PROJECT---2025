using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    private bool _endGame;
    private TouchManager TouchManager;
    private float _campUpdateDelay = 0.5f;
    private int _turnCount = 0;
    public int TurnCount { get { return _turnCount; } }
    [SerializeField] private TextMeshProUGUI turnCountText;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Image resultStar1;
    [SerializeField] private Image resultStar2;
    [SerializeField] private Image resultStar3;
    [SerializeField] private int turnThresholdForThreeStars;
    [SerializeField] private int turnThresholdForTwoStars;
    [SerializeField] private int currentLevelIndex;

    public bool IsPlayerTurn() { return _isPlayerTurn; }
    public bool IsEnemyTurn() { return _isEnemyTurn; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
        _endGame = false;
    }

    public bool IsEndGame()
    {
        return _endGame;
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
                _turnCount++;
                UpdateTurnCountDisplay();
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
            _turnCount++;
            UpdateTurnCountDisplay();
            _isPlayerTurn = true;
            if (ResetTurnManager.Instance != null)
                ResetTurnManager.Instance.RecordStartingPositions();
            _turnButton.interactable = false;
            UpdateText("Player Turn", Color.green);
        }));
    }

    public void StartPlayerTurn()
    {
        _turnCount++;
        UpdateTurnCountDisplay();
        UnlockButtonTurn();
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
        LockButtonTurn();
        TouchManager.Reset();
        _turnButton.interactable = false;
        if (!CheckEndGame())
            StartEnemyTurn();
    }

    public void StartEnemyTurn()
    {
        _turnCount++;
        UpdateTurnCountDisplay();
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
        var ally = ShipManager.Instance.GetAllyShipsOrinalCamp();
        var enemy = ShipManager.Instance.GetEnemyShipsOrinalCamp();
        if (enemy.Count == 0)
        {
            _endGame = true;
            Victory();
            return true;
        }
        if (ally.Count == 0)
        {
            _endGame = true;
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
            phaseAnimator.SetTrigger("Win");
        StartCoroutine(ShowVictoryPanel());
    }

    public void Defeat()
    {
        LockButtonTurn();
        if (phaseAnimator != null)
            phaseAnimator.SetTrigger("Defeat");
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

    private IEnumerator ShowVictoryPanel()
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);
        int stars = 1;
        if (_turnCount <= turnThresholdForThreeStars)
            stars = 3;
        else if (_turnCount <= turnThresholdForTwoStars)
            stars = 2;
        else
            stars = 1;
        if (resultStar1 != null)
            resultStar1.gameObject.SetActive(stars >= 1);
        if (resultStar2 != null)
            resultStar2.gameObject.SetActive(stars >= 2);
        if (resultStar3 != null)
            resultStar3.gameObject.SetActive(stars >= 3);
        int currentBest = PlayerPrefs.GetInt("LevelStars_" + currentLevelIndex, 0);
        if (stars > currentBest)
        {
            PlayerPrefs.SetInt("LevelStars_" + currentLevelIndex, stars);
            PlayerPrefs.Save();
        }
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(0);
    }
    
    private void UpdateTurnCountDisplay()
    {
        if (turnCountText != null)
            turnCountText.text = "Turn: " + _turnCount;
    }
}
