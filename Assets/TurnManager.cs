using System;
using System.Collections;
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
    [SerializeField] private Transform blockLeftStart;
    [SerializeField] private Transform blockLeftEnd;
    [SerializeField] private Transform blockRightStart;
    [SerializeField] private Transform blockRightEnd;
    [SerializeField] private Image playerLeftBlock;
    [SerializeField] private Image playerRightBlock;
    [SerializeField] private Image enemyLeftBlock;
    [SerializeField] private Image enemyRightBlock;
    [SerializeField] private Image dimBackground;
    private int _enemyTurn;
    private bool _waitingForEnemy = false;
    private bool _actualisedCamp = false;
    private float _blockMoveDuration = 0.5f;
    private float _textDisplayDuration = 1f;
    private float _fadeDuration = 0.5f;

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
        // Au début, on lance toujours la phase joueur.
        if (DialogueManager.Instance != null && DialogueManager.Instance.HasDialogue())
        {
            DialogueManager.Instance.StartDialogue();
        }
        StartCoroutine(PhaseTransition("Player Phase", () =>
        {
            _isPlayerTurn = true;
            if (ResetTurnManager.Instance != null)
                ResetTurnManager.Instance.RecordStartingPositions();
            LockButtonTurn();
        }));
    }

    // Cette méthode doit être appelée par le DialogueManager quand le dialogue est terminé.
    public void DialogueEnded()
    {
        StartCoroutine(PhaseTransition("Player Phase", () =>
        {
            _isPlayerTurn = true;
            if (ResetTurnManager.Instance != null)
                ResetTurnManager.Instance.RecordStartingPositions();
            LockButtonTurn();
        }));
    }

    public void StartPlayerTurn()
    {
        _isPlayerTurn = true;
        UpdateText("Player Turn", Color.green);
    }

    public void EndPlayerTurn()
    {
        _isPlayerTurn = false;
        LockButtonTurn();
        if (!CheckEndGame())
            StartEnemyTurn();
    }

    public void StartEnemyTurn()
    {
        _actualisedCamp = false;
        StartCoroutine(WaitForCampUpdate());
        ShipManager.Instance.ChangeShipsCamp();
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

    private void UpdateText(string text, Color color)
    {
        _turnText.text = text;
        _turnText.color = color;
    }

    private void UnlockButtonTurn()
    {
        _turnButton.interactable = true;
    }

    public void CheckUnlockButton()
    {
        if (_isPlayerTurn)
            UnlockButtonTurn();
    }

    // Méthode appelée par un Enemy quand il a terminé son action
    public void EnemyEndATurn()
    {
        _enemyTurn++;
        _waitingForEnemy = false;
    }

    // Pendant le tour ennemi, la logique originale est utilisée : les ennemis attaquent les unités alliées.
    private void Update()
    {
        if (_isEnemyTurn)
        {
            if (!_waitingForEnemy && _actualisedCamp)
            {
                // On itère sur les unités alliées pour que l'ennemi attaque bien les alliés.
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

    private void LockButtonTurn()
    {
        _turnButton.interactable = false;
    }

    private IEnumerator WaitForCampUpdate()
    {
        yield return new WaitForSeconds(0.5f);
        _actualisedCamp = true;
    }

    private bool CheckEndGame()
    {
        var ally = ShipManager.Instance.GetAllyShipsOrinalCamp();
        var enemy = ShipManager.Instance.GetEnemyShipsOrinalCamp();
        if (enemy.Count == 0)
        {
            WinGame();
            return true;
        }
        if (ally.Count == 0)
        {
            LoseGame();
            return true;
        }
        return false;
    }

    private void WinGame()
    {
        UpdateText("VICTORY", Color.green);
    }

    public void LoseGame()
    {
        UpdateText("DEFEAT", Color.red);
    }

    public IEnumerator PhaseTransition(string phaseName, Action onComplete)
    {
        SetAllHealthBars(false);
        Image leftBlock, rightBlock;
        if (phaseName.Contains("Player") || phaseName.Contains("VICTORY"))
        {
            leftBlock = playerLeftBlock;
            rightBlock = playerRightBlock;
        }
        else
        {
            leftBlock = enemyLeftBlock;
            rightBlock = enemyRightBlock;
        }
        leftBlock.gameObject.SetActive(true);
        rightBlock.gameObject.SetActive(true);
        dimBackground.gameObject.SetActive(true);
        _turnText.gameObject.SetActive(true);
        RectTransform leftRect = leftBlock.rectTransform;
        RectTransform rightRect = rightBlock.rectTransform;
        Vector2 leftStart = blockLeftStart.GetComponent<RectTransform>().anchoredPosition;
        Vector2 leftEnd = blockLeftEnd.GetComponent<RectTransform>().anchoredPosition;
        Vector2 rightStart = blockRightStart.GetComponent<RectTransform>().anchoredPosition;
        Vector2 rightEnd = blockRightEnd.GetComponent<RectTransform>().anchoredPosition;
        leftRect.anchoredPosition = leftStart;
        rightRect.anchoredPosition = rightStart;
        Color dimCol = dimBackground.color;
        dimCol.a = 0;
        dimBackground.color = dimCol;
        _turnText.alpha = 0;
        float t = 0;
        while (t < _fadeDuration)
        {
            t += Time.deltaTime;
            dimCol.a = Mathf.Lerp(0, 0.5f, t / _fadeDuration);
            dimBackground.color = dimCol;
            yield return null;
        }
        t = 0;
        while (t < _blockMoveDuration)
        {
            t += Time.deltaTime;
            leftRect.anchoredPosition = Vector2.Lerp(leftStart, leftEnd, t / _blockMoveDuration);
            rightRect.anchoredPosition = Vector2.Lerp(rightStart, rightEnd, t / _blockMoveDuration);
            yield return null;
        }
        t = 0;
        while (t < _fadeDuration)
        {
            t += Time.deltaTime;
            _turnText.alpha = Mathf.Lerp(0, 1, t / _fadeDuration);
            yield return null;
        }
        _turnText.text = phaseName;
        yield return new WaitForSeconds(_textDisplayDuration);
        t = 0;
        while (t < _fadeDuration)
        {
            t += Time.deltaTime;
            _turnText.alpha = Mathf.Lerp(1, 0, t / _fadeDuration);
            yield return null;
        }
        t = 0;
        while (t < _blockMoveDuration)
        {
            t += Time.deltaTime;
            leftRect.anchoredPosition = Vector2.Lerp(leftEnd, leftStart, t / _blockMoveDuration);
            rightRect.anchoredPosition = Vector2.Lerp(rightEnd, rightStart, t / _blockMoveDuration);
            yield return null;
        }
        t = 0;
        while (t < _fadeDuration)
        {
            t += Time.deltaTime;
            dimCol.a = Mathf.Lerp(0.5f, 0, t / _fadeDuration);
            dimBackground.color = dimCol;
            yield return null;
        }
        leftBlock.gameObject.SetActive(false);
        rightBlock.gameObject.SetActive(false);
        dimBackground.gameObject.SetActive(false);
        _turnText.gameObject.SetActive(false);
        SetAllHealthBars(true);
        onComplete?.Invoke();
    }

    private void SetAllHealthBars(bool visible)
    {
        ShipController[] ships = FindObjectsOfType<ShipController>();
        foreach (ShipController ship in ships)
            ship.SetHealthBarVisible(visible);
    }
}
