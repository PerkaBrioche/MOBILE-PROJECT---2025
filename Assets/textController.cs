using System;
using UnityEngine;
using TMPro;

public class textController : MonoBehaviour
{
    [SerializeField] private Animator _myAnimator;
    [SerializeField] private TextMeshProUGUI _myText;
    
    public void ShowDamage(int damage)
    {
        _myText.text = "-"+ damage;
        _myAnimator.SetTrigger("ShowDamage");
    }
}
