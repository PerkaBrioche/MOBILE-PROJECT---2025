using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class LevelData
{
    public Button levelButton;
    public int sceneIndex;
    public int turnThresholdForThreeStars;
    public int turnThresholdForTwoStars;
    public Image star1;
    public Image star2;
    public Image star3;
}

public class MenuManager : MonoBehaviour
{
    public GameObject menuPanel;
    public AudioMixer audioMixer;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public List<LevelData> levels;

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
        Application.Quit();
        Debug.Log("Quit Game");
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
    }
    
    private void Start()
    {
        foreach (LevelData level in levels)
        {
            Button btn = level.levelButton;
            int scene = level.sceneIndex;
            btn.onClick.AddListener(delegate { LoadLevel(scene); });
        }
        UpdateLevelStars();
    }

    public void LoadLevel(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void SaveLevelStars(int sceneIndex, int stars)
    {
        int currentBest = PlayerPrefs.GetInt("LevelStars_" + sceneIndex, 0);
        if (stars > currentBest)
        {
            PlayerPrefs.SetInt("LevelStars_" + sceneIndex, stars);
            PlayerPrefs.Save();
        }
    }

    public void UpdateLevelStars()
    {
        foreach (LevelData level in levels)
        {
            int bestStars = PlayerPrefs.GetInt("LevelStars_" + level.sceneIndex, 0);
            if (level.star1 != null)
                level.star1.gameObject.SetActive(bestStars >= 1);
            if (level.star2 != null)
                level.star2.gameObject.SetActive(bestStars >= 2);
            if (level.star3 != null)
                level.star3.gameObject.SetActive(bestStars >= 3);
        }
    }

    public void ResetAllStars()
    {
        foreach (LevelData level in levels)
        {
            PlayerPrefs.SetInt("LevelStars_" + level.sceneIndex, 0);
        }
        PlayerPrefs.Save();
        UpdateLevelStars();
    }
}
