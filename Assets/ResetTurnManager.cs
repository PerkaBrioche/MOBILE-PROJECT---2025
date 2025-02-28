using UnityEngine;

public class ResetTurnManager : MonoBehaviour
{
    public static ResetTurnManager Instance;

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

    public void RecordStartingPositions()
    {
        ShipController[] ships = Resources.FindObjectsOfTypeAll<ShipController>();
        foreach (ShipController ship in ships)
        {
            if (ship.gameObject.scene.isLoaded)
            { 
                //ship.SaveStartingState();
            }
        }
    }

    public void ResetTurn()
    {
        if (!TurnManager.Instance.IsPlayerTurn())
        {
            return;
        }
        ShipController[] ships = Resources.FindObjectsOfTypeAll<ShipController>();
        foreach (ShipController ship in ships)
        {
            if (ship.gameObject.scene.isLoaded)
            {
               //ship.ResetTurnState();
            }
        }
    }
}