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
    private float _actualholdTime =0f;
    
    private Collider2D actualCollider;
    
    private bool _IsHolding = false;

    private ShipController _ActualshipController = null;
    private TilesController _ActualtilesController;
    [SerializeField] private GridController _gridController;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _touchPosition = _playerInput.actions["TouchPosition"];
        _touchPress = _playerInput.actions["SinglePress"];
    }

    private void OnEnable()
    {
        _touchPress.performed += OnTouched;
        Debug.Log("Touch Action Enabled");
    }

    private void OnDisable()
    {
       _touchPress.performed -= OnTouched;
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
        }
        else
        {
            _actualholdTime = 0;
        }
    }

    private void PressReleased( InputAction.CallbackContext context)
    {
        if (!_IsHolding){return; }
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
            if(_ActualtilesController == null)
            {
                _ActualtilesController = tC;
            }
            
            if (_ActualshipController != null)
            {
                if (tC.isHighLighted())
                {
                    // ON DEPLACE LE PERSO
                    
                    tC.ChangeCollider(false);
                    _ActualtilesController.ChangeCollider(true);
                    
                    // OLD BECOME NEW
                    _ActualtilesController = tC;
                    _ActualshipController.SetNewPosition(tC);
                }
                _gridController.ResetAllTiles();
                _ActualshipController = null;

            }
        }
        if(actualCollider.TryGetComponent(out ShipController sc))
        {
            sc.GetPath();
            _ActualshipController = sc;
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




}