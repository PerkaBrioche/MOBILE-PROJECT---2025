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
    
    struct closestPositon
    {
        public TilesController TileController;
        public Vector2 position;
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
            MoveAction();
            EndTurn();
        }
        else if (enemyOnTile.Count > 0)
        {
            ShipController lowestLife = null;
            foreach (var enemy in enemyOnTile)
            {
                if (lowestLife == null)
                    lowestLife = enemy;
                else if (enemy.GetLife() < lowestLife.GetLife())
                    lowestLife = enemy;
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
        if (originTiles == null)
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
                directionTile = direction(directionTile);
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
    private void GetPathToTarget(TilesController tileTarget)
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
            print("MOVE TO BEST TILE = " + bestTile.name + " DISTANCE = " + bestDistance);
            Move(bestTile);
        }
        else
        {
            MoveInDirection(t => t.downTile);
        }
    }

    private void FindPathToClosestPlayer()
    {
        List<ShipController> allyShips = ShipManager.Instance.GetAllyShipsOrinalCamp();
        if (allyShips == null || allyShips.Count == 0)
        {
            MoveInDirection(t => t.downTile);
            return;
        }
        
        TilesController enemyTile = _shipController.GetTiles();
        ShipController closestShip = null;
        int minDistance = int.MaxValue;
        foreach (ShipController ally in allyShips)
        {
            TilesController allyTile = ally.GetTiles();
            if (allyTile == null)
                continue;
            print("ALLY- " + ally.name);

            int distance = CalculateManhattanDistance(allyTile, enemyTile);
            print("DISTANCE = " + distance);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestShip = ally;
            }
            else if (distance == minDistance)
            {
                if (ally.runtimeStats.ATK > closestShip.runtimeStats.ATK)
                {
                    closestShip = ally;
                }
                else if (ally.runtimeStats.ATK == closestShip.runtimeStats.ATK)
                {
                    print("SAME DAMAGE");
                    if (ally.runtimeStats.HP > closestShip.runtimeStats.HP)
                    {
                    }
                    else
                    {
                        closestShip = ally;
                    }
                }
            }
        }
        
        if (closestShip == null)
        {
            MoveInDirection(t => t.downTile);
            return;
        }
        print("closest ship = " + closestShip.name);
        
        TilesController targetTile = closestShip.GetTiles();
        // int dx = targetTile.GetColumnPosition() - enemyTile.GetColumnPosition();
        // int dy = targetTile.GetRowPosition() - enemyTile.GetRowPosition(); 
        //
        // Func<TilesController, TilesController> direction;
        // print("DX = " + dx + " DY = " + dy);
        // if (Mathf.Abs(dx) >= Mathf.Abs(dy))
        // {
        //     direction = dx > 0 ? (t => t.rightTile) : (t => t.leftTile);
        // }
        // else
        // {
        //     direction = dy < 0 ? (t => t.upTile) : (t => t.downTile);
        // }
        //
        GetPathToTarget(closestShip.GetTiles());
    }
    private int CalculateManhattanDistance(TilesController a, TilesController b)
    {
        int dx = Mathf.Abs(a.GetColumnPosition() - b.GetColumnPosition());
        int dy = Mathf.Abs(a.GetRowPosition() - b.GetRowPosition());
        return dx + dy;
    }
}
