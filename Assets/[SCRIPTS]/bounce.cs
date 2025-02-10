using System;
using System.Collections;
using UnityEngine;

public class bounce : MonoBehaviour
{
    
    [SerializeField]  private float _bounceForce = 1f;
    [SerializeField] private float _bounceDuration = 0.05f;
    [SerializeField] private float _bounceDecreaseSpeed = 3f;
    
    
    
    private Vector3 originalScale;
    
    public interface IBounce
    {
        public void Bounce();
    }
    
    
    private void Start()
    {
        originalScale = transform.localScale;
    }


    public void StartBouncePARAM(float bounceForce, float bounceDuration, bool holded)
    {
        if(bounceForce == 0) {bounceForce = _bounceForce;}
        if(bounceDuration == 0) {bounceDuration = _bounceDuration;}
        
        StartCoroutine(bounceEnumerator(bounceForce, bounceDuration, holded));
    }
    
    public void StartBounce()
    {
        StartCoroutine(bounceEnumerator(_bounceForce, _bounceDuration, false));
    }
    

    private IEnumerator bounceEnumerator(float bounceForce, float bounceDuration, bool stay)
    {
        float alpha = 0f;
        Vector3 targetScale = new Vector3(transform.localScale.x * bounceForce, transform.localScale.y * bounceForce );
        while (alpha <= bounceDuration)
        {
            alpha += Time.deltaTime;
            Vector3 newScaleObject = Vector3.Lerp(originalScale, targetScale, alpha);
            transform.localScale = newScaleObject;
            yield return null;
        }
        if (stay) {yield break;}
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * _bounceDecreaseSpeed;
            Vector3 newScaleObject = Vector3.Lerp(originalScale, targetScale, alpha);
            transform.localScale = newScaleObject;
            yield return null;
        }
    }  
    
        
    public void ResetTransform()
    {
        StartCoroutine(ResetScale());
    }
    private IEnumerator ResetScale()
    {
        float alpha = 1;
        Vector3 targetScale = transform.localScale;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * _bounceDecreaseSpeed;
            Vector3 newScaleObject = Vector2.Lerp(originalScale, targetScale, alpha);
            transform.localScale = newScaleObject;
            yield return null;
        }
    }
}
