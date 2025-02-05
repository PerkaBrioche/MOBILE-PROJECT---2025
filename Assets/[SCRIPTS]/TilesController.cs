using System;
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


    [Space(20)] [Foldout("References")] [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [Foldout("References")] [SerializeField]
    private bounce _bounce;

    private Color _myColor;
    private Color _originalColor;

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

    public void HighLightTiles()
    {
        ChangeTilesColor(Color.green);
        _bounce.StartBouncePARAM(0, 0, true);
        SetHighlight(true);
    }

    public void ResetTiles()
    {
        print("RESET TILES");
        ChangeTilesColor(_originalColor);
        _bounce.ResetTransform();
        SetHighlight(false);
    }

    public void GetTiles(int distance, Func<TilesController, TilesController> directionFunc)
    {
        TilesController[] tilesControllers = new TilesController[distance];
        tilesControllers[0] = directionFunc(this);

        for (int i = 1; i < distance; i++)
        {
            if (tilesControllers[i - 1] != null)
            {
                print(tilesControllers[i - 1].name);
                tilesControllers[i] = directionFunc(tilesControllers[i - 1]);
            }
            
        }

        foreach (var tile in tilesControllers)
        {
            if (tile == null)
            {
                Debug.LogError("LA TILE EST NULL WTF");
                continue;
            }

            if (tile._boxCollider2D.enabled == false)
            {
                break;
            }
            
            tile.HighLightTiles();
        }
        
    }
    
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
    
    

}
