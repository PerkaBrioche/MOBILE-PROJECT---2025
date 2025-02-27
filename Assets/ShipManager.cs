using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipManager : MonoBehaviour
{
    [SerializeField] private List<ShipController> _listaAllyShips = new List<ShipController>();
    [SerializeField] private List<ShipController> _listEnemyShips = new List<ShipController>();
    
    public static ShipManager Instance;
    
    private bool _isCooldown = false;
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

    private void Update()
    {
        if (_isCooldown)
        {
            return;
        }
        if(!TurnManager.Instance.IsPlayerTurn()) { return;}
        if (HasPlayedAllShips() && !CombatManager.Instance.IsInCombat())
        {
            TurnManager.Instance.CheckUnlockButton();
        }


    }

    private void UpdateLists()
    {
        ClearShips();
        var AllShips = FindObjectsByType<ShipController>(FindObjectsSortMode.None);
        foreach (var ship in AllShips)
        {
            if (ship.IsAnEnemy())
            {
                // ADDS ENEMY SHIPS
                _listEnemyShips.Add(ship);
            }
            else
            {
                // ADDS ALLY SHIPS
                _listaAllyShips.Add(ship);
            }
        }
    }

    public void DisableEnemyColliders()
    {
        foreach (var ship in _listEnemyShips)
        {
            ship.ChangeCollider(false);
        }
    }

    public void EnableColldiersEnemy()
    {
        foreach (var ship in _listEnemyShips)
        {
            ship.ChangeCollider(true);
        }
    }

    private IEnumerator UpdateCooldown()
    {
        yield return new WaitForSeconds(1);
        _isCooldown = false;
    }

    private void ClearShips()
    {
        _listaAllyShips.Clear();
        _listEnemyShips.Clear();
    }

    public bool HasPlayedAllShips()
    {
        UpdateLists();
        foreach (var ship in _listaAllyShips)
        {
            if(!ship.IsLocked())
            {
                return false;
            }
        }
        return true;
    }
    
    public void ResetAllShips()
    {
        var AllShips = FindObjectsByType<ShipController>(FindObjectsSortMode.None);
        print("RESET ALL SHIPS");
        foreach (var ships in AllShips)
        {
            ships.ResetShip();
        }
    }
    
    public ShipController GetEnemyShip(int index)
    {
        UpdateLists();
        return _listEnemyShips[index];
    }
    public ShipController GetActualAllyShip(int index)
    {
        UpdateLists();
        return _listaAllyShips[index];
    }
    public int GetActualEnemyShipsCount()
    {
        UpdateLists();
        return _listEnemyShips.Count;
    }
    
    public List<ShipController> GetActualEnemyShips()
    {
        UpdateLists();
        return _listEnemyShips;
    }

    public void ChangeShipsCamp()
    {
        var AllShips = FindObjectsByType<ShipController>(FindObjectsSortMode.None);
        foreach (var ships in AllShips)
        {
            ships.ChangeCamp();
        }
    }
    
    public List<ShipController> GetActualAllyShips()
    {
        UpdateLists();
        return _listaAllyShips;
    }
    
    public int GetAllyShip()
    {
        UpdateLists();
        return _listaAllyShips.Count;
    }

    public List<ShipController> GetAllyShipsOrinalCamp()
    {
        var AllShips = FindObjectsByType<ShipController>(FindObjectsSortMode.None);
        List<ShipController> allyShips = new List<ShipController>();
        foreach (var ship in AllShips)
        {
            if (!ship.IsOriginCampEnemy())
            {
                allyShips.Add(ship);
            }
        }
        return allyShips;
    }
    
    public List<ShipController> GetEnemyShipsOrinalCamp()
    {
        var AllShips = FindObjectsByType<ShipController>(FindObjectsSortMode.None);
        List<ShipController> enemyShips = new List<ShipController>();
        foreach (var ship in AllShips)
        {
            if (ship.IsOriginCampEnemy())
            {
                enemyShips.Add(ship);
            }
        }
        return enemyShips;
    }
    
    
}
