using System;
using System.Collections;
using UnityEngine;

public class Fortress : Enemy
{
    private TilesController _myTile;
    private ShipController _closestEnemy;
    int _distanceToEnemy = 0;
    
    private ShipController _shipController;
    private int _AggroTime = 0;

    private void Start()
    {
        base.Start();
        _shipController = GetComponent<ShipController>();
    }
    public override void SetMyTurn()
    {
        base.SetMyTurn();
        if (_shipController.IsLocked())
        {
            return;
        }
        StartCoroutine(Wait(1.5f));
    }
    
    private IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);
        if (!_shipController.IsLocked())
        {
            CheckCondition();
        }
        else
        {
            EndTurn();
        }
    }
    private void CheckCondition()
    {
        _myTile = _shipController.GetTiles();
        _closestEnemy = FindClosestEnemy()[0];
        int distanceTarget = CalculateManhattanDistance(_closestEnemy.GetTiles(), _shipController.GetTiles());
        if (_AggroTime > 0)
        {
            _AggroTime--;
            PlayPathAutomatically( FindBestTile(_closestEnemy.GetTiles()));
        }
        else // STAY BACK
        {
            if (distanceTarget <= 1)
            {
                _AggroTime = 2;
                MoveInDirection(GetOpossiteDirection(_closestEnemy.GetTiles(), _myTile));
                EndTurn();
            }
            else
            {
                PlayPathAutomatically( FindBestTile(_closestEnemy.GetTiles()));
            }
        }

    }
}
