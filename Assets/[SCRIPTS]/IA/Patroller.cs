using System;
using System.Collections;
using UnityEngine;

public class Patroller : Enemy
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
}
