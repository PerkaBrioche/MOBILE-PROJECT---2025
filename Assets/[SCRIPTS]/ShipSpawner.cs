using System;
using UnityEngine;

public class ShipSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _shipPrefab;
    [SerializeField] TilesController _tilesController;

    private ShipController _shipController;
    private void Start()
    {
        SpawnShip();
    }
    
    public void SpawnShip()
    {
        var ship = Instantiate(_shipPrefab, _tilesController.transform.position, Quaternion.identity);
        _shipController = ship.GetComponent<ShipController>();
        _shipController.SetTiles(_tilesController);
    }
}
