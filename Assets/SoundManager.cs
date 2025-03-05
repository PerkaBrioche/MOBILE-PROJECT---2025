using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    
    private AudioSource _audioSource;
    
    [SerializeField] private List<AudioClip> _audioClips = new List<AudioClip>();
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
        
        _audioSource = GetComponent<AudioSource>();
    }
    
    public enum SoundList
    {
        ShipMove,
        ShipAttack,
        ShipDeath,
    }

    public void PlaySound(SoundList sound)
    {
        switch (sound)
        {
            
        }
    }

}
