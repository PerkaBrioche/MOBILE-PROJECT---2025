using System;
using System.Collections;
using UnityEngine;

public class Rider : Enemy
{
    private TilesController _myTile;
    private ShipController _shipController;

    private void Start()
    {
        base.Start();
        _shipController = GetComponent<ShipController>();
    }
    
    public override void SetMyTurn()
    {
        base.SetMyTurn();
        StartCoroutine(Wait(1.5f));
    }
    
    private IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);
        CheckPath();
    }
    
    public override void GoCoward()
    {
        _shipController.GetPath();
       StartCoroutine(CowarWait());
    }
    
    private IEnumerator CowarWait()
    {
        yield return new WaitForSeconds(1.5F);
        GoOpossite();
    }


    public void GoOpossite()
    {
        ShipController closestEnemy = FindClosestEnemy();
        TilesController enemyTile = closestEnemy != null ? closestEnemy.GetTiles() : null;
        TilesController myTile = _shipController.GetTiles();
        
        Func<TilesController, TilesController> retreatDirection;
        if(enemyTile != null && myTile != null)
        {
            retreatDirection = GetOpossiteDirection(enemyTile, myTile);
            MoveInDirection(retreatDirection);
        }
        else
        {
            MoveInDirection(t => t.upTile);
        }

        _shipController.SetHasMoved(true);
        _shipController.SetLockMode(true);
        SetMyTurn();
    }
}

