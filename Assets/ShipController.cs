using System;
using System.Collections;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    
    [Header("DEPLACEMENT TILES")]
    [SerializeField] private int right;
    [SerializeField] private int left;
    [SerializeField] private int up;
    [SerializeField] private int down;

    [Space(20)] [SerializeField] private float _speed;
    
    [SerializeField] private TilesController _tilesController;
    
    

    public void GetPath()
    {
        if(_tilesController == null)
        {
            Debug.LogError("TilesController is null");
            return;
        }
        
        _tilesController.GetTiles(up, _tilesController => _tilesController.upTile);
        _tilesController.GetTiles(down, _tilesController => _tilesController.downTile);
        _tilesController.GetTiles(left, _tilesController => _tilesController.leftTile);
        _tilesController.GetTiles(right, _tilesController => _tilesController.rightTile);
    }
    
    public void SetTiles(TilesController tilesController)
    {
        _tilesController = tilesController;
    }
    
    public TilesController GetTiles()
    {
        return _tilesController;
    }
    
    public void SetNewPosition(TilesController neswtiles)
    {
        StartCoroutine(TranslationPosition(neswtiles.transform.position));
        SetTiles(neswtiles);
    }

    public IEnumerator TranslationPosition(Vector2 newPosition)
    {
        float alpha = 0;
        Vector2 originalPosition = transform.position;
        while (alpha <= 1)
        {
            alpha += Time.deltaTime * _speed;
            transform.position = Vector2.Lerp(originalPosition, newPosition, alpha);
            yield return null;
        }
        yield return null;
    }

    private void OnValidate()
    {
        if(_speed < 1)
        {
            _speed = 1;
        }
    }
}
