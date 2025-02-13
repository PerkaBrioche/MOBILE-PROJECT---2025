using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    private List<TilesController> _enemyTiles = new List<TilesController>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    
    public void AddTiles(TilesController[] tilesController)
    {
        foreach (var tile in tilesController)
        {
            _enemyTiles.Add(tile);
        }
    }
    
    public TilesController[] GetTiles()
    {
        print(_enemyTiles.Count);
        return _enemyTiles.ToArray();
    }
    
    public void RemoveTile(TilesController tilesController)
    {
        _enemyTiles.Remove(tilesController);
    }
    
    public void ClearTiles()
    {
        _enemyTiles.Clear();
    }
}
