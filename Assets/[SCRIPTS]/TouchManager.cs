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
    
    // HOLD

    [SerializeField] private GameObject draggablePrefab;
    private bool _isDragging = false;
    private IDraggable currentDraggable;
    private GameObject currentDraggedObject;
    

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
        // UPDATE LA POSITION 
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
        // ICI ON JOUE UNE ACTIONS LORSQU'ON RESTE APPUYER SUR UN OBJET
    }

    private void GetTouchPositon(InputAction.CallbackContext context)
    {
        
    }
    
    private void OnTouched(InputAction.CallbackContext context)
    {
        // LORSQUE LE JOUEUR APPUIE *UNE FOIS* SUR L'ECRAN
        Vector2 touchedPosition = _touchPosition.ReadValue<Vector2>();
        _actualTouchedPosition = Camera.main.ScreenToWorldPoint(touchedPosition);       
        actualCollider = GetCollider();
        if (actualCollider == null)
        {
            return;
        }
        
        // INTERFACE BOUNCE QUI SERA REMPLACER PAR INTERACTABLE
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
            if (_isHighLighted)
            {
                if (tC.isHighLighted())
                {
                    _ActualshipController.SetNewPosition(tC);
                }
                _gridController.ResetAllTiles();
                _ActualshipController = null;
            }



        }
        if(actualCollider.TryGetComponent(out ShipController sc))
        
        if (actualCollider.TryGetComponent(out ShipController sc))
        {
            if (_ActualshipController == null)
            {
                _isHighLighted = true;
                sc.GetPath();
                _ActualshipController = sc;
            }
            else
            {
                _gridController.ResetAllTiles();
                _ActualshipController = null;
            }

        }
        else
        {
            Debug.LogError("No ShipController Found");
        }
    }
    
    private Collider2D GetCollider()
    {
        // ON RETOURNE L'OOBJET APPUYER
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
