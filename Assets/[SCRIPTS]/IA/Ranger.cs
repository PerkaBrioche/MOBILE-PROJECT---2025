using System;
using System.Collections;
using UnityEngine;

public class Ranger : Enemy
{
    private TilesController _myTile;
    private ShipController _closestEnemy;
    int _distanceToEnemy = 0;
    
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
        if (_shipController.HasMoved())
        {
            PlayPathAutomatically();
        }
        else
        { 
            CheckCondition();
        }
    }
    private void CheckCondition()
    {
        _myTile = _shipController.GetTiles();
        _closestEnemy = FindClosestEnemy();
        int distanceTarget = CalculateManhattanDistance(_closestEnemy.GetTiles(), _shipController.GetTiles());
        if (distanceTarget <= 0)
        {
            print("TOO CLOSE");
            MoveInDirection(GetOpossiteDirection(_closestEnemy.GetTiles(), _myTile), _shipController.GetTiles());
            EndTurn();
        }
        else
        {
            print("PLAY AUTOMATICALLY");
            PlayPathAutomatically( FindBestTile(_closestEnemy.GetTiles()));
        }
    }
}
