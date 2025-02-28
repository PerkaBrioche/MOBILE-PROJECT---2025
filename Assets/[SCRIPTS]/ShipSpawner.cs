using System;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;

public class ShipSpawner : MonoBehaviour
{
    [Header("PUT THE SHIP STATS HERE")]
    private UnitStats _shipStats;
    public shipType TypeShip;

    [Header("PUT THE TILES HERE")]
    [Header("IS AN ENEMY ?")]
    [SerializeField] private bool isEnemy = false;
    
    private ShipController _shipController;
    [Foldout("References")]
    [SerializeField] private GameObject _shipPrefab;

    public TilesController shipTile;
    public enum shipType
    {
        Patroller,
        Ranger,
        Rider,
        SpacceBerzerker,
        Tank,
        SpaceFortress
    }
    [Button] private void DestroySpawner()
    {
        DestroyImmediate(gameObject);
    }
    [Foldout("References")]
    [SerializeField] private UnitStats PatrollerStats;
    [Foldout("References")]
    [SerializeField] private UnitStats RangerStats;
    [Foldout("References")]
    [SerializeField] private UnitStats RiderStats;
    [Foldout("References")]
    [SerializeField] private UnitStats SpaceBerzerkerStats;
    [Foldout("References")]
    [SerializeField] private UnitStats TankStats;
    [Foldout("References")]
    [SerializeField] private UnitStats SpaceFortressStats;
    
    private void Start()
    {
        SpawnShip();
    }
    
    public void SpawnShip()
    {
        print(shipTile);
        var ship = Instantiate(_shipPrefab, shipTile.transform.position, Quaternion.identity);
        _shipController = ship.GetComponent<ShipController>();
        switch (TypeShip)
        {
            case shipType.Patroller:
                _shipStats = PatrollerStats;
                ship.AddComponent<Patroller>();
                break;
            case shipType.Ranger:
                _shipStats = RangerStats;
                ship.AddComponent<Ranger>();
                break;
            case shipType.Rider:
                _shipStats = RiderStats;
                ship.AddComponent<Rider>();
                break;
            case shipType.SpacceBerzerker:
                ship.AddComponent<Patroller>();
                _shipStats = SpaceBerzerkerStats;
                break;
            case shipType.Tank:
                _shipStats = TankStats;
                ship.AddComponent<Tank>();
                break;
            case shipType.SpaceFortress:
                _shipStats = SpaceFortressStats;
                ship.AddComponent<Fortress>();
                break;
        }
        
        if(_shipStats == null)
        {
            Debug.LogError("Ship Stats not found");
            return;
        }
        
        _shipController.Initialize(isEnemy, _shipStats, TypeShip);
        _shipController.SetTiles(shipTile);
    }
}
