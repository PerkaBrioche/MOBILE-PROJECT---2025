using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    private bool isPlayerTurn = false;
    private bool isEnemyTurn = false;
    public static TurnManager Instance;
    [SerializeField] private Button turnButton;
    [SerializeField] private Image dimBackground;
    [SerializeField] private TextMeshProUGUI phaseTransitionText;
    [SerializeField] private Transform blockLeftStart;
    [SerializeField] private Transform blockLeftEnd;
    [SerializeField] private Transform blockRightStart;
    [SerializeField] private Transform blockRightEnd;
    [SerializeField] private Image playerLeftBlock;
    [SerializeField] private Image playerRightBlock;
    [SerializeField] private Image enemyLeftBlock;
    [SerializeField] private Image enemyRightBlock;
    private int enemyTurn;
    private bool waitingForEnemy = false;
    private bool actualisedCamp = false;
    private float blockMoveDuration = 0.5f;
    private float textDisplayDuration = 1f;
    private float fadeDuration = 0.5f;

    public bool IsPlayerTurn() { return isPlayerTurn; }
    public bool IsEnemyTurn() { return isEnemyTurn; }

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
            return;
        }
        StartCoroutine(PhaseTransition("Player Phase", () =>
        {
            isPlayerTurn = true;
            if (ResetTurnManager.Instance != null)
                ResetTurnManager.Instance.RecordStartingPositions();
            LockButtonTurn();
        }));
    }
    public void DialogueEnded()
    {
        StartCoroutine(PhaseTransition("Player Phase", () =>
        {
            isPlayerTurn = true;
            if (ResetTurnManager.Instance != null)
                ResetTurnManager.Instance.RecordStartingPositions();
            LockButtonTurn();
        }));
    }
    public void EndPlayerTurn()
    {
        isPlayerTurn = false;
        LockButtonTurn();
        if (!CheckEndGame())
        {
            StartCoroutine(PhaseTransition("Enemy Phase", () =>
            {
                enemyTurn = 0;
                isEnemyTurn = true;
            }));
        }
    }
    public void EndEnemyTurn()
    {
        isEnemyTurn = false;
        ShipManager.Instance.ResetAllShips();
        if (!CheckEndGame())
        {
            StartCoroutine(PhaseTransition("Player Phase", () =>
            {
                isPlayerTurn = true;
                if (ResetTurnManager.Instance != null)
                    ResetTurnManager.Instance.RecordStartingPositions();
                LockButtonTurn();
            }));
        }
    }
    public void ShowWinScreen()
    {
        StartCoroutine(PhaseTransition("VICTORY", () => { }));
    }
    public void ShowLoseScreen()
    {
        StartCoroutine(PhaseTransition("DEFEAT", () => { }));
    }
    private IEnumerator PhaseTransition(string phaseName, Action onComplete)
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
        phaseTransitionText.gameObject.SetActive(true);
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
        phaseTransitionText.text = "";
        phaseTransitionText.alpha = 0;
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            dimCol.a = Mathf.Lerp(0, 0.5f, t / fadeDuration);
            dimBackground.color = dimCol;
            yield return null;
        }
        t = 0;
        while (t < blockMoveDuration)
        {
            t += Time.deltaTime;
            leftRect.anchoredPosition = Vector2.Lerp(leftStart, leftEnd, t / blockMoveDuration);
            rightRect.anchoredPosition = Vector2.Lerp(rightStart, rightEnd, t / blockMoveDuration);
            yield return null;
        }
        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            phaseTransitionText.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }
        phaseTransitionText.text = phaseName;
        yield return new WaitForSeconds(textDisplayDuration);
        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            phaseTransitionText.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }
        t = 0;
        while (t < blockMoveDuration)
        {
            t += Time.deltaTime;
            leftRect.anchoredPosition = Vector2.Lerp(leftEnd, leftStart, t / blockMoveDuration);
            rightRect.anchoredPosition = Vector2.Lerp(rightEnd, rightStart, t / blockMoveDuration);
            yield return null;
        }
        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            dimCol.a = Mathf.Lerp(0.5f, 0, t / fadeDuration);
            dimBackground.color = dimCol;
            yield return null;
        }
        leftBlock.gameObject.SetActive(false);
        rightBlock.gameObject.SetActive(false);
        dimBackground.gameObject.SetActive(false);
        phaseTransitionText.gameObject.SetActive(false);
        SetAllHealthBars(true);
        onComplete?.Invoke();
    }
    private void LockButtonTurn()
    {
        turnButton.interactable = false;
    }
    public void CheckUnlockButton()
    {
        if (isPlayerTurn)
            turnButton.interactable = true;
    }
    public void EnemyEndATurn()
    {
        enemyTurn++;
        waitingForEnemy = false;
    }
    private void Update()
    {
        if (isEnemyTurn)
        {
            if (!waitingForEnemy && actualisedCamp)
            {
                if (enemyTurn >= ShipManager.Instance.GetActualEnemyShipsCount())
                {
                    EndEnemyTurn();
                    waitingForEnemy = false;
                    return;
                }
                waitingForEnemy = true;
                if (ShipManager.Instance.GetEnemyShip(enemyTurn).TryGetComponent(out Enemy enemy))
                    enemy.SetMyTurn();
                else
                    Debug.LogError("MISSING ENEMY COMPONENT");
            }
        }
    }
    private IEnumerator waitForSwapCamp()
    {
        yield return new WaitForSeconds(0.5f);
        actualisedCamp = true;
    }
    private bool CheckEndGame()
    {
        var ally = ShipManager.Instance.GetAllyShipsOrinalCamp();
        var enemy = ShipManager.Instance.GetEnemyShipsOrinalCamp();
        if (enemy.Count == 0)
        {
            ShowWinScreen();
            return true;
        }
        if (ally.Count == 0)
        {
            ShowLoseScreen();
            return true;
        }
        return false;
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
