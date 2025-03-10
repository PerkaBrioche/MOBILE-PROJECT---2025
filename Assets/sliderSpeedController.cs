using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class sliderSpeedController : MonoBehaviour
{
    private Slider _slider;
    [SerializeField] private TextMeshProUGUI _speedText;

    private void Start()
    {
        _slider = GetComponent<Slider>();
        _slider.value = PlayerPrefs.GetInt("Speed");
    }

    public void SliderValueChange()
    {
        GameManager.Instance.ChangeTimeScale((int)_slider.value);
        _speedText.text = _slider.value + "X";
        var bounce = _speedText.GetComponent<bounce>();
        
        bounce.ChangeForce( 1 + (_slider.value * 0.5f));    
        
        bounce.StartBounce();
        PlayerPrefs.SetInt("Speed", (int) _slider.value);
    }
    
}
