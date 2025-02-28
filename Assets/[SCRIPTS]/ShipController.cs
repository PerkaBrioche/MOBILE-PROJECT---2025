using System;
using System.Collections;
using System.Collections.Generic;
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
    
    [SerializeField] private bool _isLocked = false;
    [SerializeField] private bool _hasMoved;
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
    private TilesController _startingTile;
    private bool _isDead = false;
    private RuntimeStats _initialStats;
    private Sprite _shipSprite;
    
    private Animator _shipAnimator;
    
    private ShipSpawner.shipType _shipType;
    
    public enum shipAnimations
    {
        takeDamage,
        locked,
        Unlocked,
    }
    
    public void PlayAnim(shipAnimations anim)
    {
        if (_shipAnimator == null)
            return;
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
        }
    }
    
    public void SetType(ShipSpawner.shipType type)
    {
        _shipType = type;
    }
    
    public new ShipSpawner.shipType GetType()
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
        if (_shipAnimator == null)
            Debug.LogWarning("Animator manquant sur " + gameObject.name);
    }
    
    public void GetPath()
    {
        if (_myTilesController == null)
            return;
        print("RECUPERER LE PATH");
        GetTilesPath();
    }
    
    private void GetTilesPath()
    {
        int distance = _myStats.WalkDistance + _myStats.AttackRange;
        if (distance > 0)
        {
            _myTilesController.GetTiles(distance, t => t.upTile, _myStats.WalkDistance, new List<Func<TilesController, TilesController>> { t => t.leftTile, t => t.rightTile });
            _myTilesController.GetTiles(distance, t => t.downTile, _myStats.WalkDistance, new List<Func<TilesController, TilesController>> { t => t.leftTile, t => t.rightTile });
            _myTilesController.GetTiles(distance, t => t.leftTile, _myStats.WalkDistance, new List<Func<TilesController, TilesController>> { t => t.upTile, t => t.downTile });
            _myTilesController.GetTiles(distance, t => t.rightTile, _myStats.WalkDistance, new List<Func<TilesController, TilesController>> { t => t.upTile, t => t.downTile });
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
        runtimeStats.WalkDistance = _myStats.WalkDistance;
        runtimeStats.AttackRange = _myStats.AttackRange;
        SetOriginCamp(IsEnemy);
        if (_sliderLife != null)
        {
            _sliderLife.minValue = 0;
            _sliderLife.maxValue = _myStats.HP;
            _sliderLife.value = _myStats.HP;
        }
        SaveStartingState();
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
    
    public void SetNewPosition(TilesController neswtiles)
    {
        print("SET NEW POSITON");
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
        print("FIN DE MOUVEMENT");
        if (TurnManager.Instance.IsEnemyTurn())
        {
            if (transform.TryGetComponent(out Enemy enemy))
            {
                print("J'APPELLE LE TURN DE L'ENEMY");
                enemy.SetMyTurn();
            }
        }
        SetMoving(false);
    }
    
    private void OnValidate()
    {
        if (_speed < 1)
            _speed = 1;
    }
    
    public void TakeDamage(int damage)
    {
        runtimeStats.HP -= damage;
        UpdateSlider();
        PlayAnim(shipAnimations.takeDamage);
        if (runtimeStats.HP <= 0) { Die(); }
    }
    
    public void Die()
    {
        if (TurnManager.Instance != null && TurnManager.Instance.IsPlayerTurn())
        {
            _isDead = true;
            gameObject.SetActive(false);
            _myTilesController.ChangeCollider(true);
            _myTilesController.SetHasAnEnemy(false);
        }
        else
        {
            Destroy(gameObject);
            _myTilesController.ChangeCollider(true);
            _myTilesController.SetHasAnEnemy(false);
        }
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
        if (play)
            PlayAnim(shipAnimations.locked);
        else
            PlayAnim(shipAnimations.Unlocked);
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
        return (!HasMoved() && !IsLocked());
    }
    
    public bool CanAttack()
    {
        return (!HasAttacked() && !IsLocked());
    }
    
    public void SetHasAttacked(bool attack)
    {
        _hasAttacked = attack;
        CheckLock();
        if (!_isInLockDown && _myStats.CooldownAttack > 0)
            _isInLockDown = true;
    }
    
    public void SetHasMoved(bool move)
    {
        _hasMoved = move;
    }
    
    public void CheckLock()
    {
        if (_isLocked)
            return;
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
            if (_hasMoved && _hasAttacked && _shipType != ShipSpawner.shipType.Rider)
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
            _RawNumber.sprite = null;
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
    
    public void SaveStartingState()
    {
        _startingTile = _myTilesController;
        _initialStats = runtimeStats;
    }
    
    public void ResetTurnState()
    {
        if (_isDead)
        {
            gameObject.SetActive(true);
            _isDead = false;
        }
        ResetShip();
        runtimeStats = _initialStats;
        UpdateSlider();
        if (_startingTile != null)
        {
            transform.position = _startingTile.transform.position;
            SetTiles(_startingTile);
        }
    }
    
    public void SetHealthBarVisible(bool visible)
    {
        if (_sliderLife != null)
            _sliderLife.gameObject.SetActive(visible);
    }
}
