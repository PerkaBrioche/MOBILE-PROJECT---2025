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

    private int _enemyTurn;
    private bool _waitingForEnemy = false;

    private bool _actualisedCamp = false;
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    
    private void Start()
    {
        StartPlayerTurn();
        LockButtonTurn();
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
        {
            StartEnemyTurn();
        }
    }
    
    public void StartEnemyTurn()
    {
        _actualisedCamp = false;
        StartCoroutine(waitForSwapCamp());
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
        {
            UnlockButtonTurn();
        }
    }

    public void EnemyEndATurn()
    {
        print("ENEMY END A TURN");
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
                if(ShipManager.Instance.GetActualAllyShip(_enemyTurn).TryGetComponent(out Enemy enemy))
                {
                    enemy.SetMyTurn();
                }
                else
                {
                    Debug.LogError("MISSING ENEMY COMPONENT");
                }
            }

        }
    }

    private void LockButtonTurn()
    {
        _turnButton.interactable = false;
    }


    public bool IsPlayerTurn()
    {
        return _isPlayerTurn;
    }
    
    public bool IsEnemyTurn()
    {
        return _isEnemyTurn;
    }

    private IEnumerator waitForSwapCamp()
    {
        yield return new WaitForSeconds(0.5f);
        _actualisedCamp = true;
    }

    private bool CheckEndGame()
    {

        var ally = ShipManager.Instance.GetAllyShipsOrinalCamp();
        var enemy = ShipManager.Instance.GetEnemyShipsOrinalCamp();
        
        if (enemy.Count > 0)
        {
        }
        else
        {
            WinGame();
            return true;
        }
        
        if(ally.Count > 0)
        {
        }
        else
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
}
