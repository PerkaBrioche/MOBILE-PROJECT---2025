using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class buttonController : MonoBehaviour
{
    [Button] public void UnlockButton()
    {
        buttonState = ButtonState.Unlocked;
        GetComponent<Button>().interactable = true;
        UpdateSprite();
    }
    [Button] public void LockButton()
    {
        buttonState = ButtonState.Locked;
        GetComponent<Button>().interactable = false;
        UpdateSprite();
    }

    public void SetPerfect()
    {
        buttonState = ButtonState.Perfected;
        UpdateSprite();
    }
    
    [SerializeField] private int _buttonID;
    [Foldout("References")]
    [SerializeField] private Sprite _lockedSprite;
    [Foldout("References")]
    [SerializeField] private Sprite _unlockedSprite;
    [Foldout("References")]
    [SerializeField] private Sprite _perfectedSprite;

    public enum ButtonState
    {
        Locked,
        Unlocked,
        Perfected
    }
    
    
    private ButtonState buttonState;

    private void Start()
    {
        //CheckButtonState();
        //UpdateSprite();
    }

    // public void CheckUnlocked()
    // {
    //     if(PlayerPrefs.GetInt(_buttonID.ToString()) == 1)
    //     {
    //         buttonState = ButtonState.Unlocked;
    //         return;
    //     }
    //     if(_buttonID == 0)
    //     {
    //         buttonState = ButtonState.Unlocked;
    //         return;
    //     }
    // }

    // private void CheckButtonState()
    // {
    //     if(buttonState == ButtonState.Unlocked || buttonState  == ButtonState.Perfected){return;}
    //     
    //     int previousButtonID = _buttonID - 1;
    //     if (previousButtonID < 0)
    //     {
    //         return;
    //     }
    //     var previousButtonState = ButtonManager.Instance.GetButtonState(previousButtonID);
    //     print(previousButtonID + " STATE IS: " + previousButtonState);
    //     if(previousButtonState == ButtonState.Perfected || previousButtonState == ButtonState.Unlocked)
    //     {
    //         buttonState = ButtonState.Unlocked;
    //     }
    //     else
    //     {
    //         buttonState = ButtonState.Locked;
    //     }
    // }

    private void UpdateSprite()
    {
        switch (buttonState)
        {
            case ButtonState.Locked:
                GetComponent<Image>().sprite = _lockedSprite;
                break;
            case ButtonState.Unlocked:
                GetComponent<Image>().sprite = _unlockedSprite;
                break;
            case ButtonState.Perfected:
                GetComponent<Image>().sprite = _perfectedSprite;
                break;
        }
    }

    public ButtonState GetButtonState()
    {
        return buttonState;
    }
}
