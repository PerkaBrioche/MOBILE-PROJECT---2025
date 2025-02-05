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
      //  Vector2 touchedPosition = _touchPosition.ReadValue<Vector2>();
      //  _actualTouchedPosition = Camera.main.ScreenToWorldPoint(touchedPosition);
      
        // ICI SI IL HOLD ON PEUT FAIT QUELQUE CHOSE
        if (_IsHolding)
        {
            if (_isDragging && currentDraggable != null)
            {
                Vector2 touchedPos = _touchPosition.ReadValue<Vector2>();
                _actualTouchedPosition = Camera.main.ScreenToWorldPoint(touchedPos);
                _actualTouchedPosition.z = 0f;
                currentDraggable.OnDrag(_actualTouchedPosition);
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
        if (actualCollider.TryGetComponent(out bounce.IBounce Ib))
        {
            Ib.Bounce();
        }
        else
        {
            Debug.LogError("No IBounce Interface Found");
        }
        
        if (actualCollider.TryGetComponent(out ShipController sc))
        {
            sc.GetPath();
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
        _isDragging = true;
        Vector2 touchedPos = _touchPosition.ReadValue<Vector2>();
        _actualTouchedPosition = Camera.main.ScreenToWorldPoint(touchedPos);
        _actualTouchedPosition.z = 0f;
        Collider2D hitCollider = Physics2D.OverlapPoint(_actualTouchedPosition);
        if (hitCollider != null && hitCollider.TryGetComponent<IDraggable>(out var draggable))
        {
            currentDraggedObject = hitCollider.gameObject;
            currentDraggable = draggable;
        }
        else if (draggablePrefab != null)
        {
            currentDraggedObject = Instantiate(draggablePrefab, _actualTouchedPosition, Quaternion.identity);
            currentDraggable = currentDraggedObject.GetComponent<IDraggable>();
            if (currentDraggable == null)
            {
                Debug.LogError("probleme de prefab");
            }
        }
        currentDraggable?.OnBeginDrag();
    }
    
    private void OnHoldCanceled(InputAction.CallbackContext context)
    {
        _IsHolding = false;
        _isDragging = false;
        if (currentDraggable != null)
        {
            currentDraggable.OnEndDrag();
        }
        currentDraggable = null;
        currentDraggedObject = null;
    }
}
