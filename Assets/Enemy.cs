using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private ShipController _shipController;
    private UnitStats _unitStats;
    
    private bool canAction = true;
    private GridController _gridController;
    
    
    private void Awake()
    {
        _gridController = FindObjectOfType<GridController>();
    }

    private void Start()
    {
        _shipController = GetComponent<ShipController>();
        _unitStats = _shipController.GetStats();
    }
    
    public virtual void SetMyTurn()
    {
        Debug.Log("Enemy turn");
        _shipController.GetPath();
        StartCoroutine(Wait(1f));
    }
    
    public virtual void Attack(ShipController sc)
    {
        sc.TakeDamage(_unitStats.ATK);
        Debug.Log("Enemy attack");
    }

    public virtual void Move(TilesController tilesController)
    {
        _shipController.SetNewPosition(tilesController);
        _gridController.ResetAllTiles();
    }

    public virtual void MoveFoward()
    {
        var fowardTile = _shipController.GetTiles().downTile;
        if (fowardTile != null)
        {
            Move(fowardTile);
        }
    }
    
    public virtual void EndTurn()
    {
        Debug.Log("Enemy end turn");
    }
    
    private IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);
        MoveFoward();
    }
}

