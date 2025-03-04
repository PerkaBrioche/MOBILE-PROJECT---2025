using UnityEngine;
using System.Collections.Generic;

public class ResetTurnManager : MonoBehaviour
{
    public static ResetTurnManager Instance;
    private List<ShipController> recordedShips = new List<ShipController>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public void RecordStartingPositions()
    {
        recordedShips.Clear();
        ShipController[] ships = Resources.FindObjectsOfTypeAll<ShipController>();
        foreach (ShipController ship in ships)
        {
            if (ship.gameObject.activeInHierarchy)
            {
                ship.SaveStartingState();
                if (!recordedShips.Contains(ship))
                    recordedShips.Add(ship);
            }
        }
    }

    public void ResetTurn()
    {
        if (!TurnManager.Instance.IsPlayerTurn() || TurnManager.Instance.IsEndGame())
            return;
        foreach (ShipController ship in recordedShips)
        {
            if (ship.WasAliveAtTurnStart)
            {
                if (ship.IsAnEnemy())
                {
                    if (!ship.gameObject.activeInHierarchy)
                        ship.gameObject.SetActive(true);
                    ship.ResetTurnState();
                }
                else
                {
                    if (ship.gameObject.activeInHierarchy)
                        ship.ResetTurnState();
                }
            }
        }
    }
}