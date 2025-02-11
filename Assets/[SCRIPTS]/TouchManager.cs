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
    private bool interactionEnabled = true;

    public void SetInteractionEnabled(bool enabled)
    {
        interactionEnabled = enabled;
    }

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _touchPosition = _playerInput.actions["TouchPosition"];
        _touchPress = _playerInput.actions["SinglePress"];
        _holdPress = _playerInput.actions["HoldPress"];
    }

    private void OnEnable()
    {
        _touchPress.performed += OnTouched;
        _holdPress.started += OnHoldStarted;
        _holdPress.canceled += OnHoldCanceled;
        Debug.Log("Touch Action Enabled");
    }

    private void OnDisable()
    {
        _touchPress.performed -= OnTouched;
        _holdPress.started -= OnHoldStarted;
        _holdPress.canceled -= OnHoldCanceled;
        Debug.Log("Touch Action Disabled");
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
                Camera.main.transform.position = new Vector3(
                    Camera.main.transform.position.x,
                    newCameraY,
                    Camera.main.transform.position.z
                );
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
        if (!interactionEnabled) return;
        Vector2 touchedPosition = _touchPosition.ReadValue<Vector2>();
        _actualTouchedPosition = Camera.main.ScreenToWorldPoint(touchedPosition);       
        actualCollider = GetCollider();
        if (actualCollider == null)
        {
            return;
        }
        if (actualCollider.TryGetComponent(out bounce.IBounce Ib))
        {
            Ib.Bounce();
        }
        else
        {
            Debug.LogError("No IBounce Interface Found");
        }
        if (actualCollider.TryGetComponent(out TilesController tC))
        {
            Debug.Log("CLICKED ON TILES CONTROLLER");
            if (_isHighLighted)
            {
                if (tC.isHighLighted())
                {
                    if (_ActualshipController != null)
                    {
                        if (tC.IsAnAttackTile())
                        {
                            Reset();
                        }
                        else
                        {
                            _ActualshipController.SetNewPosition(tC);
                        }
                    }
                    else
                    {
                        Debug.LogError("NO SHIP CONTROLLER SELECTED");
                    }
                }
            }
            Reset();
        }
        if (actualCollider.TryGetComponent(out ShipController sc))
        {
            Debug.Log("CLICKED ON SHIP CONTROLLER");
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
                    CombatManager cm = FindObjectOfType<CombatManager>();
                    if(cm != null && _ActualshipController != null)
                    {
                        cm.StartCombat(_ActualshipController, sc);
                    }
                    Reset();
                }
                else
                {
                    if(_ActualshipController == sc)
                    {
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
        else
        {
            Debug.Log("NO SHIP CONTROLLER SELECTED");
        }
    }

    private void Reset()
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
        Debug.DrawLine(_actualTouchedPosition, _actualTouchedPosition + Vector3.right * 0.1f, Color.magenta, 3f);
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
            Debug.Log("Début du drag sur l'unité : " + currentDraggedObject.name);
        }
        else
        {
            _isDragging = false;
            if (canScroll)
            {
                _isScrolling = true;
                _scrollStartTouchPos = _actualTouchedPosition;
                _scrollStartCameraY = Camera.main.transform.position.y;
                Debug.Log("Début du scroll de la scène.");
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
}
