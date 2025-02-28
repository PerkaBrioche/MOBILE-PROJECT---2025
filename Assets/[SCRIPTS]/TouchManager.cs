using System;
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

    private ShipController _ActualshipController = null;
    private TilesController _ActualtilesController;
    [SerializeField] private GridController _gridController;

    private bool _isHighLighted;
    private CombatManager _combatManager;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _touchPosition = _playerInput.actions["TouchPosition"];
        _touchPress = _playerInput.actions["SinglePress"];
        _holdPress = _playerInput.actions["HoldPress"];
        _combatManager = FindObjectOfType<CombatManager>();
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;
    }
    private void OnEnable()
    {
        _touchPress.performed += OnTouched;
        _holdPress.started += OnHoldStarted;
        _holdPress.canceled += OnHoldCanceled;
    }

    private void OnDisable()
    {
        _touchPress.performed -= OnTouched;
        _holdPress.started -= OnHoldStarted;
        _holdPress.canceled -= OnHoldCanceled;
    }

    private void Update()
    {
        if (_IsHolding)
        {
            if (_isDragging && currentDraggable != null)
            {
                Vector2 touchedPos = _touchPosition.ReadValue<Vector2>();
                _actualTouchedPosition = Camera.main.ScreenToWorldPoint(touchedPos);
                _actualTouchedPosition.z = 0f;
                currentDraggable.OnDrag(_actualTouchedPosition);
            }
            else if (_isScrolling && canScroll)
            {
                Vector2 touchedPos = _touchPosition.ReadValue<Vector2>();
                Vector3 currentTouchWorldPos = Camera.main.ScreenToWorldPoint(touchedPos);
                currentTouchWorldPos.z = 0f;
                float deltaY = currentTouchWorldPos.y - _scrollStartTouchPos.y;
                float newCameraY = Mathf.Clamp(_scrollStartCameraY - deltaY * 0.75f, scrollMinY, scrollMaxY);
                Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, newCameraY, Camera.main.transform.position.z);
            }
        }
        else
        {
            _actualholdTime = 0;
        }
    }

    private void PressReleased(InputAction.CallbackContext context)
    {
        if (!_IsHolding) { return; }
        _IsHolding = false;
    }

    private void OnHolding(InputAction.CallbackContext context)
    {
    }

    private void GetTouchPositon(InputAction.CallbackContext context)
    {
    }

    private void OnTouched(InputAction.CallbackContext context)
    {
        print("ON TOUCHED");
        if (!TurnManager.Instance.IsPlayerTurn() || !_gameManager.CanTouch())
        {
            Debug.LogError("PROBLEM TOUCH");
            return;
        }
        _gameManager.TouchScreen();
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
        if (actualCollider.TryGetComponent(out TilesController tC)) // TILES
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
                            print("RANGE TILE");

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
            if(sc.GetType() == ShipSpawner.shipType.MothherShip){return;}
           sc.GetInfos();
            if (_ActualshipController == null)
            {
                if(sc.IsAnEnemy())
                {
                }
                else
                {
                    _isHighLighted = true;
                }
                sc.GetPath();
                _ActualshipController = sc;
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
                        _ActualshipController.SetHasAttacked(true);
                        _combatManager.StartCombat(_ActualshipController, sc);
                    }
                    Reset();
                }
                else
                {
                    if(_ActualshipController == sc)  // SI LE VAISSEAU SELECTIONNER EST LE MEME QUE LE PRECEDENT
                    {
                        _ActualshipController.SetLockMode(true);
                        Reset();
                    }
                    else
                    {
                        Reset();
                        sc.GetPath();
                        _ActualshipController = sc;
                    }
                }
            }
        }
    }

    public void Reset()
    {
        _gridController.ResetAllTiles();
        _ActualshipController = null;
        _ActualtilesController = null;
        _isHighLighted = false;
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

    private void OnHoldStarted(InputAction.CallbackContext context)
    {
        _IsHolding = true;
        Vector2 touchedPos = _touchPosition.ReadValue<Vector2>();
        _actualTouchedPosition = Camera.main.ScreenToWorldPoint(touchedPos);
        _actualTouchedPosition.z = 0f;
        Collider2D hitCollider = Physics2D.OverlapPoint(_actualTouchedPosition);
        if (hitCollider != null && hitCollider.TryGetComponent<IDraggable>(out var draggable))
        {
            currentDraggedObject = hitCollider.gameObject;
            currentDraggable = draggable;
            _isDragging = true;
            _isScrolling = false;
            currentDraggable.OnBeginDrag();
        }
        else
        {
            _isDragging = false;
            if (canScroll)
            {
                _isScrolling = true;
                _scrollStartTouchPos = _actualTouchedPosition;
                _scrollStartCameraY = Camera.main.transform.position.y;
            }
        }
    }

    private void OnHoldCanceled(InputAction.CallbackContext context)
    {
        _IsHolding = false;
        if (_isDragging && currentDraggable != null)
        {
            currentDraggable.OnEndDrag();
        }
        _isDragging = false;
        _isScrolling = false;
        currentDraggable = null;
        currentDraggedObject = null;
    }

    // public void SetInteractionEnabled(bool enabled)
    // {
    //     _playerInput.enabled = enabled;
    // }
}
