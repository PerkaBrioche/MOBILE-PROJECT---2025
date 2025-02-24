using System;
using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private bool hasAnEnemy = false;
    private bool hasAlly = false;
    
    private ShipController _shipController;

    private float _timeBeetweenReveal = 0.2f;
    
    [Foldout("OTHERS")]
    [SerializeField] private GameObject _shipSpawner;

    private GameObject _shipSpawbner;
    [Button]
    
    private void PlaceSpawner()
    {
        if(transform.childCount > 0) { return; }
        _shipSpawbner = Instantiate(_shipSpawner, transform.position, Quaternion.identity, transform);
    }
    [Button]
    private void DestroySpawner()
    {
        DestroyImmediate(transform.GetChild(0).gameObject);
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
        SetHighlight(true);
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

   public void GetTiles(int distance, Func<TilesController, TilesController> directionFunc, 
                     int walkDistance, 
                     List<Func<TilesController, TilesController>> sideFuncs = null, 
                     bool diagonal = false)
{
    if (_shipController.IsLocked()) { return; }
    
    bool isEnemy = _shipController.IsAnEnemy();
    int attackRange = distance - walkDistance;
    int baseWalkDistance = walkDistance;
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

        seconds += 0.07f;

        if (TurnManager.Instance.IsEnemyTurn() && !tilesForEnemy.Contains(tile))
        {
            tilesForEnemy.Add(tile);
        }

        if (walkDistance > 0) // GREEN TILES
        {
            if (isEnemy)
            {
                tile.HighLightTiles(seconds, false);
                walkDistance--;
                continue;
            }

            if (tile.HasAnAlly() && !isEnemy)
            {
                break;
            }

            if (tile.HasAnEnemy() && !isEnemy)
            {
                // RED TILES
                tile.HighLightTiles(seconds, true);
                break;
            }

            if (_shipController.HasMoved())
            {
                walkDistance--;
                continue;
            }

            tile.HighLightTiles(seconds, false);

            if (walkDistance != 1)
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

            walkDistance--;
        }
        else
        {
            // RED TILES
            if (tile.HasAnAlly() && !isEnemy)
            {
                break;
            }

            if (diagonal) // DIAGONAL
            {
                if (tile == tilesControllers[distance - 1]) // DERNIERE CASE
                {
                    if (attackRange == 1 && (baseWalkDistance + 1) < 2)
                    {
                    }
                    else if (attackRange == 2 && (baseWalkDistance + 1) == 2)
                    {
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
                        break;
                    }
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
                if (tile != tilesControllers[distance - 1])
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
                else
                {
                    if ((baseWalkDistance) == 3)
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
            }

            tile.HighLightTiles(seconds, true);
        }
        // --- Fin logique des tuiles ---
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

    private IEnumerator RevealTiles(Color color, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ChangeTilesColor(color);
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

    public void Debug()
    {
        StopAllCoroutines();
        StartCoroutine(DebugColor());
    }
    private IEnumerator DebugColor()
    {
        ChangeTilesColor(Color.magenta);
        yield return  new WaitForSeconds(2);
        ChangeTilesColor(Color.white);
    }

}
