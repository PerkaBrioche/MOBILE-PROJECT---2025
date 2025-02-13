using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private ShipController _shipController;
    private UnitStats _unitStats;
    
    private bool canAction = true;
    private GridController _gridController;
    
    private TilesController[] _tilesDetected;
    
    
    private void Awake()
    {
        _gridController = FindObjectOfType<GridController>();
    }

    private void Start()
    {
        _shipController = GetComponent<ShipController>();
        _unitStats = _shipController.GetStats();
    }
    
    public virtual void SetMyTurn()
    {
        _shipController.GetPath();
        StartCoroutine(Wait(1f));
    }


    private void CheckPath()
    {
        _tilesDetected = EnemyManager.Instance.GetTiles();
        print(_tilesDetected.Length);
        bool canMoove = true;
        foreach (var tile in _tilesDetected)
        {
            if (tile == null)
            {
                continue;
            }
            if (tile.HasAnEnemy() && tile.IsAnAttackTile())
            {
                Attack(tile.GetShipController());
                TurnManager.Instance.EnemyEndATurn();
                canMoove = false;
            }
        }

        if (canMoove)
        {
            MoveFoward();
        }
        
        EndTurn();
    }
    
    public virtual void Attack(ShipController sc)
    {
        sc.Die();
        Debug.Log("Enemy attack");
    }

    public virtual void Move(TilesController tilesController)
    {
        _shipController.SetNewPosition(tilesController);
    }

    public virtual void MoveFoward()
    {
        var fowardTile = _shipController.GetTiles().downTile;
        if (fowardTile != null)
        {
            Move(fowardTile);
        }
    }
    
    public virtual void EndTurn()
    {
        _gridController.ResetAllTiles();
    }
    
    private IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time); 
        CheckPath();
    }
}

