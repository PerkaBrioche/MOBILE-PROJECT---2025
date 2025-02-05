using System;
using System.Collections;
using UnityEngine;

public class BpmManager : MonoBehaviour
{
    [Header("BPM INFO")]
    [SerializeField] private float _bpm;
    [SerializeField] private AudioClip _audioClip;

    [Header("OTHERS")]
    [SerializeField] private GridController _gridController;
    
    private AudioSource _audioSource;
    
    [SerializeField] private bool _canBeat;
    private bool _isBeating;
    
    private float _beatPerSeconds;
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = _audioClip;
        _audioSource.Play();
        
        _beatPerSeconds = GetBeatPerSeconds();
        print(_beatPerSeconds);
    }

    private void Update()
    {
        if (_canBeat)
        {
            if (!_isBeating)
            {
                _isBeating = true;
                StartCoroutine(BeatCooldown());
            }
        }

    }
    
    private void Beat()
    {
        _gridController.ShineEffect();
    }
    
    private IEnumerator BeatCooldown()
    {
        Beat();
        yield return new WaitForSeconds(_beatPerSeconds);
        _isBeating = false;
    }

    private float GetBeatPerSeconds()
    {
        return (60 / _bpm);
    }
    
    
}
