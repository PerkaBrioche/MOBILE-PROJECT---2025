using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] public List<buttonController> _buttonList;
    public static ButtonManager Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public buttonController.ButtonState GetButtonState(int index)
    {
        return _buttonList[index].GetButtonState();
    }
    
    
}
