using System;
using NaughtyAttributes;
using UnityEngine;

public class TilesController : MonoBehaviour, bounce.IBounce
{
    [Header("ADJACENT TILES")]
    [SerializeField] private TilesController _upTile;
    [SerializeField] private TilesController _downTile;
    [SerializeField] private TilesController _leftTile;
    [SerializeField] private TilesController _rightTile;
    
    
    [Space(20)]
    
    [Foldout("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [Foldout("References")]
    [SerializeField] private bounce _bounce;
    
        private Color _myColor;

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
            if(hit.collider.TryGetComponent(out TilesController tilesController))
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
        _bounce.StartBouncePARAM(0,0, true);
    }
    
    public void ResetTiles()
    {
        ChangeTilesColor(_myColor);
        _bounce.ResetTransform();
    }

    public void GetUpTiles(int distance)
    {
        TilesController[] tilesControllers = new TilesController[distance];
        tilesControllers[0] = _upTile;
        
        for (int i = 1; i < distance; i++)
        {
            tilesControllers[i] = tilesControllers[i-1]._upTile;
        }
        
        foreach (var tile in tilesControllers)
        {
            tile.HighLightTiles();
        }
    }
    
}
