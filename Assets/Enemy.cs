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

  private ShipController _targetShips;
  
  
    
    
    private void Awake()
    {
        _gridController = FindObjectOfType<GridController>();
    }

    private void Start()
    {
        _shipController = GetComponent<ShipController>();
        _unitStats = _shipController.GetUnitStats();
    }
    
    public virtual void SetMyTurn()
    {
        if (_shipController.IsLocked())
        {
            TurnManager.Instance.EnemyEndATurn();
            EndTurn();
            return;
        }
        _shipController.GetPath();
        StartCoroutine(Wait(1.5f));
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
            if (tile.HasAnEnemy() && tile.IsAnAttackTile())
            {
                enemyOnTile.Add(tile.GetShipController());
                canMoove = false;
            }
        }
        print("TOTAL ENEMY DETECTED = " + enemyOnTile.Count);
        print("CanMooe = " + canMoove);

        if (canMoove)
        {
            if (_shipController.HasMoved())
            {
                _shipController.SetLockMode(true);
                TurnManager.Instance.EnemyEndATurn();
                EndTurn();
                return;
            }
            MoveAction();
            EndTurn();
        }
        
        else if (enemyOnTile.Count > 0)
        {
            // GET THE LOWEST ENEMY LIFE

            ShipController lowestLife = null;
            foreach (var enemy in enemyOnTile)
            {
                if (lowestLife == null)
                {
                    lowestLife = enemy;
                }
                else
                {
                    if (enemy.GetLife() < lowestLife.GetLife())
                    {
                        lowestLife = enemy;
                    }
                }
            }
            _targetShips = lowestLife;
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

    private void MoveAction()
    {
        FindPathToClosestPlayer();
        return;
        if(_targetShips != null)
        {
            FindPathToClosestPlayer();
            return;
        }
        MoveInDirection(_tilesDetected => _tilesDetected.downTile);
    }

    #region MOVEMENT
    
    private void MoveInDirection(Func<TilesController, TilesController> direction, TilesController originTiles = null)
    {
        int distance = _unitStats.WalkDistance;
        if(originTiles == null)
        {
            originTiles = _shipController.GetTiles();
        }
        TilesController directionTile = direction(originTiles);
        for (int i = 1; i < distance; i++)
        {
            if (directionTile != null)
            {
                directionTile = direction(directionTile);
            }
        }
        if (directionTile != null)
        {
            Move(directionTile);
        }
    }
    #endregion

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
    }
    
    private void FindPathToClosestPlayer()
    {
        int walkDistance = _unitStats.WalkDistance;

        TilesController centerTile = _shipController.GetTiles();
        while (centerTile != null) // CHECK UP TILE
        {
            if(GetHorizontalTiles(_tilesController => _tilesController.rightTile, centerTile) != null) // SI ENEMI A DROITE
            {
                if(walkDistance > 0)
                {
                    MoveInDirection(_tilesController => _tilesController.rightTile, centerTile);
                    return;
                }
                MoveInDirection(_tilesController => _tilesController.upTile);
                return;
            }
            if(GetHorizontalTiles(_tilesController => _tilesController.leftTile, centerTile) != null) // SI ENEMI A GAUCHE
            {
                if(walkDistance > 0)
                {
                    MoveInDirection(_tilesController => _tilesController.leftTile, centerTile);
                    return;
                }
                MoveInDirection(_tilesController => _tilesController.upTile);
                return;
            }

            walkDistance--;
            centerTile = centerTile.upTile;
        }
        centerTile = _shipController.GetTiles();
        walkDistance = _unitStats.WalkDistance;
        while (centerTile != null) // CHECK UP TILE
        {
            if(GetHorizontalTiles(_tilesController => _tilesController.rightTile, centerTile) != null) // SI ENEMI A DROITE
            {
                if(walkDistance > 0)
                {
                    MoveInDirection(_tilesController => _tilesController.rightTile, centerTile);
                    return;
                }
                MoveInDirection(_tilesController => _tilesController.downTile);
                return;
            }
            if(GetHorizontalTiles(_tilesController => _tilesController.leftTile, centerTile) != null) // SI ENEMI A GAUCHE
            {
                if(walkDistance > 0)
                {
                    MoveInDirection(_tilesController => _tilesController.leftTile, centerTile);
                    return;
                }
                MoveInDirection(_tilesController => _tilesController.downTile);
                return;
            }
            walkDistance--;
            centerTile = centerTile.downTile;
        }
    }

    private TilesController GetHorizontalTiles(Func<TilesController, TilesController> direction, TilesController originTiles)
    {
        TilesController nextTile = direction(originTiles);
        while (nextTile != null)
        {
            if (nextTile.HasAnEnemy())
            {
                return nextTile;
            }
            nextTile = direction(nextTile);
        }
        return null;
        
    }

    
}

