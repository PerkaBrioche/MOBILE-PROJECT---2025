using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ShipController : MonoBehaviour, bounce.IBounce
{
    [System.Serializable]
    public struct RuntimeStats {
        public string UnitName;
        public int HP;
        public int ATK;
        public int DEF;
        public int WalkDistance;
        public int AttackRange;
    }
    public RuntimeStats runtimeStats;

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
    private bool _isOriginCampEnemy;

    [Header("OTHERS")] 
    [SerializeField] private GameObject _shipLock;
    [SerializeField] private SpriteRenderer _shipIcon;
    
    [SerializeField] private SpriteRenderer _RawNumber;
    [SerializeField] private List<Sprite> _numbers;
    [SerializeField] private Slider _sliderLife;
    private Sprite _shipSprite;


    private ShipSpawner.shipType _shipType;
    
    public void SetType( ShipSpawner.shipType type)
    {
        _shipType = type;
    }
    
    public ShipSpawner.shipType GetType()
    {
        return _shipType;
    }
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
            _myTilesController.GetTiles(distance, _tilesController => _tilesController.upTile, _myStats.WalkDistance, new List<Func<TilesController, TilesController>> { t => t.leftTile, t => t.rightTile });
            _myTilesController.GetTiles(distance, _tilesController => _tilesController.downTile, _myStats.WalkDistance, new List<Func<TilesController, TilesController>> { t => t.leftTile, t => t.rightTile });
            _myTilesController.GetTiles(distance, _tilesController => _tilesController.leftTile, _myStats.WalkDistance, new List<Func<TilesController, TilesController>> { t => t.upTile, t => t.downTile });
            _myTilesController.GetTiles(distance, _tilesController => _tilesController.rightTile, _myStats.WalkDistance, new List<Func<TilesController, TilesController>> { t => t.upTile, t => t.downTile });
        }
        int diagonal = distance - 1;
        if (diagonal > 0)
        {
            _myTilesController.GetTiles(diagonal, t => t.upTile != null ? t.upTile.rightTile : null, _myStats.WalkDistance - 1, diagonal > 1 ? new List<Func<TilesController, TilesController>> { t => t.leftTile, t => t.downTile } : null, true);
            _myTilesController.GetTiles(diagonal, t => t.upTile != null ? t.upTile.leftTile : null, _myStats.WalkDistance - 1, diagonal > 1 ? new List<Func<TilesController, TilesController>> { t => t.rightTile, t => t.downTile } : null, true);
            _myTilesController.GetTiles(diagonal, t => t.downTile != null ? t.downTile.rightTile : null, _myStats.WalkDistance - 1, diagonal > 1 ? new List<Func<TilesController, TilesController>> { t => t.leftTile, t => t.upTile } : null, true);
            _myTilesController.GetTiles(diagonal, t => t.downTile != null ? t.downTile.leftTile : null, _myStats.WalkDistance - 1, diagonal > 1 ? new List<Func<TilesController, TilesController>> { t => t.rightTile, t => t.upTile } : null, true);
        }
    }

    public void Initialize(bool IsEnemy, UnitStats stats, ShipSpawner.shipType st)
    {
        _shipType = st;
        isEnemy = IsEnemy;
        _myStats = stats;

        if (isEnemy)
        {
            _shipIcon.sprite = _myStats._unitenemyIcon;
            _shipSprite = _myStats._unitenemyIcon;
            _shipIcon.flipY = true;
        }
        else
        {
            _shipIcon.sprite = _myStats._unitallyIcon;
            _shipSprite = _myStats._unitallyIcon;
        }
        
        
        
        runtimeStats.UnitName = _myStats.UnitName;
        runtimeStats.HP = _myStats.HP;  
        runtimeStats.ATK = _myStats.ATK;
        runtimeStats.DEF = Mathf.RoundToInt(_myStats.DEF);
        runtimeStats.WalkDistance = _myStats.WalkDistance;
        runtimeStats.AttackRange = _myStats.AttackRange;
        SetOriginCamp(IsEnemy);
        
        if(_sliderLife != null)
        {
            _sliderLife.minValue = 0;
            _sliderLife.maxValue = _myStats.HP;
            UpdateSlider();
        }
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
        SetHasMoved(true);
        CheckLock();
        StartCoroutine(TranslationPosition(neswtiles.transform.position));
        SetTiles(neswtiles);
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
            if(transform.TryGetComponent(out Enemy enemy))
            {
                // if (_shipType != ShipSpawner.shipType.SpaceFortress)
                // {
                    print("J'APPELLE LE TURN DE L'ENEMY");
                    enemy.SetMyTurn();
                //}
            }
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
        print("TAKE DAMAGE");
        runtimeStats.HP -= damage;
        UpdateSlider();
    }

    public void Die()
    {
        Destroy(gameObject);
        _myTilesController.ChangeCollider(true);
        _myTilesController.SetHasAnAlly(false);
        _myTilesController.SetHasAnEnemy(false);
        _myTilesController.ResetTiles();
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
        _shipIcon.color = play ? Color.gray : Color.white;
        
        //_shipLock.SetActive(play);
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
        if (!_isInLockDown && _myStats.CooldownAttack > 0)
        {
            print("LOCK DOWN");
            _isInLockDown = true;
        }
    }

    public void SetHasMoved(bool move)
    {
        _hasMoved = move;
    }

    public void CheckLock()
    {
        if(_isLocked)
        {
            return;
        }
        if (_shipType != ShipSpawner.shipType.SpaceFortress)
        {
            if (_hasAttacked)
            {
                if (_shipType != ShipSpawner.shipType.Rider)
                {
                    SetLockMode(true);
                    return;
                }
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
                print("JE ME LOCK GAGAGA");
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
            print("LOCK DOWN RESET");
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

    public int lockDownLeft()
    {
        return _currentLockAttack;
    }
    private void UpdateNumImage(int index)
    {
        _RawNumber.sprite = _numbers[index];
        if (index == 0)
        {
            _RawNumber.sprite = null;
        }
    }
    
    public int GetLife()
    {
        return runtimeStats.HP;
    }
        
    public int GetAttack()
    {
        return runtimeStats.ATK;
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
        isEnemy = !isEnemy;
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
    
    private void SetOriginCamp(bool state)
    {
        _isOriginCampEnemy = state;
    }
    
    public bool IsOriginCampEnemy()
    {
        return _isOriginCampEnemy;
    }
    
    private void UpdateSlider()
    {
        _sliderLife.value = runtimeStats.HP;
    }

    public Sprite GetSprite()
    {
        return _shipSprite;
    }
    
    public UnitStats GetUnitStats()
    {
        return _myStats;
    }
    
    
}
