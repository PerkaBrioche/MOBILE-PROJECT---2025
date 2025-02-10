using TMPro;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    
    private bool _isPlayerTurn = false;
    private bool _isEnemyTurn = false;
    public static TurnManager Instance;
    
    [SerializeField] private TextMeshProUGUI _turnText;
    
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
    
    public void StartPlayerTurn()
    {
        UpdateText("Player Turn", Color.green);
    }

    public void EndPlayerTurn()
    {
        
        StartEnemyTurn();
    }
    
    public void StartEnemyTurn()
    {
        UpdateText("Enemy Turn", Color.red);
    }
    
    public void EndEnemyTurn()
    {
        
        StartPlayerTurn();
    }
    
    private void UpdateText(string text, Color color)
    {
        _turnText.text = text;
        _turnText.color = color;
    }
    
}
