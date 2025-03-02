using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ShipController : MonoBehaviour, bounce.IBounce
{
    private float _speedSlider = 1.3f;
    
    private bool _isMotherShip = false;
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
    private bool _isLocked = false;
    private bool _hasMoved;
    private bool _hasAttacked;

    private int _currentLockAttack;
    private bool _isInLockDown;

    private bool _isMooving;
    private bool _isOriginCampEnemy;

    [Foldout("OTHERS")] 
    [SerializeField] private GameObject _shipLock;
    [Foldout("OTHERS")] 
    [SerializeField] private SpriteRenderer _shipIcon;
    [Foldout("OTHERS")] 
    [SerializeField] private SpriteRenderer _RawNumber;
    [Foldout("OTHERS")] 
    [SerializeField] private List<Sprite> _numbers;
    [Foldout("OTHERS")] 
    [SerializeField] private Slider _sliderLife;
    [Foldout("OTHERS")] 
    [SerializeField] private Slider _sliderLifePrewiew;
    [Foldout("OTHERS")] 
    [SerializeField] private textController _textController;

    
    private Sprite _shipSprite;
    private Animator _shipAnimator;
    private ShipSpawner.shipType _shipType;
    
    private bool _hasBonusDamage;
    

    
    public enum shipAnimations
    {
        takeDamage,
        locked,
        Unlocked,
        InDanger,
        NoDanger
    }
    
    public void PlayAnim(shipAnimations anim)
    {
        switch (anim)
        {
            case shipAnimations.takeDamage:
                _shipAnimator.SetTrigger("TakeDamage");
                break;
            case shipAnimations.locked:
                _shipAnimator.SetBool("Locked", true);
                break;
            case shipAnimations.Unlocked:
                _shipAnimator.SetBool("Locked", false);
                break;
            case shipAnimations.InDanger:
                _shipAnimator.SetBool("InDanger", true);
                break;
            case shipAnimations.NoDanger:
                _shipAnimator.SetBool("InDanger", false);
                break;
        }
    }
    
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
        if(GetType() == ShipSpawner.shipType.MothherShip)
        {
            _isMotherShip = true;
        }
    }

    public void Bounce()
    {
        _bounce.StartBounce();
    }
    
    public bool IsMotherShip()
    {
        return _isMotherShip;
    }

    private void Awake()
    {
        _bounce = GetComponent<bounce>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _shipAnimator = GetComponent<Animator>();
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
        int distance = runtimeStats.WalkDistance + runtimeStats.AttackRange;
        if (distance > 0)
        {
            _myTilesController.GetTiles(distance, _tilesController => _tilesController.upTile, runtimeStats.WalkDistance, new List<Func<TilesController, TilesController>> { t => t.leftTile, t => t.rightTile });
            _myTilesController.GetTiles(distance, _tilesController => _tilesController.downTile, runtimeStats.WalkDistance, new List<Func<TilesController, TilesController>> { t => t.leftTile, t => t.rightTile });
            _myTilesController.GetTiles(distance, _tilesController => _tilesController.leftTile, runtimeStats.WalkDistance, new List<Func<TilesController, TilesController>> { t => t.upTile, t => t.downTile });
            _myTilesController.GetTiles(distance, _tilesController => _tilesController.rightTile, runtimeStats.WalkDistance, new List<Func<TilesController, TilesController>> { t => t.upTile, t => t.downTile });
        }
        int diagonal = distance - 1;
        if (diagonal > 0)
        {
            _myTilesController.GetTiles(diagonal, t => t.upTile != null ? t.upTile.rightTile : null, runtimeStats.WalkDistance - 1, diagonal > 1 ? new List<Func<TilesController, TilesController>> { t => t.leftTile, t => t.downTile } : null, true);
            _myTilesController.GetTiles(diagonal, t => t.upTile != null ? t.upTile.leftTile : null, runtimeStats.WalkDistance - 1, diagonal > 1 ? new List<Func<TilesController, TilesController>> { t => t.rightTile, t => t.downTile } : null, true);
            _myTilesController.GetTiles(diagonal, t => t.downTile != null ? t.downTile.rightTile : null, runtimeStats.WalkDistance - 1, diagonal > 1 ? new List<Func<TilesController, TilesController>> { t => t.leftTile, t => t.upTile } : null, true);
            _myTilesController.GetTiles(diagonal, t => t.downTile != null ? t.downTile.leftTile : null, runtimeStats.WalkDistance - 1, diagonal > 1 ? new List<Func<TilesController, TilesController>> { t => t.rightTile, t => t.upTile } : null, true);
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
        
        _sliderLifePrewiew.minValue = 0;
        _sliderLifePrewiew.maxValue = _myStats.HP;
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
    
    public void SetOnlyTile(TilesController newTiles)
    {
        _myTilesController = newTiles;
    }

    public TilesController GetTiles()
    {
        return _myTilesController;
    }

    public void SetLifePrewiew()
    {
        if (TouchManager.Instance.GetActualShipController() == null)
        {
            print("NO SHIP SELECTED");
            return;
        }
        print(  "runtimeStats.HP = "+ runtimeStats.HP + " l'autre ATK = " + TouchManager.Instance.GetActualShipController().runtimeStats.ATK);
        _sliderLifePrewiew.value = (runtimeStats.HP - TouchManager.Instance.GetActualShipController().runtimeStats.ATK);
    }
    public void SetNewPosition(TilesController neswtiles)
    {
        SetHasMoved(true);
        if (GetType() != ShipSpawner.shipType.Rider)
        {
            CheckLock();
        }
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
                print("ON LANCE LE MY TUNT");
                enemy.SetMyTurn();
            }
        }
        SetMoving(false);
        CheckTileType();
    }

    private void CheckTileType()
    {
        var tileType = _myTilesController.GetTileType();
        switch (tileType)
        {
            case TilesController.tileType.HealTile:
                _myTilesController.SetTileType(TilesController.tileType.defaultTile);
                ApplyHealth();
                break;
            case TilesController.tileType.DamageTile:
                _myTilesController.SetTileType(TilesController.tileType.defaultTile);
                SetBonusDamage(true);
                break;
        }
    }

    private void ApplyHealth()
    {
        runtimeStats.HP += 15;
        if(runtimeStats.HP > _myStats.HP)
        {
            runtimeStats.HP = _myStats.HP;
        }
        UpdateSlider();
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
        runtimeStats.HP -= damage;
        UpdateSlider();
        PlayAnim(shipAnimations.takeDamage);
        _textController.ShowDamage(damage);
        
        ShakeManager.instance.ShakeCamera(0.3f,0.15f);  
        
    }

    public void Die()
    {
        _myTilesController.ChangeCollider(true);
        _myTilesController.SetHasAnAlly(false);
        _myTilesController.SetHasAnEnemy(false);
        _myTilesController.ResetTiles();
        Destroy(gameObject);
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
        if(_isMotherShip){return;}
        if (play) { PlayAnim(shipAnimations.locked); }
        else { PlayAnim(shipAnimations.Unlocked); }
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
        if(_shipType == ShipSpawner.shipType.Rider)
        {
            runtimeStats.WalkDistance--;
            SetHasMoved(false);
        }
        CheckLock();
        if (!_isInLockDown && _myStats.CooldownAttack > 0)
        {
            _isInLockDown = true;
        }
    }

    public void SetHasMoved(bool move)
    {
        _hasMoved = move;
        CheckLock();
    }

    public void CheckLock()
    {
        if(_isLocked)
        {
            return;
        }
        if (_shipType != ShipSpawner.shipType.SpaceFortress)
        {
            if (_shipType == ShipSpawner.shipType.Tank && HasMoved() && IsInLockDown())
            {
                SetLockMode(true);
                return;
            }
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
                SetLockMode(true);
                return;
            }
            SetLockMode(false);
        }
    }

    public void ResetShip()
    {
        runtimeStats.WalkDistance = GetUnitStats().WalkDistance;
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
        print("HasEnemy = " + _myTilesController.HasAnEnemy());
        print("HasAlly = " + _myTilesController.HasAnAlly());   
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
        StartCoroutine(UpdateSliderLife());
    }

    private IEnumerator UpdateSliderLife()
    {
        float alpha = 0;
        float originalValue = _sliderLife.value;
        while (alpha <= 1)
        {
            alpha += Time.deltaTime * _speedSlider;
            _sliderLife.value = Mathf.Lerp(originalValue, runtimeStats.HP, alpha);
            yield return null;
        }
        if (runtimeStats.HP <= 0) { Die(); }
        yield return null;
    }

    public Sprite GetSprite()
    {
        return _shipSprite;
    }
    
    public UnitStats GetUnitStats()
    {
        return _myStats;
    }
    public void SetBonusDamage(bool state)
    {
        _hasBonusDamage = state;
    }
    public bool HasBonusDamage()
    {
        return _hasBonusDamage;
    }
    
}
