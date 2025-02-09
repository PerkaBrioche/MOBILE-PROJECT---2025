using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MenuManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    public void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0.0001f, 1f);
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }

    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0.0001f, 1f);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
    }
    
    public void SetSFXVolume(float volume)
    {   
        volume = Mathf.Clamp(volume, 0.0001f, 1f);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
    }

    public void QuitGame()
    {
        Application.Quit(); //ne fonctionne que en mode build donc tkt il marche NORMALEMENT :)
        Debug.Log("Quit Game");
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
    }
    
//    ⠀⣠⣤⣶⣶⣦⣄⡀  ⠀⢀⣤⣴⣶⣶⣤⣀⠀
//    ⣼⣿⣿⣿⣿⣿⣿⣷⣤⣾⣿⣿⣿⣿⣿⣿⣧
//    ⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
//    ⠹⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠏
//    ⠀⠙⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠋⠀
//    ⠀⠀⠀⠙⢿⣿⣿⣿⣿⣿⣿⣿⡿⠛⠁⠀⠀
//    ⠀⠀⠀⠀⠀⠉⢿⣿⣿⣿⠟⠋⠀⠀⠀⠀⠀
//    ⠀⠀⠀⠀⠀⠀⠀⠙⠻⠁⠀⠀⠀
//          ^^^^^^
//         GAB + MAX⠀⠀⠀⠀⠀⠀⠀⠀⠀
    
}