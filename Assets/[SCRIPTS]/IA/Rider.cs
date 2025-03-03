using System;
using System.Collections;
using UnityEngine;

public class Rider : Enemy
{
    private TilesController _myTile;
    private ShipController _shipController;
    
    private bool _reset = false;

    private void Start()
    {
        _reset = true;
        base.Start();
        _shipController = GetComponent<ShipController>();
    }
    
    public override void SetMyTurn()
    {
        base.SetMyTurn();
        if (!_shipController.IsLocked())
        {
            StartCoroutine(Wait(1.5f));
        }
    }
    
    private IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);
        PlayPathAutomatically();
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
        print("GO OPPOSITE");
        ShipController closestEnemy = FindClosestEnemy()[0];
        TilesController enemyTile = closestEnemy != null ? closestEnemy.GetTiles() : null;
        TilesController myTile = _shipController.GetTiles();
        Func<TilesController, TilesController> retreatDirection;
        if(enemyTile != null && myTile != null)
        {
            retreatDirection = GetOpossiteDirection(enemyTile, myTile);
            MoveInDirection(retreatDirection, _shipController.GetTiles());
        }
        else
        {
            MoveInDirection(t => t.upTile, _shipController.GetTiles());
        }
        _shipController.SetLockMode(true);
        EndTurn();
    }
}

