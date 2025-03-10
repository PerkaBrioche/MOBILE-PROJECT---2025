using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    private GameManager.GameWinCondition gameWinCondition;
    private int TurnMinimumTwoStars;
    private int TurnMinimumThreeStars;
    
    private bool _isPlayerTurn = false;
    private bool _isEnemyTurn = false;
    public static TurnManager Instance;
    [Foldout("REFERENCES")]
    [SerializeField] private TextMeshProUGUI _turnText;
    [Foldout("REFERENCES")]
    [SerializeField] private Button _turnButton;
    [Foldout("REFERENCES")]
    [SerializeField] private Button _REturnButton;
    [Foldout("REFERENCES")]
    [SerializeField] private Animator phaseAnimator;
    private int _enemyTurn;
    private bool _waitingForEnemy = false;
    private bool _actualisedCamp = false;
    private bool _endGame;
    private TouchManager TouchManager;
    private float _campUpdateDelay = 0.5f;
    private bool _gameStarted = false;
    private int _turnCount = 0;

    [SerializeField] private GameObject _panelOption;
    public int TurnCount { get { return _turnCount; } }
    
    [Foldout("REFERENCES")]
    [SerializeField] private TextMeshProUGUI turnCountText;
    [Foldout("REFERENCES")]
    [SerializeField] private GameObject resultPanel;
    [Foldout("REFERENCES")]
    [SerializeField] private Button _nextTurnButton;
    [Foldout("REFERENCES")]
    [SerializeField] private Image resultStar1;
    [Foldout("REFERENCES")]
    [SerializeField] private Image resultStar2;
    [Foldout("REFERENCES")]
    [SerializeField] private Image resultStar3;

    [Foldout("REFERENCES")] [SerializeField]
    private Sprite _unlockedStars;
    [Foldout("REFERENCES")] [SerializeField] private Sprite _lockedStars;
    
    private bool optionPanelActive = false;
    //[Foldout("REFERENCES")]
    // [SerializeField] private int currentLevelIndex;

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
        gameWinCondition = GameManager.Instance.gameWinCondition;
        TurnMinimumTwoStars = GameManager.Instance.TurnMinimumTwoStars;
        TurnMinimumThreeStars = GameManager.Instance.TurnMinimumThreeStars;
        if (DialogueManager.Instance != null && DialogueManager.Instance.HasDialogue())
        {
            DialogueManager.Instance.StartDialogue();
            StartCoroutine(WaitForDialogueEnd());
        }
        else
        {
            StartCoroutine(PhaseTransition("PlayerPhase", () =>
            {
                _turnButton.interactable = false;
                UpdateText("Player Turn", Color.green);
            }));
        }
        TouchManager = FindFirstObjectByType<TouchManager>();
    }
    
    public void LockRETURNButton()
    {
        _REturnButton.interactable = false;
    }
    
    public void UnlockRETURNButton()
    {
        _REturnButton.interactable = true;
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
            _turnButton.interactable = false;
            _gameStarted = true;
        }));
    }
    
    public void HideAllHealthBars(bool hide)
    {
        foreach (var ship in ShipManager.Instance.GetAllships())
        {
            ship.SetHealthBarVisible(hide);
        }
    }

    public void StartPlayerTurn()
    {
        if(_endGame){return;}

        UnlockButtonTurn();
        ResetTurnManager.Instance.RecordStartingPositions();
        _isPlayerTurn = true;
        SoundManager.Instance.PlaySound(SoundManager.SoundList.PlayerPhase);
        phaseAnimator.SetTrigger("PlayerPhase");
    }

    public void EndPlayerTurn()
    {
        if(_endGame){return;}
        _isPlayerTurn = false;
        LockButtonTurn();
        TouchManager.Reset();
        _turnButton.interactable = false;
        _turnCount++;
        UpdateTurnCountDisplay();
        if (!CheckEndGame())
            StartEnemyTurn();
    }

    public void StartEnemyTurn()
    {
        if(_endGame){return;}
        _actualisedCamp = false;
        StartCoroutine(WaitForCampUpdate());
        ShipManager.Instance.ChangeShipsCamp();
        if (phaseAnimator != null)
            phaseAnimator.SetTrigger("EnemyPhase");
        SoundManager.Instance.PlaySound(SoundManager.SoundList.EnemyPhase);
        _enemyTurn = 0;
        _isEnemyTurn = true;
    }

    public void EndEnemyTurn()
    {
        if(_endGame){return;}
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
    
    public void ChangeOptionPanelState()
    {
        optionPanelActive = !optionPanelActive;
        HideAllHealthBars(!optionPanelActive);
        _panelOption.SetActive(optionPanelActive);
    }

    private void Update()
    {
        if (!_gameStarted || _endGame)
        {
            return;
        }
        if (_isPlayerTurn)
        {
            UnlockRETURNButton();
        }
        else
        {
            LockRETURNButton();
        }
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
        var ally = ShipManager.Instance.GetAllyShipsOrinalCamp();

        if (ally.Count == 0)
        {
            EndGame();
            Defeat();
            return true;
        }
        if(ally.Count == 1 && ally[0].GetComponent<ShipController>().IsMotherShip())
        {
            EndGame();
            Defeat();
            return true;
        }
        if (gameWinCondition != GameManager.GameWinCondition.destroyAll)
        {
            return false;
        }
        
        var enemy = ShipManager.Instance.GetEnemyShipsOrinalCamp();
        if (enemy.Count == 0)
        {
            EndGame();
            Victory();
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
        SoundManager.Instance.PlaySound(SoundManager.SoundList.Win);
        EndGame();
        LockButtonTurn();
        ShipManager.Instance.BounceDispawn();
        if (phaseAnimator != null)
            phaseAnimator.SetTrigger("Win");
        StartCoroutine(ShowVictoryPanel());
    }

    public void Defeat()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundList.Lose);
        EndGame();
        LockButtonTurn();
        ShipManager.Instance.BounceDispawn();
        if (phaseAnimator != null)
            phaseAnimator.SetTrigger("Defeat");
        StartCoroutine(ShowDefeatPanel());
    }

    private void EndGame()
    {
        var musicSource = GameObject.Find("MUSIC SOURCE");
        if (musicSource != null)
        {
            musicSource.GetComponent<AudioSource>().Stop();
        }
        _isPlayerTurn = false;
        _isEnemyTurn = false;
        _endGame = true;
       // HideAllHealthBars(false);
        LockButtonTurn();
        LockRETURNButton();
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
        yield return new WaitForSeconds(2f);
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
        yield return new WaitForSeconds(0.4f);

        int stars = 1;
        if (_turnCount <= TurnMinimumTwoStars)
        {
            stars = 2;
        }
        if (_turnCount <= TurnMinimumThreeStars)
        {
            stars = 3;
        }
        int currentBest = PlayerPrefs.GetInt("LevelStars_" + SceneManager.GetActiveScene().buildIndex, 0);
        if (stars > currentBest)
        {
            PlayerPrefs.SetInt("LevelStars_" + SceneManager.GetActiveScene().buildIndex, stars);
            PlayerPrefs.Save();
        }
        SoundManager.Instance.PlaySound(SoundManager.SoundList.Star1);
            
        AchievementManager.CheckAchievements();

        resultStar1.sprite = _unlockedStars;
        resultStar1.transform.GetComponent<bounce>().StartBounce();
        yield return new WaitForSeconds(0.4f);

        if (stars >= 2)
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundList.Star2);
            resultStar2.sprite = _unlockedStars;
            resultStar2.transform.GetComponent<bounce>().StartBounce();
        }
        yield return new WaitForSeconds(0.4f);

        if (stars >= 3)
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundList.Star3);
            resultStar3.sprite = _unlockedStars;
            resultStar3.transform.GetComponent<bounce>().StartBounce();
            yield return new WaitForSeconds(0.5f);
            resultStar1.transform.GetComponent<bounce>().StartBounce();
            yield return new WaitForSeconds(0.1f);
            resultStar2.transform.GetComponent<bounce>().StartBounce();
            yield return new WaitForSeconds(0.1f);
            resultStar3.transform.GetComponent<bounce>().StartBounce();
        }
    }

    private IEnumerator ShowDefeatPanel()
    {
        yield return new WaitForSeconds(2f);
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            if(_nextTurnButton != null)
            {
                _nextTurnButton.gameObject.SetActive(false);
            }
        }
        yield return new WaitForSeconds(0.5f);
        if(resultStar1 != null)
            resultStar1.sprite = _lockedStars;
        if(resultStar2 != null)
            resultStar2.sprite = _lockedStars;
        if(resultStar3 != null)
            resultStar3.sprite = _lockedStars;
    }
    
    private void UpdateTurnCountDisplay()
    {
        if (turnCountText != null)
            turnCountText.text = "Turn: " + _turnCount;
    }
    
    public void ReturnToMenu()
    {
        SceneManager.LoadScene(1);
    }
    
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
