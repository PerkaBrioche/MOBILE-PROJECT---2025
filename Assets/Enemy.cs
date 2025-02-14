using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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


    public void CheckPath()
    {
        _tilesDetected = EnemyManager.Instance.GetTiles();
        bool canMoove = true;
        List<ShipController> enemyOnTile = new List<ShipController>();
        enemyOnTile.Clear();
        foreach (var tile in _tilesDetected)
        {
            if (tile == null)
            {
                continue;
            }
            if (tile.HasAnEnemy())
            {
                enemyOnTile.Add(tile.GetShipController());
                canMoove = false;
            }
        }
        if (canMoove)
        {
            if (_shipController.HasMoved() || _shipController.IsLocked())
            {
                _shipController.SetLockMode(true);
                TurnManager.Instance.EnemyEndATurn();
                EndTurn();
                return;
            }
            MoveFoward();
        }
        else if (enemyOnTile.Count > 0)
        {
            // GET THE LOWEST ENEMY LIFE
            print("TOTAL ENEMY DETECTED = " + enemyOnTile.Count);

            ShipController lowestLife = null;
            foreach (var enemy in enemyOnTile)
            {
                if (lowestLife == null)
                {
                    lowestLife = enemy;
                }
                else
                {
                    if (enemy.GetStats().HP < lowestLife.GetStats().HP)
                    {
                        lowestLife = enemy;
                    }
                }
            }
            Attack(lowestLife);
            _shipController.SetLockMode(true);
            StartCoroutine(WaitAnimationFight());
        } 
        EndTurn();

    }
    
    public void Attack(ShipController sc)
    {
        CombatManager.Instance.StartCombat(_shipController, sc);
    }

    public virtual void Move(TilesController tilesController)
    {
        _shipController.SetNewPosition(tilesController);
    }

    public virtual void MoveFoward()
    {
        int distance = _unitStats.WalkDistance;
        TilesController fowardTile = _shipController.GetTiles().downTile;
        for (int i = 1; i < distance; i++)
        {
            if (fowardTile != null)
            {
                fowardTile = fowardTile.downTile;
            }
        }
        if (fowardTile != null)
        {
            Move(fowardTile);
        }
    }
    
    public virtual void EndTurn()
    {
        _gridController.ResetAllTiles();
        EnemyManager.Instance.ClearTiles();
    }
    
    private IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time); 
        CheckPath();
    }

    private IEnumerator WaitAnimationFight()
    {
        yield return new WaitForSeconds(3); 
        TurnManager.Instance.EnemyEndATurn();
    }}

