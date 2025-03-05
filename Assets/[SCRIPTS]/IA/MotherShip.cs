using System;
using System.Collections;
using UnityEngine;

public class MotherShip : Enemy
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
        _shipController.SetLockMode(true);
        base.SetMyTurn();
    }
}
