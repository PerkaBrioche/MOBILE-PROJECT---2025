using System;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;

public class ShipSpawner : MonoBehaviour
{
    [Header("PUT THE SHIP STATS HERE")]
    [SerializeField] private UnitStats _shipStats;
    [Header("PUT THE TILES HERE")]
    TilesController _tilesController;
    [Header("IS AN ENEMY ?")]
    [SerializeField] private bool isEnemy =false;
    
    private ShipController _shipController;
    [Foldout("References")]
    [SerializeField] private GameObject _shipPrefab;
    
    
    private void Start()
    {
        _tilesController = transform.parent.GetComponent<TilesController>();
        SpawnShip();
    }
    
    public void SpawnShip()
    {
        var ship = Instantiate(_shipPrefab, _tilesController.transform.position, Quaternion.identity);
        _shipController = ship.GetComponent<ShipController>();
        _shipController.Initialize(isEnemy, _shipStats);
        _shipController.SetTiles(_tilesController);
    }
}
