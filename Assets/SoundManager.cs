using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    
    private AudioSource _audioSource;
    
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    
    
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
    
    public void Start()
    {
        
        masterVolumeSlider.value = PlayerPrefs.GetFloat("Master", 1);
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1);
        
        audioMixer.SetFloat("Master", GetDB( masterVolumeSlider.value));
        audioMixer.SetFloat("MusicVolume", GetDB( musicVolumeSlider.value));
        audioMixer.SetFloat("SFXVolume", GetDB( sfxVolumeSlider.value));
    }

    public void SetMasterVolume()
    {
        float dB = GetDB(masterVolumeSlider.value);
        audioMixer.SetFloat("Master", dB);
        PlayerPrefs.SetFloat("Master", masterVolumeSlider.value);
        
        print("Master Volume: " + masterVolumeSlider.value);
    }
    public void SetMusicVolume()
    {
        float dB = GetDB(musicVolumeSlider.value);
        audioMixer.SetFloat("MusicVolume", dB);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
    }

    public void SetSFXVolume()
    {
        float dB = GetDB(sfxVolumeSlider.value);
        audioMixer.SetFloat("SFXVolume", dB);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
    }

    public float GetDB(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1);
        return Mathf.Log10(value) * 20;
    }

    public enum SoundList // IN ORDER
    {
        ShipAttack,
        ShipDeath,
        ButtonClick,
        Win,
        Lose,
        Stars,
    }
    
    public void PlayButtonSound()
    {
        PlaySound(SoundList.ButtonClick);
    }

    public void PlaySound(SoundList sound)
    {
        switch (sound)
        {
            case SoundList.ShipAttack:
                _audioSource.PlayOneShot(_audioClips[Random.Range(0, 3)]);
                break;
            case SoundList.ShipDeath:
                _audioSource.PlayOneShot(_audioClips[3]);
                break;
            case SoundList.ButtonClick:
                _audioSource.PlayOneShot(_audioClips[4]);
                break;
            case SoundList.Win:
                _audioSource.PlayOneShot(_audioClips[5]);
                break;
            case SoundList.Lose:
                _audioSource.PlayOneShot(_audioClips[6]);
                break;
            case SoundList.Stars:
                _audioSource.PlayOneShot(_audioClips[7]);
                break;
            
        }
    }

}
