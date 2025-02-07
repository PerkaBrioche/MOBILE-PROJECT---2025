using System;
using Unity.VisualScripting;
using UnityEngine;

public class ShipSpawner : MonoBehaviour
{
    [Header("PUT THE SHIP PREFAB HERE")]
    [SerializeField] private GameObject _shipPrefab;
    [Header("PUT THE TILES HERE")]
    [SerializeField] TilesController _tilesController;
    [Header("IS AN ENEMY ?")]
    [SerializeField] private bool isEnemy =false;
    
    private ShipController _shipController;
    
    
    private void Start()
    {
        SpawnShip();
    }
    
    public void SpawnShip()
    {
        var ship = Instantiate(_shipPrefab, _tilesController.transform.position, Quaternion.identity);
        if (isEnemy)
        {
            ship.tag = "Enemy";
        }
        _shipController = ship.GetComponent<ShipController>();
        _shipController.Initialize(isEnemy);
        _shipController.SetTiles(_tilesController);
    }
}
