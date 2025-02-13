using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ShipController : MonoBehaviour, bounce.IBounce
{

    private UnitStats _myStats;
    [Space(20)] private float _speed = 1.2f;

    private TilesController _myTilesController;
    private bounce _bounce;
    private bool isEnemy;
    private BoxCollider2D _boxCollider2D;

    // TURN
    [SerializeField] private bool _isLocked = false;
    private bool _hasMoved;
    private bool _hasAttacked;

    private int _currentLockAttack;
    private bool _isInLockDown;

    private bool _isMooving;

    [Header("OTHERS")] [SerializeField] private GameObject _shipLock;
    [SerializeField] private SpriteRenderer _RawNumber;

    [SerializeField] private List<Sprite> _numbers;



    private void Start()
    {
        _currentLockAttack = _myStats.CooldownAttack;
    }

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
        if (_myTilesController == null)
        {
            return;
        }

        GetTilesPath();
    }

    private void GetTilesPath()
    {
        int distance = _myStats.WalkDistance + _myStats.AttackRange;
        if (distance > 0)
        {
            _myTilesController.GetTiles(distance, _tilesController => _tilesController.upTile, _myStats.WalkDistance
                , new List<Func<TilesController, TilesController>> { t => t.leftTile, t => t.rightTile });
            _myTilesController.GetTiles(distance, _tilesController => _tilesController.downTile, _myStats.WalkDistance
                , new List<Func<TilesController, TilesController>> { t => t.leftTile, t => t.rightTile });
            _myTilesController.GetTiles(distance, _tilesController => _tilesController.leftTile, _myStats.WalkDistance
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
        }
    }


    public void Initialize(bool IsEnemy, UnitStats stats)
    {
        isEnemy = IsEnemy;
        _myStats = stats;
    }

    public void SetTiles(TilesController newTiles)
    {
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

        SetHasMoved(true);
        CheckLock();
    }

    public IEnumerator TranslationPosition(Vector2 newPosition)
    {
        SetMoving(true);
        float alpha = 0;
        Vector2 originalPosition = transform.position;
        while (alpha <= 1)
        {
            alpha += Time.deltaTime * _speed;
            transform.position = Vector2.Lerp(originalPosition, newPosition, alpha);
            yield return null;
        }
        yield return null;
        
        EndMovement();
    }

    private void EndMovement()
    {
        if (TurnManager.Instance.IsEnemyTurn())
        {
            TurnManager.Instance.EnemyEndATurn();
        }
        SetMoving(false);

    }

    private void OnValidate()
    {
        if (_speed < 1)
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

    public void SetLockMode(bool play)
    {
        _isLocked = play;
        _shipLock.SetActive(play);
    }

    public bool IsLocked()
    {
        return _isLocked;
    }

    public bool HasMoved()
    {
        return _hasMoved;
    }

    public bool HasAttacked()
    {
        return _hasAttacked;
    }

    public bool CanMove()
    {
        if (!HasMoved() && !IsLocked())
        {
            return true;
        }

        return false;
    }

    public bool CanAttack()
    {
        if (!HasAttacked() && !IsLocked())
        {
            return true;
        }

        return false;
    }

    public void SetHasAttacked(bool attack)
    {
        _hasAttacked = attack;
        CheckLock();
        if (!_isInLockDown && !_myStats.CanMooveAndShoot)
        {
            _isInLockDown = true;
        }
    }

    public void SetHasMoved(bool move)
    {
        _hasMoved = move;
    }

    public void CheckLock()
    {
        if (_myStats.CanMooveAndShoot)
        {
            if (_hasAttacked)
            {
                SetLockMode(true);
                return;
            }

            if (_hasMoved && _hasAttacked)
            {
                SetLockMode(true);
                return;
            }
            
            SetLockMode(false);
            
        }
        else
        {
            if (_hasMoved || _hasAttacked)
            {
                SetLockMode(true);
                return;
            }

            SetLockMode(false);
            
        }
    }

    public void ResetShip()
    {
        _hasMoved = false;
        _hasAttacked = false;
        SetLockMode(false);

        if (_isInLockDown)
        {
            _currentLockAttack--;
            UpdateNumImage(_currentLockAttack);
            if (_currentLockAttack <= 0)
            {
                _isInLockDown = false;
                _currentLockAttack = _myStats.CooldownAttack;
            }
        }
    }

    public bool IsInLockDown()
    {
        return _isInLockDown;
    }

    private void UpdateNumImage(int index)
    {
        _RawNumber.sprite = _numbers[index];
        if (index == 0)
        {
            _RawNumber.sprite = null;
        }
    }

    public UnitStats GetStats()
    {
        return _myStats;
    }

    public void GetInfos()
    {
        print("HasMoved = " + _hasMoved);
        print("HasAttacked = " + _hasAttacked);
        print("IsLocked = " + _isLocked);
        print("IsInLockDown = " + _isInLockDown);
    }
    
    public void ChangeCamp()
    {
        isEnemy =! isEnemy;
        SetTiles(_myTilesController);
    }

    public bool IsMoving()
    {
        return _isMooving;
    }
    
    public void SetMoving(bool state)
    {
        _isMooving = state;
    }
}
