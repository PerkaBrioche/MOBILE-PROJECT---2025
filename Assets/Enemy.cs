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
  private int _distanceTravelled = 0;
  struct tileDetectedInfo
  {
      public TilesController TileController;
      public int Distance;
  }
  
    
    
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
    }

    #region MOVEMENT
    
    private void MoveInDirection(Func<TilesController, TilesController> direction, TilesController originTiles = null)
    {
        int distance = _unitStats.WalkDistance;
        if(originTiles == null)
        {
            originTiles = _shipController.GetTiles();
        }
        else
        {
            distance--;
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

    private tileDetectedInfo GetPathReturnShip(Func<TilesController, TilesController> directionCenter)
    {
        TilesController centerTile = directionCenter(_shipController.GetTiles());
        while (centerTile != null) // CHECK UP TILE
        {
            var RTile = GetHorizontalTiles(_tilesController => _tilesController.rightTile, centerTile); // SI ENEMI A DROITE
            if (RTile != null)
            {
                return new tileDetectedInfo()
                {
                    TileController = RTile,
                    Distance = _distanceTravelled
                };
            }

            var LTile = GetHorizontalTiles(_tilesController => _tilesController.leftTile, centerTile); // SI ENEMI A DROITE
            if (LTile != null)
            {
                return new tileDetectedInfo()
                {
                    TileController = LTile,
                    Distance = _distanceTravelled
                };
            }
            centerTile = directionCenter(centerTile);
        }
        return new tileDetectedInfo()
        {
            TileController = null,
            Distance = 0
        };
    }

    private void GetPathToTarget(Func<TilesController, TilesController> directionToTarget)
    {
        int walkDistance = _unitStats.WalkDistance-1;
        TilesController targetTile = directionToTarget(_shipController.GetTiles());
        bool foundEnemy = false;
        while (walkDistance > 0)
        {
            if(GetHorizontalTiles(_tilesController => _tilesController.rightTile, targetTile) != null)
            { 
                MoveInDirection(_tilesController => _tilesController.rightTile, targetTile); 
                return;
            }
            if(GetHorizontalTiles(_tilesController => _tilesController.leftTile, targetTile) != null)
            { 
                MoveInDirection(_tilesController => _tilesController.leftTile, targetTile); 
                return;
            }
            walkDistance--;
            targetTile = directionToTarget(targetTile);
        }
        MoveInDirection(tilec => tilec.downTile);
    }
    
    private void FindPathToClosestPlayer()
    {
        _distanceTravelled = 0;
        var UpShip = GetPathReturnShip(_tilesController => _tilesController.upTile);
        print("DISTANCCE TRAVELLED = " + _distanceTravelled);
        _distanceTravelled = 0;
        var DownShip = GetPathReturnShip(_tilesController => _tilesController.downTile);
        _distanceTravelled = 0;
        // GET THE CLOSEST SHIP
        
        
        
        if(UpShip.TileController == null)
        {
            print(DownShip.Distance + " DOWN DISTANCE");
            GetPathToTarget(_tilesController => _tilesController.downTile);
            return;
        }
        if(DownShip.TileController == null)
        {
            print(UpShip.Distance + " UP DISTANCE");
            GetPathToTarget(_tilesController => _tilesController.upTile);
            return; 
        }
        print("DISTANCE / UP = " + UpShip.Distance + " DOWN = " + DownShip.Distance);

        if(UpShip.Distance == DownShip.Distance)
        {
            print ("SAME DISTANCE = " + UpShip.Distance);
            if (UpShip.TileController.GetShipController().runtimeStats.ATK == DownShip.TileController.GetShipController().runtimeStats.ATK) // MEME ATTAQUE
            {
                print ("SAME ATTACK");

                if (UpShip.TileController.GetShipController().runtimeStats.HP == DownShip.TileController.GetShipController().runtimeStats.HP) // MEME HP
                {
                    print ("SAME HP");
                    GetPathToTarget(_tilesController => _tilesController.upTile);
                }
                else
                {
                    GetPathToTarget(UpShip.TileController.GetShipController().runtimeStats.HP > DownShip.TileController.GetShipController().runtimeStats.HP ?
                        _tilesController => _tilesController.downTile : _tilesController => _tilesController.upTile);
                }
            } 
            else
            {
                GetPathToTarget(UpShip.TileController.GetShipController().runtimeStats.ATK > DownShip.TileController.GetShipController().runtimeStats.ATK ?
                    _tilesController => _tilesController.downTile : _tilesController => _tilesController.upTile);
            }
            return;
        }
        
        
        GetPathToTarget(UpShip.Distance > DownShip.Distance ? _tilesController => _tilesController.downTile : _tilesController => _tilesController.upTile);
    }

    private TilesController GetHorizontalTiles(Func<TilesController, TilesController> direction, TilesController originTiles)
    {
        TilesController nextTile = direction(originTiles);
        while (nextTile != null)
        {
            _distanceTravelled++;
            print(" ACTUAL DISTANCE TRAVVELLED = " + _distanceTravelled);
            if (nextTile.HasAnEnemy())
            {
                return nextTile;
            }
            nextTile = direction(nextTile);
        }
        return null;
        
    }

    
}

