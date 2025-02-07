using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;

public class TilesController : MonoBehaviour, bounce.IBounce
{
    [Header("ADJACENT TILES")] [SerializeField]
    private TilesController _upTile;

    [SerializeField] private TilesController _downTile;
    [SerializeField] private TilesController _leftTile;
    [SerializeField] private TilesController _rightTile;
    
    private BoxCollider2D _boxCollider2D;

    public TilesController upTile => _upTile;
    public TilesController downTile => _downTile;
    public TilesController leftTile => _leftTile;
    public TilesController rightTile => _rightTile;


    private bool _isHighLighted;
    private bool _isAttackTiles;
    private bool _isRangeTiles;


    [Space(20)] [Foldout("References")] [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [Foldout("References")] [SerializeField]
    private bounce _bounce;

    private Color _myColor;
    private Color _originalColor;

    private bool hasAnEnemy = false;
    private bool hasAlly = false;
    
    private ShipController _shipController;

    private float _timeBeetweenReveal = 0.2f;

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

    [Button("Get Adjacent Tiles")]
    private void GetAdjacentTiles()
    {
        _upTile = DetectAdjacent(Vector2.up);
        _downTile = DetectAdjacent(Vector2.down);
        _leftTile = DetectAdjacent(Vector2.left);
        _rightTile = DetectAdjacent(Vector2.right);
    }

    public void ChangeTilesColor(Color color)
    {
        _spriteRenderer.color = color;
        _myColor = color;
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

    public void HighLightTiles(float seconds, bool attackTiles)
    {
        var color = Color.white;
        if(attackTiles)
        {
            SetIsAttackTile(true);
            color = Color.red;
        }
        else
        {
            SetIsRangeTile(false);
            color = Color.green;
        }
        
        StartCoroutine(RevealTiles(color, seconds));
    }

    public void ResetTiles()
    {
        ChangeTilesColor(_originalColor);
        _bounce.ResetTransform();
        SetHighlight(false);
        
        SetIsAttackTile(false);
        SetIsRangeTile(false);
    }

    public void GetTiles(int distance, Func<TilesController, TilesController> directionFunc, int walkDistance)
    {
        float seconds = 0.05f;
        TilesController[] tilesControllers = new TilesController[distance];
        tilesControllers[0] = directionFunc(this);
        
        for (int i = 1; i < distance; i++)
        {
            if (tilesControllers[i - 1] != null)
            {
                tilesControllers[i] = directionFunc(tilesControllers[i - 1]);
            }
            
        }
        
        
        foreach (var tile in tilesControllers)
        {
            if (tile == null)
            {
                continue;
            }
    
            seconds += 0.05f;

            if(walkDistance > 0)
            {
                // GREEN TILES
                if (tile.HasAnAlly())
                {
                    break;
                }

                if (tile.HasAnEnemy())
                {
                    tile.HighLightTiles(seconds, true);
                    break;
                }
                tile.HighLightTiles(seconds, false);
                walkDistance--;
            }
            else
            {
                // RED TILES
                if (tile.HasAnAlly())
                {
                    continue;
                }
                tile.HighLightTiles(seconds, true);
                break;
            }
        }
        
    }
    private IEnumerator RevealTiles(Color color, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ChangeTilesColor(color);
        _bounce.StartBouncePARAM(0, 0, true);
        SetHighlight(true);
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


}
