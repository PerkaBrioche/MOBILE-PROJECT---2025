using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private bool _canTouch = true;
    
    [SerializeField]  private float cooldownTouch = 0.5f;
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
    
    public void SetCanTouch(bool canTouch)
    {
        _canTouch = canTouch;
    }   
    public bool CanTouch()
    {
        return _canTouch;
    }

    public void TouchScreen()
    {
        _canTouch = false;
        StartCoroutine(CoolDownTouch());
    }

    private IEnumerator CoolDownTouch()
    {
        yield return new WaitForSeconds(cooldownTouch);
        _canTouch = true;
    }
    


}
