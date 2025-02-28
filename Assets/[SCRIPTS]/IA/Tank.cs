using System;
using System.Collections;
using UnityEngine;

public class Tank : Enemy
{
    private TilesController _myTile;
    private ShipController _shipController;
    private bool _isCoward = false;
    private bool _hasbeenCoward = false;
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
        if (_shipController.IsInLockDown() && !_shipController.HasMoved())
        {
            RunAway();
        }
        else
        {
            PlayPathAutomatically();
        }
    }

    private IEnumerator WaitForPathToRunAway()
    {
        yield return new WaitForSeconds(1.5f);
    }
    public override void HasAttacked()
    {
        if (!_isCoward)
        {
            _isCoward = true;
            StartCoroutine(WaitForPathToRunAway());
        }
    }

    private void RunAway()
    {
        switch (_shipController.lockDownLeft())
        {
            case 1:
                PlayPathAutomatically();
                break;
            case 2:
                MoveInDirection(GetOpossiteDirection(FindClosestEnemy()[0].GetTiles(), _shipController.GetTiles()), _shipController.GetTiles());
                EndTurn();
                break;
            case 3:
                MoveInDirection(GetOpossiteDirection(FindClosestEnemy()[0].GetTiles(), _shipController.GetTiles()));
                EndTurn();
                break;
        }
        _shipController.SetHasMoved(true);
    }
}