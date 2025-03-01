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
    
    public TilesController targetTile;
    public void Start()
    {
        _shipController = GetComponent<ShipController>();
        _unitStats = _shipController.GetUnitStats();
    }

    private void Awake()
    {
        _gridController = FindObjectOfType<GridController>();
    }
    
    public virtual void SetMyTurn()
    {
        if (TurnManager.Instance.IsPlayerTurn())
        {
            return;
        }
        if (_shipController.IsLocked())
        {
            print("IL EST LOCK C'EST COOKED POUR TOI");
            TurnManager.Instance.EnemyEndATurn();
            EndTurn();
            return;
        }
        _shipController.GetPath();
    }

    protected virtual void PathUpdated()
    {
        
    }
    public void PlayPathAutomatically(TilesController targetile = null)
    {
        _tilesDetected = EnemyManager.Instance.GetTiles();
        bool canMoove = true;
        List<ShipController> enemyOnTile = new List<ShipController>();
        enemyOnTile.Clear();
        foreach (var tile in _tilesDetected)
        {
            if (tile == null) {continue;}
            if (tile.HasAnEnemy() && tile.IsAnAttackTile())
            {
                print("ADD ENEMY ON TILE OUAHHH"); 
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
            
            _shipController.SetHasMoved(true);
            
            ShipController closestEnemy = FindClosestEnemy()[0];

            if (targetile != null)
            {
                Move(targetile);
            }
            else
            {
                if (closestEnemy != null)
                {
                    Move(FindBestTile(closestEnemy.GetTiles()));
                }
                else
                {
                    EndTurn();
                    TurnManager.Instance.EnemyEndATurn();
                }
            }
            EndTurn();
        }
        else if (enemyOnTile.Count > 0)
        {
            if (_shipController.IsInLockDown())
            {
                EndTurn();
                TurnManager.Instance.EnemyEndATurn();
                return;
            }
            ShipController lowestLife = null;

            foreach (var enemy in enemyOnTile)
            {
                if (lowestLife == null)
                {
                    lowestLife = enemy;
                }
                else if (enemy.GetLife() < lowestLife.GetLife()) // NEW ENEMY < OLD ENEMY // COMPARE LIFE // SI L'ANCIEN A PLUS DE VIE ON CHANGE
                {
                    lowestLife = enemy;
                }else if (enemy.GetLife() == lowestLife.GetLife())
                {
                    if (enemy.GetAttack() > lowestLife.GetAttack()) // NEW ENEMY > OLD ENEMY // COMPARE DAMAGE // SI L'ANCIEN A MOINS DE ATTAQUE ON CHANGE
                    {
                        lowestLife = enemy;
                    }
                }
            }
            _targetShips = lowestLife;
            Attack(lowestLife);
            _shipController.SetHasAttacked(true);
            if (_shipController.GetType() != ShipSpawner.shipType.Rider)
            {
                _shipController.SetLockMode(true);
            }
            else
            {
                _shipController.SetHasMoved(false);
            }
            StartCoroutine(WaitAnimationFight());
        }
        EndTurn();
    }

    protected virtual void Attack(ShipController sc)
    {
        CombatManager.Instance.StartCombat(_shipController, sc);
        HasAttacked();
    }

    public virtual void HasAttacked(){}
    public virtual void Move(TilesController tilesController)
    {
        _shipController.SetNewPosition(tilesController);
    }

    public virtual void TankBlocked() { }
    
    #region MOVEMENT
    public void MoveInDirection(Func<TilesController, TilesController> direction, TilesController originTiles = null)
    {
        var listEnemyClosest = FindClosestEnemy();
        foreach (var pos in listEnemyClosest)
        {
        }
        TilesController finalTile = null;
        int distance = _unitStats.WalkDistance;
        if (originTiles == null)
        {
            originTiles = _shipController.GetTiles();
        }
        else
        {
            distance--;
        }
        finalTile = originTiles;
        
        TilesController directionTile = direction(originTiles);
        if (!IsTileValid(directionTile)) // BLOCKED AGAINST WALL
        {
            for (int i = 1; i < listEnemyClosest.Count; i++)
            {
                var retreatDirection = GetOpossiteDirection(listEnemyClosest[i].GetTiles(), originTiles);
                var newTile = retreatDirection(originTiles);
                print("originTiles = " + originTiles + " listEnemyClosest[i].GetTiles() = " + listEnemyClosest[i].GetTiles() + " retreatDirection = " + retreatDirection);

            //    print("TRY WITH THE NEXT ENEMY + " + listEnemyClosest[i].GetUnitStats().name + " TILE = " + newTile);
                if (!IsTileValid(newTile))
                {
                    print("BLOCKED AGAINST WALL");
                }
                else
                {
                    directionTile = newTile;
                    finalTile = directionTile;
                    break;
                }
            }
        }

        if (!IsTileValid(directionTile))// SI APRES TOUT CA TOUJOURS PAS BON
        {
            Move(originTiles);
            print("BLOCKED AGAINST WALL FINAL");
            return;
        }
        for (int i = 0; i < distance; i++)
        {
            if (IsTileValid(directionTile)) 
            {
                finalTile = directionTile;
                directionTile = direction(directionTile);
            }
        }
        if (finalTile != null)
        {
            Move(finalTile);
        }
    }
    #endregion

    public virtual void EndTurn()
    {
        _gridController.ResetAllTiles();
        EnemyManager.Instance.ClearTiles();
    }

    private bool IsTileValid(TilesController t)
    {
        if (t != null)
        {
            print(t + " TILE " + t.IsBlocked() + " " + t.HasAnEnemy() + " " + t.HasAnAlly());
            if (!t.IsBlocked() && !t.HasAnEnemy() && !t.HasAnAlly())
            {
                return true;
            }
            return false;
        }
        else
        {
            print("TILE NULL BRUH");
        }
        return false;
    }
    

    
    private IEnumerator WaitAnimationFight()
    {
        yield return new WaitForSeconds(0); 
        if(_shipController.GetType() != ShipSpawner.shipType.Rider)
        {
            TurnManager.Instance.EnemyEndATurn();
        }
        else // RIDER
        {
            GoCoward();
        }
    }

    public virtual void GoCoward()
    {
        
    }

    public bool IsEnemyInrange()
    {
        List<TilesController> attackTile = new List<TilesController>();
        foreach (var t in EnemyManager.Instance.GetTiles())
        {
            if (t.IsAnAttackTile())
            {
                attackTile.Add(t);
            }
        }
        bool isInRange = false;
        foreach (var t in attackTile)
        {
            if (t.HasAnEnemy())
            {
                isInRange = true;
            }
        }
        return isInRange;
    }
    
    
    public TilesController FindBestTile(TilesController tileTarget)
    {
        List<TilesController> walkTiles = new List<TilesController>();
        foreach (var t in EnemyManager.Instance.GetTiles())
        {
            if (t.IsRangeTile())
            {
                walkTiles.Add(t);
            }
        }
        
        TilesController bestTile = null;
        float bestDistance = float.MaxValue;
        Vector2 targetPos = new Vector2(tileTarget.GetColumnPosition(), tileTarget.GetRowPosition());
    
        foreach (var t in walkTiles)
        {
            Vector2 tilePos = new Vector2(t.GetColumnPosition(), t.GetRowPosition());
            float distance = Mathf.Abs(tilePos.x - targetPos.x) + Mathf.Abs(tilePos.y - targetPos.y);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTile = t;
            }
        }
    
        if (bestTile != null)
        {
            return (bestTile);
        }
        
        return (null);
    }
    
   public List<ShipController> FindClosestEnemy()
{
    List<ShipController> allyShips = ShipManager.Instance.GetAllyShipsOrinalCamp();
    if (allyShips == null || allyShips.Count == 0)
    {
        return null;
    }
    
    TilesController enemyTile = _shipController.GetTiles();
    
    List<(ShipController ship, int distance)> shipDistances = new List<(ShipController, int)>();
    foreach (ShipController ally in allyShips)
    {
        TilesController allyTile = ally.GetTiles();
        if (allyTile == null)
            continue;
        
        int pathDist = FindPath(enemyTile, allyTile) != null ? FindPath(enemyTile, allyTile).Count : -1;
        print( "PATH DISTANCE OF "+ ally.GetUnitStats().name + " = " + pathDist);
        if (pathDist < 0)
        {
            print("PAS DE CHEMIN POSSIBLE");
            continue;
        }
     //   int distance = CalculateManhattanDistance(allyTile, enemyTile);
        shipDistances.Add((ally, pathDist));
    }
    
    shipDistances.Sort((a, b) =>
    {
        int cmp = a.distance.CompareTo(b.distance);
        if (cmp == 0)
        {
            if (_shipController.GetType() == ShipSpawner.shipType.SpacceBerzerker)
            {
                cmp = a.ship.runtimeStats.HP.CompareTo(b.ship.runtimeStats.HP);
                if (cmp == 0)
                    cmp = a.ship.runtimeStats.ATK.CompareTo(b.ship.runtimeStats.ATK);
            }
            else
            {
                cmp = a.ship.runtimeStats.ATK.CompareTo(b.ship.runtimeStats.ATK);
                if (cmp == 0)
                    cmp = a.ship.runtimeStats.HP.CompareTo(b.ship.runtimeStats.HP);
            }
        }
        return cmp;
    });
    
    List<ShipController> sortedShips = new List<ShipController>();
    foreach (var entry in shipDistances)
    {
        sortedShips.Add(entry.ship);
    }
    
    return sortedShips;
}



    // public List<ShipController> FindClosestEnemy()
    // {
    //     List<ShipController> allyShips = ShipManager.Instance.GetAllyShipsOrinalCamp();
    //     if (allyShips == null || allyShips.Count == 0)
    //     {
    //         return null;
    //     }
    //
    //     TilesController enemyTile = _shipController.GetTiles();
    //     List<(ShipController ship, int distance)> shipDistances = new List<(ShipController, int)>();
    //     
    //     foreach (ShipController ally in allyShips)
    //     {
    //         TilesController allyTile = ally.GetTiles();
    //         if (allyTile == null)
    //             continue;
    //         int distance = CalculateManhattanDistance(allyTile, enemyTile);
    //         shipDistances.Add((ally, distance));
    //     }
    //     shipDistances.Sort((a, b) =>
    //     {
    //         int cmp = a.distance.CompareTo(b.distance);
    //         if (cmp == 0)
    //         {
    //             if (_shipController.GetType() == ShipSpawner.shipType.SpacceBerzerker)
    //             {
    //                 cmp = a.ship.runtimeStats.HP.CompareTo(b.ship.runtimeStats.HP);
    //                 if (cmp == 0)
    //                     cmp = a.ship.runtimeStats.ATK.CompareTo(b.ship.runtimeStats.ATK);
    //             }
    //             else
    //             {
    //                 cmp = a.ship.runtimeStats.ATK.CompareTo(b.ship.runtimeStats.ATK);
    //                 if (cmp == 0)
    //                     cmp = a.ship.runtimeStats.HP.CompareTo(b.ship.runtimeStats.HP);
    //             }
    //         }
    //         return cmp;
    //     });
    //
    //     List<ShipController> sortedShips = new List<ShipController>();
    //     foreach (var entry in shipDistances)
    //     {
    //         print(entry.ship.GetUnitStats().name + " " + entry.distance);
    //         sortedShips.Add(entry.ship);
    //     }
    //     return sortedShips;
    // }

    public List<TilesController> FindPath(TilesController start, TilesController goal) // SCRIPT CHATGPT OPTIONNEL (CONSTRUCTION ET DESTRUCTION DE CHEMIN)
    {
        if (start == goal)
            return new List<TilesController> { start };

        Queue<TilesController> queue = new Queue<TilesController>();
        Dictionary<TilesController, TilesController> parents = new Dictionary<TilesController, TilesController>();
    
        queue.Enqueue(start);
        parents[start] = null; 

        print("START = " + start + " GOAL = " + goal);
        while (queue.Count > 0)
        {
            TilesController current = queue.Dequeue();
            foreach (TilesController neighbor in current.GetNeighbors())
            {
                if (neighbor != null && !parents.ContainsKey(neighbor))
                {
                    if (!neighbor.IsBlocked() && !neighbor.HasAnEnemy() && !neighbor.HasAnAlly() || neighbor == goal || neighbor == start) // TILES BONNE
                    {
                        parents[neighbor] = current; 
                        queue.Enqueue(neighbor);
                    }
                
                    if (neighbor == goal)
                    {
                        print("GOAL FOUND = " + neighbor);
                        List<TilesController> path = new List<TilesController>();
                        TilesController pathTile = goal;
                        while (pathTile != null)
                        {
                            print("PATH TILE = " + pathTile);
                            path.Add(pathTile);
                            pathTile = parents[pathTile];
                        }
                        path.Reverse();
                        return path;
                    }
                }
            }
        }
        return null;
    }

    protected int CalculateManhattanDistance(TilesController a, TilesController b)
    {
        int dx = Mathf.Abs(a.GetColumnPosition() - b.GetColumnPosition());  // ALLY
        int dy = Mathf.Abs(a.GetRowPosition() - b.GetRowPosition());  // ENEMY
        return (dx + dy) -1;
    }

    public Func<TilesController, TilesController> GetOpossiteDirection(TilesController A, TilesController B)
    {
        Vector2 enemyPos = new Vector2(A.GetColumnPosition(), A.GetRowPosition());
        Vector2 myTile = new Vector2(B.GetColumnPosition(), B.GetRowPosition());
        Vector2 diff = enemyPos - myTile;
        Func<TilesController, TilesController> retreatDirection;
        if (Mathf.Abs(diff.x) >= Mathf.Abs(diff.y))
        {
            retreatDirection = diff.x >= 0 ? (t => t.leftTile) : (t => t.rightTile);
        }
        else
        {
            retreatDirection = diff.y >= 0 ? (t => t.upTile) : (t => t.downTile);
            
        }
        return retreatDirection;
    }
}


