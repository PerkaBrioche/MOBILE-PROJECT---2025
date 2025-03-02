using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class TilesController : MonoBehaviour, bounce.IBounce
{

    [SerializeField] private bool _blockInteraction;
    [Header("ADJACENT TILES")] 
    private TilesController _upTile;
    private TilesController _downTile;
    private TilesController _leftTile;
    private TilesController _rightTile;
    
    private BoxCollider2D _boxCollider2D;

    public TilesController upTile => _upTile;
    public TilesController downTile => _downTile;
    public TilesController leftTile => _leftTile;
    public TilesController rightTile => _rightTile;

    private int columnPosition = 0;
    private int rowPosition = 0;

    private bool _isHighLighted;
    private bool _isAttackTiles;
  [SerializeField]   private bool _isRangeTiles;


    [Space(20)] [Foldout("References")] 
    [SerializeField] private SpriteRenderer _spriteRenderer;
     [Foldout("References")] 
    [SerializeField] private SpriteRenderer _tileAttackRenderer;

    [Foldout("References")] [SerializeField]
    private bounce _bounce;

    private Color _myColor;
    private Color _originalColor;

    private bool hasAnEnemy = false;
    private bool hasAlly = false;
    
    private ShipController _shipController;

    private float _timeBeetweenReveal = 0.2f;
    
    [Foldout("OTHERS")]
    [SerializeField] private GameObject _shipSpawner;
    [Foldout("OTHERS")]
    [SerializeField] private List<Sprite> _tilesSprite;

    [Foldout("OTHERS")] [SerializeField] private Sprite _deplacementTileSprite;
    [Foldout("OTHERS")] [SerializeField] private Sprite _blurTileSprite;
    [Foldout("OTHERS")] [SerializeField] private Sprite _attackTileSprite;
    [Foldout("OTHERS")] [SerializeField] private Sprite _enemyDetectedTileSprite;
    private Sprite _defaultSpriteTile;
    
    public enum enumTileSprites
    {
        defaultTile,
        deplacementTile, 
        blurTile,
        enemyDetectedTile,
        attackTile,
    }
    
    [Button] private void PlaceSpawner()
    {
        Transform parentSpawner = GameObject.FindGameObjectWithTag("Spawner").transform;
        var spawner = Instantiate(_shipSpawner, transform.position, Quaternion.identity, parentSpawner);
        if(spawner.TryGetComponent<ShipSpawner>(out ShipSpawner shipSpawner))
        {
            shipSpawner.shipTile = this;
        }
        else
        {
            print("NO SPAWNER COMPONENT");
        }
    }

    public void ChangeTileSprite(enumTileSprites tileSprite)
    {
        switch (tileSprite)
        {
            case enumTileSprites.defaultTile:
                _tileAttackRenderer.sprite = null;
                break;
            case enumTileSprites.deplacementTile:
                _tileAttackRenderer.sprite = _deplacementTileSprite;
                break;
            case enumTileSprites.enemyDetectedTile:
                _tileAttackRenderer.sprite = _enemyDetectedTileSprite;
                break;
            case enumTileSprites.blurTile:  
                _tileAttackRenderer.sprite = _blurTileSprite;
                break;
            case enumTileSprites.attackTile:
                _tileAttackRenderer.sprite = _attackTileSprite;
                break;
        }
    }
    
    

    public enum TileColor
    {
        Red,
        Blue,
        Green,
        Yellow,
        Magenta
    }


    private void Awake()
    {
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _myColor = _spriteRenderer.color;
        _originalColor = _myColor;
        _bounce = GetComponent<bounce>();
        GetAdjacentTiles();
    }

    private void Start()
    {
        _defaultSpriteTile = _tilesSprite[Random.Range(0, _tilesSprite.Count)];
        _spriteRenderer.sprite = _defaultSpriteTile;
    }

    [Button("Get Adjacent Tiles")]
    private void GetAdjacentTiles()
    {
        _upTile = DetectAdjacent(Vector2.up);
        _downTile = DetectAdjacent(Vector2.down);
        _leftTile = DetectAdjacent(Vector2.left);
        _rightTile = DetectAdjacent(Vector2.right);
    }


    public void Bounce()
    {
        _bounce.StartBounce();
    }

    private TilesController DetectAdjacent(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 2f);
        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent(out TilesController tilesController))
            {
                return tilesController;
            }

            return null;
        }

        return null;
    }

    public void HighLightTiles(float seconds, bool attackTiles, bool lockdown = false)
    {
        SetHighlight(true);
        enumTileSprites _tileSprite = enumTileSprites.defaultTile;
        
        if(lockdown) // TILE QUI PEUT ETRE ATTAQUER
        {
            _tileSprite = enumTileSprites.attackTile;
            SetIsAttackTile(true);
            if (HasAnEnemy())
            {
                _tileSprite = enumTileSprites.enemyDetectedTile;
                _shipController.PlayAnim(ShipController.shipAnimations.InDanger);
            }
        }
        else if (attackTiles) // TILE QUI MONTRE L'ATTAQUE LIMITATION
        {
            SetIsRangeTile(false);
            _tileSprite = enumTileSprites.blurTile;
        }
        else // TILE DE DEPLACEMENT
        {
            SetIsRangeTile(true);
            _tileSprite = enumTileSprites.deplacementTile;
        }
        StartCoroutine(RevealTiles(_tileSprite, seconds));
    }

    public void ResetTiles(bool noBounce = false)
    {
        if(_shipController != null)
        {
            _shipController.PlayAnim(ShipController.shipAnimations.NoDanger);
        }
        ChangeTileSprite(enumTileSprites.defaultTile);
        _bounce.ResetTransform(noBounce);
        SetHighlight(false);
        SetIsAttackTile(false);
        SetIsRangeTile(false);
    }

   public void GetTiles(int distance, Func<TilesController, TilesController> directionFunc, int walkDistance, List<Func<TilesController, TilesController>> sideFuncs = null, bool diagonal = false)
   {
       if (_shipController.IsLocked()) { return; }
    
       bool isEnemy = _shipController.IsAnEnemy();
       int attackRange = distance - walkDistance;
       int realAttackRange = _shipController.runtimeStats.AttackRange;
       int attackrangelEFT = _shipController.runtimeStats.AttackRange;
       int baseWalkDistance = walkDistance;
       
       if(diagonal)
       {
           attackrangelEFT--;
       }

       bool lockdown = _shipController.HasMoved();
       float seconds = 0f;
       TilesController[] tilesControllers = new TilesController[distance];
       tilesControllers[0] = directionFunc(this);
       for (int i = 1; i < distance; i++)
       {
           if (tilesControllers[i - 1] != null)
           {
               tilesControllers[i] = directionFunc(tilesControllers[i - 1]);
           }
       }
       List<TilesController> tilesForEnemy = new List<TilesController>();
       foreach (var tile in tilesControllers)
       {
           if (tile == null)
           {
               continue;
           }

           if (lockdown)
           {
               if (diagonal && realAttackRange <= 1)
               {
                   break;
               }
               if (realAttackRange <= 0)
               {
                   break;
               }
               else
               {
                   realAttackRange--;
               }
           }


           seconds += 0.07f;

           if (TurnManager.Instance.IsEnemyTurn() && !tilesForEnemy.Contains(tile))
           {
               tilesForEnemy.Add(tile);
           }

           if (walkDistance > 0 || lockdown) // GREEN TILES
           {
               if (isEnemy)
               {
                   tile.HighLightTiles(seconds, false);
                   walkDistance--;
                   continue;
               }

               if (tile.HasAnAlly())
               {
                   if (diagonal)
                   {
                       if (!lockdown)
                       {
                           print("ALLY DIAGONAL");
                           var sideTiles = CheckTiles(sideFuncs, tile, seconds, false);
                           if (TurnManager.Instance.IsEnemyTurn() && sideTiles != null)
                           {
                               foreach (var st in sideTiles)
                               {
                                   if (st != null && !tilesForEnemy.Contains(st))
                                   {
                                       tilesForEnemy.Add(st);
                                   }
                               }
                           }
                       }
                   }
                   break;
               }
               if (tile.HasAnEnemy())
               {
                   if(lockdown || attackrangelEFT > 0)
                   {
                       if(_shipController.GetType() == ShipSpawner.shipType.Rider && _shipController.HasAttacked() || _shipController.IsInLockDown())
                       {
                           tile.HighLightTiles(seconds, true);
                           break;
                       }
                       tile.HighLightTiles(seconds, true, true);
                   }
                   else
                   {
                       print("ENEMY MAIS PAS DE RANGE LEFT");
                       tile.HighLightTiles(seconds, true);
                   }
                   break;
               }
               
               tile.HighLightTiles(seconds, false, lockdown);
               
               if (walkDistance > 1 && !lockdown)
               {
                   var sideTiles = CheckTiles(sideFuncs, tile, seconds, false);
                   if (TurnManager.Instance.IsEnemyTurn() && sideTiles != null)
                   {
                       foreach (var st in sideTiles)
                       {
                           if (st != null && !tilesForEnemy.Contains(st))
                           {
                               tilesForEnemy.Add(st);
                           }
                       }
                   }
               }
               else 
               {
                   if (baseWalkDistance == 3)
                   {
                       var sideTiles = CheckTiles(sideFuncs, tile, seconds, false);
                       if (TurnManager.Instance.IsEnemyTurn() && sideTiles != null)
                       {
                           foreach (var st in sideTiles)
                           {
                               if (st != null && !tilesForEnemy.Contains(st))
                               {
                                   tilesForEnemy.Add(st);
                               }
                           }
                       }
                   }
               }
               attackrangelEFT--;
               walkDistance--;
           }
           else
           {
               bool hideMyself = tile.HasAnAlly() && !isEnemy;

               if (diagonal) // DIAGONAL
               {
                   if (tile == tilesControllers[distance - 1]) // DERNIERE CASE
                   {

                       if (attackRange == 2 && (baseWalkDistance + 1) == 2) { }else
                       {
                           var sideTiles = CheckTiles(sideFuncs, tile, seconds, true);
                           if (TurnManager.Instance.IsEnemyTurn() && sideTiles != null)
                           {
                               foreach (var st in sideTiles)
                               {
                                   if (st != null && !tilesForEnemy.Contains(st))
                                   {
                                       tilesForEnemy.Add(st);
                                   }
                               }
                           }
                       }
                       break;

                   }
                   else
                   {
                       var sideTiles = CheckTiles(sideFuncs, tile, seconds, true);
                       if (TurnManager.Instance.IsEnemyTurn() && sideTiles != null)
                       {
                           foreach (var st in sideTiles)
                           {
                               if (st != null && !tilesForEnemy.Contains(st))
                               {
                                   tilesForEnemy.Add(st);
                               }
                           }
                       }
                   }

               }
               else
               {
                   if (tile != tilesControllers[distance - 1] || (baseWalkDistance) == 3)
                   {
                       var sideTiles = CheckTiles(sideFuncs, tile, seconds, true);
                       if (TurnManager.Instance.IsEnemyTurn() && sideTiles != null)
                       {
                           foreach (var st in sideTiles)
                           {
                               if (st != null && !tilesForEnemy.Contains(st))
                               {
                                   tilesForEnemy.Add(st);
                               }
                           }
                       }
                   }
               }

               tile.HighLightTiles(seconds, true);
           }
       }
       if (TurnManager.Instance.IsEnemyTurn() && tilesForEnemy.Count > 0)
       {
           EnemyManager.Instance.AddTiles(tilesForEnemy);
       }
   }

   private TilesController[] CheckTiles(List<Func<TilesController, TilesController>> sideFuncs, 
                                     TilesController tile, 
                                     float seconds, 
                                     bool attack)
{
    if (sideFuncs == null) return null;


    TilesController[] tilesControllers = new TilesController[sideFuncs.Count];

    for (int i = 0; i < sideFuncs.Count; i++)
    {
        TilesController adjacent = sideFuncs[i](tile);

        if (adjacent != null && !adjacent.HasAnAlly())
        {
            adjacent.HighLightTiles(seconds, attack);
        }

        tilesControllers[i] = adjacent;
    }

    return tilesControllers;
}

    private IEnumerator RevealTiles(enumTileSprites enumSprite, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ChangeTileSprite(enumSprite);
        _bounce.StartBouncePARAM(0, 0, true);
    }

    #region GETTERS AND SETTERS

    

    public bool isHighLighted()
    {
        return _isHighLighted;
    }

    public void SetHighlight(bool h)
    {
        _isHighLighted = h;
    }

    public void ChangeCollider(bool enabled)
    {
        _boxCollider2D.enabled = enabled;
    }
    
    public bool HasAnEnemy()
    {
        return hasAnEnemy;
    }
    public void SetHasAnEnemy(bool enemy)
    {
        hasAnEnemy = enemy;
    }
    
    public void SetShipController(ShipController shipController)
    {
        _shipController = shipController;
    }
    
    public ShipController GetShipController()
    {
        return _shipController;
    }
    
    public bool HasAnAlly()
    {
        return hasAlly;
    }
    public void SetHasAnAlly(bool ally)
    {
        hasAlly = ally;
    }
    

    
    public bool IsAnAttackTile()
    {
        return _isAttackTiles;
    }
    public void SetIsAttackTile(bool attack)
    {
        _isAttackTiles = attack;
    }
    
    public bool IsRangeTile()
    {
        return _isRangeTiles;
    }
    
    public void SetIsRangeTile(bool range)
    {
        _isRangeTiles = range;
    }
    
    #endregion

    public List<TilesController> GetNeighbors()
    {
        List<TilesController> neighbors = new List<TilesController>();
    
        if (upTile != null) neighbors.Add(upTile);
        if (downTile != null) neighbors.Add(downTile);
        if (leftTile != null) neighbors.Add(leftTile);
        if (rightTile != null) neighbors.Add(rightTile);
        return neighbors;
    }

    // public void Debug()
    // {
    //     StopAllCoroutines();
    //     StartCoroutine(DebugColor());
    // }
    // private IEnumerator DebugColor()
    // {
    //     ChangeTilesColor(Color.magenta);
    //     yield return  new WaitForSeconds(2);
    //     ChangeTilesColor(Color.white);
    // }
    
    public void SetColumAndRowPosition(int column, int row)
    {
        columnPosition = column;
        rowPosition = row;
    }
    
    public int GetColumnPosition()
    {
        return columnPosition;
    }
    
    public int GetRowPosition()
    {
        return rowPosition;
    }

    public bool IsBlocked()
    {
        return _blockInteraction;
    }
}
