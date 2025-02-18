using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ShipController : MonoBehaviour, bounce.IBounce
{
    
    [SerializeField] private UnitStats _myStats;


    [Space(20)] [SerializeField] private float _speed;
    
    [SerializeField] private TilesController _myTilesController;
    
    private bounce _bounce;

    private bool isEnemy;
    private BoxCollider2D _boxCollider2D;
    public void Bounce()
    {
        _bounce.StartBounce();
    }
    private void Awake()
    {
        _bounce = GetComponent<bounce>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
    }

    public void GetPath()
    {
        if(_myTilesController == null)
        {
            return;
        }
        
        GetTilesPath();
    }

    private void GetTilesPath()
    {
        int distance = _myStats.WalkDistance + _myStats.AttackRange;
        if(distance > 0)
        {
            _myTilesController.GetTiles(distance, _tilesController => _tilesController.upTile, _myStats.WalkDistance
                , new List<Func<TilesController, TilesController>> { t => t.leftTile, t => t.rightTile });
            _myTilesController.GetTiles(distance, _tilesController => _tilesController.downTile , _myStats.WalkDistance
                , new List<Func<TilesController, TilesController>> { t => t.leftTile, t => t.rightTile });
            _myTilesController.GetTiles(distance, _tilesController => _tilesController.leftTile , _myStats.WalkDistance
                , new List<Func<TilesController, TilesController>> { t => t.upTile, t => t.downTile });
            _myTilesController.GetTiles(distance, _tilesController => _tilesController.rightTile, _myStats.WalkDistance
                , new List<Func<TilesController, TilesController>> { t => t.upTile, t => t.downTile });
        }
        
        // DIAGONAL
        int diagonal = (distance) - 1;
        if (diagonal > 0)
        {
            _myTilesController.GetTiles(
                diagonal, 
                t => t.upTile != null ? t.upTile.rightTile : null,
                _myStats.WalkDistance - 1,
                diagonal > 1 
                    ? new List<Func<TilesController, TilesController>> { t => t.leftTile, t => t.downTile } 
                    : null, true
            );
            
            _myTilesController.GetTiles(
                diagonal, 
                t => t.upTile != null ? t.upTile.leftTile : null,
                _myStats.WalkDistance - 1,
                diagonal > 1 
                    ? new List<Func<TilesController, TilesController>> { t => t.rightTile, t => t.downTile } 
                    : null, true
            );
            
            _myTilesController.GetTiles(
                diagonal, 
                t => t.downTile != null ? t.downTile.rightTile : null,
                _myStats.WalkDistance - 1,
                diagonal > 1 
                    ? new List<Func<TilesController, TilesController>> { t => t.leftTile, t => t.upTile } 
                    : null, true
            );
            
            _myTilesController.GetTiles(
                diagonal, 
                t => t.downTile != null ? t.downTile.leftTile : null,
                _myStats.WalkDistance - 1,
                diagonal > 1 
                    ? new List<Func<TilesController, TilesController>> { t => t.rightTile, t => t.upTile } 
                    : null, true
            );

            // DIAGONAL 
            //     
            //     // UP right
            //     _myTilesController.GetTiles(diagonal, _tilesController => _tilesController.rightTile != null ? _tilesController.rightTile.upTile : null ,  (_myStats.WalkDistance-1), _myTilesController.upTile);
            //     _myTilesController.GetTiles(diagonal, _tilesController => _tilesController.rightTile != null ? _tilesController.rightTile.upTile : null ,  (_myStats.WalkDistance-1), _myTilesController.rightTile);
            //     
            //     
            //     // UP left
            //     
            //     _myTilesController.GetTiles(diagonal, _tilesController => _tilesController.leftTile != null ? _tilesController.leftTile.upTile : null ,  (_myStats.WalkDistance-1), _myTilesController.upTile);
            //     _myTilesController.GetTiles(diagonal, _tilesController => _tilesController.leftTile != null ? _tilesController.leftTile.upTile : null ,  (_myStats.WalkDistance-1), _myTilesController.leftTile);
            //
            //     // DOWN right
            //     
            //     _myTilesController.GetTiles(diagonal, _tilesController => _tilesController.rightTile != null ? _tilesController.rightTile.downTile : null ,  (_myStats.WalkDistance-1), _myTilesController.downTile);
            //     _myTilesController.GetTiles(diagonal, _tilesController => _tilesController.rightTile != null ? _tilesController.rightTile.downTile : null ,  (_myStats.WalkDistance-1), _myTilesController.rightTile);
            //     
            //     // DOWN left
            //     
            //     _myTilesController.GetTiles(diagonal, _tilesController => _tilesController.leftTile != null ? _tilesController.leftTile.downTile : null ,  (_myStats.WalkDistance-1), _myTilesController.downTile);
            //     _myTilesController.GetTiles(diagonal, _tilesController => _tilesController.leftTile != null ? _tilesController.leftTile.downTile : null ,  (_myStats.WalkDistance-1), _myTilesController.leftTile);
            // }
        }
    }
    

    public void Initialize( bool enemy = false)
    {
        isEnemy = enemy;
    }
    
    public void SetTiles(TilesController newTiles)
    {
        
        Debug.LogError("SET TILES");

        if (_myTilesController != null)
        {
            _myTilesController.ChangeCollider(true);
            _myTilesController.SetShipController(null);
            _myTilesController.SetHasAnEnemy(false);
            _myTilesController.SetHasAnAlly(false);
        }
        
        // NOUVELLE TILES
        _myTilesController = newTiles;
        
        _myTilesController.SetHasAnEnemy(isEnemy);
        _myTilesController.SetHasAnAlly(!isEnemy);
        _myTilesController.ChangeCollider(false);
        _myTilesController.SetShipController(this);

    }
    
    public TilesController GetTiles()
    {
        return _myTilesController;
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
    
    public void TakeDamage(int damage)
    {
        
    }
    
    public void Die()
    {
        Destroy(gameObject);
        _myTilesController.ChangeCollider(true);
        _myTilesController.SetHasAnEnemy(false);
    }
    
    public void ChangeCollider(bool state)
    {
        _boxCollider2D.enabled = state;
    }
    
    public bool IsAnEnemy()
    {
        return isEnemy;
    }
    

}
