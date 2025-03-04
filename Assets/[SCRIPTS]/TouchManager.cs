using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TouchManager : MonoBehaviour
{
    private PlayerInput _playerInput;
    private InputAction _touchPosition;
    private InputAction _touchPress;
    private InputAction _holdPress;
    private Vector3 _actualTouchedPosition;
    private float _holdTime = 0.4f;
    private float _actualholdTime = 0f;
    private Collider2D actualCollider;
    private bool _IsHolding = false;

    [SerializeField] private GameObject draggablePrefab;
    private bool _isDragging = false;
    private bool _isScrolling = false;
    private IDraggable currentDraggable;
    private GameObject currentDraggedObject;
    
    private GameManager _gameManager;

    [Header("Scrolling Settings")]
    [SerializeField] private bool canScroll = true;
    [SerializeField] private float scrollMinY = -5f;
    [SerializeField] private float scrollMaxY = 5f;
    private Vector3 _scrollStartTouchPos;
    private float _scrollStartCameraY;

    private ShipController _ActualshipController;
    private TilesController _ActualtilesController;
    [SerializeField] private GridController _gridController;

    private bool _isHighLighted;
    private CombatManager _combatManager;
    private ShipController _previewTarget = null;
    
    public static TouchManager Instance;
    

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _touchPosition = _playerInput.actions["TouchPosition"];
        _touchPress = _playerInput.actions["SinglePress"];
        _combatManager = FindFirstObjectByType<CombatManager>();
        _gridController = FindFirstObjectByType<GridController>();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;
    }
    private void OnEnable()
    {
        _touchPress.performed += OnTouched;
    }

    private void OnDisable()
    {
        _touchPress.performed -= OnTouched;
    }

    public ShipController GetActualShipController()
    {
        return _ActualshipController;
    }

    private void OnTouched(InputAction.CallbackContext context)
    {
        if (!TurnManager.Instance.IsPlayerTurn() || !_gameManager.CanTouch())
        {
            
            return;
        }
        _gameManager.TouchScreen(_touchPress);

        Vector2 touchedPosition = _touchPosition.ReadValue<Vector2>();
        _actualTouchedPosition = Camera.main.ScreenToWorldPoint(touchedPosition);
        actualCollider = GetCollider();
        if (actualCollider == null)
        {
            Debug.LogError("NO COLLIDER");
            return;
        }
        if (actualCollider.TryGetComponent(out bounce.IBounce Ib))
        {
            Ib.Bounce();
        }
        if (actualCollider.TryGetComponent(out TilesController tC))
        {
            if (tC.IsBlocked())
            {
                return;
            }
            if (_isHighLighted)
            {
                if (tC.isHighLighted())
                {
                    if(_ActualshipController != null)
                    {
                        if(tC.IsAnAttackTile())
                        {
                            Reset();
                        }
                        else if(tC.IsRangeTile())
                        {
                            if (_ActualshipController.CanMove())
                            {
                                _ActualshipController.SetNewPosition(tC);
                            }
                        }
                    }
                }
            }
            Reset();
        }
        if (actualCollider.TryGetComponent(out ShipController sc))
        {
            if (_ActualshipController == null) // NO SHIP SELECTED
            {
                if(sc.IsAnEnemy()) // IF THE SHIP IS AN ENEMY
                {
                    _combatManager.DisplayAttackerStats(sc);
                }
                else
                {
                    _combatManager.DisplayAllyStats(sc);
                    if(sc.GetType() == ShipSpawner.shipType.MothherShip){return;}
                    _isHighLighted = true;
                }
                _ActualshipController = sc;
                sc.GetPath();
            }
            else
            {
                if(sc.IsAnEnemy())
                {
                    if (_ActualshipController.IsAnEnemy())
                    {
                        Reset();
                        return;
                    }
                    if (sc.GetTiles().HasAnEnemy() && sc.GetTiles().IsAnAttackTile() && _ActualshipController.CanAttack() && !_ActualshipController.IsInLockDown())
                    {
                        if(_previewTarget == null || _previewTarget != sc)
                        {
                            _previewTarget = sc;
                            _combatManager.PreviewCombat(_ActualshipController, sc);
                        }
                        else
                        {
                            _ActualshipController.SetHasAttacked(true);
                            _combatManager.StartCombat(_ActualshipController, sc);
                            _previewTarget = null;
                            Reset();
                        }
                    }
                    else
                    {
                        Reset();
                    }
                }
                else
                {
                    if (sc.GetType() == ShipSpawner.shipType.MothherShip)
                    {
                        _combatManager.DisplayAllyStats(sc);
                    }
                    else
                    {
                        if(_ActualshipController == sc)  // SI LE VAISSEAU SELECTIONNER EST LE MEME QUE LE PRECEDENT
                        {
                            Reset();
                        }
                        else
                        {
                            Reset(true);
                            _ActualshipController = sc;
                            _ActualshipController.GetPath();
                            _combatManager.DisplayAllyStats(_ActualshipController);
                        }
                    }
                }
            }
        }
    }

    public void Reset(bool noBounce = false)
    {
        _gridController.ResetAllTiles(noBounce);
        _ActualshipController = null;
        _ActualtilesController = null;
        _isHighLighted = false;
        _previewTarget = null;
        _combatManager.ClearPreview();
    }
    
    private Collider2D GetCollider()
    {
        Collider2D hit = Physics2D.OverlapPoint(_actualTouchedPosition);
        if (hit != null)
        {
            return hit;
        }
        return null;
    }
    
     public void SetInteractionEnabled(bool enabled)
     {
         _playerInput.enabled = enabled;
     }
}
