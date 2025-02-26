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
            if (tile == null)
                continue;
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
            
            _shipController.SetHasMoved(true);
            
            if (targetile != null)
            {
                Move(targetile);
            }
            else
            {
                Move(FindBestTile(FindClosestEnemy().GetTiles()));
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
            if (_shipController.GetType() != ShipSpawner.shipType.Rider || _shipController.HasMoved())
            {
                _shipController.SetLockMode(true);
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
    public void MoveInDirection(Func<TilesController, TilesController> direction, TilesController originTiles = null, int maxtime = 0)
    {
        int distance = _unitStats.WalkDistance;
        if (originTiles == null)
        {
            originTiles = _shipController.GetTiles();
        }
        else
        {
            distance--;
        }
        
        TilesController directionTile = direction(originTiles);
        if (!IsTileValid(directionTile)) 
        { 
            Move(originTiles);
            return;
        }
        
        for (int i = 1; i < distance; i++)
        {
            if (IsTileValid(directionTile)) 
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

    private bool IsTileValid(TilesController t)
    {
        print(t + " TILE " + t.IsBlocked() + " " + t.HasAnEnemy() + " " + t.HasAnAlly());

        if (t != null)
        {
            if (!t.IsBlocked() && !t.HasAnEnemy() && !t.HasAnAlly())
            {
                return true;
            }
            return false;
        }
        return false;
    }
    

    
    private IEnumerator WaitAnimationFight()
    {
        yield return new WaitForSeconds(3); 
        if(_shipController.GetType() != ShipSpawner.shipType.Rider)
        {
            TurnManager.Instance.EnemyEndATurn();
        }
        else if (!_shipController.HasMoved()) // RIDER
        {
            GoCoward();
        }
        else
        {
            TurnManager.Instance.EnemyEndATurn();
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

    public ShipController FindClosestEnemy()
    {
        List<ShipController> allyShips = ShipManager.Instance.GetAllyShipsOrinalCamp();
        if (allyShips == null || allyShips.Count == 0)
        {
            return null;
        }
        
        TilesController enemyTile = _shipController.GetTiles();
        ShipController closestShip = null;
        int minDistance = int.MaxValue;
        foreach (ShipController ally in allyShips)
        {
            TilesController allyTile = ally.GetTiles();
            if (allyTile == null) { continue; }
            
            int distance = CalculateManhattanDistance(allyTile, enemyTile);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestShip = ally;
            }
            else if (distance == minDistance)
            {
                if(_shipController.GetType() == ShipSpawner.shipType.SpacceBerzerker)
                {
                    if (ally.runtimeStats.HP < closestShip.runtimeStats.HP)
                    {
                        closestShip = ally;
                    }
                    else if (ally.runtimeStats.HP == closestShip.runtimeStats.HP)
                    {
                        if (ally.runtimeStats.ATK < closestShip.runtimeStats.ATK)
                        {
                            closestShip = ally;
                        }
                    }
                }
                else // PAS BERSERKER
                {
                    if (ally.runtimeStats.ATK < closestShip.runtimeStats.ATK)
                    {
                        closestShip = ally;
                    }
                    else if (ally.runtimeStats.ATK == closestShip.runtimeStats.ATK)
                    {
                        if (ally.runtimeStats.HP < closestShip.runtimeStats.HP)
                        {
                            closestShip = ally;
                        }
                    }
                }
            }
        }
        
        if (closestShip == null)
        {
            return  (null);
        }

        return closestShip;
    }
    protected int CalculateManhattanDistance(TilesController a, TilesController b)
    {
        int dx = Mathf.Abs(a.GetColumnPosition() - b.GetColumnPosition());
        int dy = Mathf.Abs(a.GetRowPosition() - b.GetRowPosition());
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


