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
        _isCooldown = true;
        StartCoroutine(UpdateCooldown());
        UpdateLists();
    }

    private void UpdateLists()
    {
        ClearShips();
        var AllShips = GameObject.FindGameObjectsWithTag("Ship");
        foreach (var ship in AllShips)
        {
            var shipController = ship.GetComponent<ShipController>();
            if (shipController.IsAnEnemy())
            {
                // ADDS ENEMY SHIPS
                _listEnemyShips.Add(shipController);
            }
            else
            {
                // ADDS ALLY SHIPS
                _listaAllyShips.Add(shipController);
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
}
