using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OptionsManager : MonoBehaviour
{
    public GameObject optionsPanel;
    public GameObject guideImage;
    public GameObject darkBackground;
    public Button menuToggleButton;
    public Button guideButton;
    public Button restartButton;
    public Button mainMenuButton;
    public Button quitButton;
    public Button resetStarsButton;
    private bool optionsOpen = false;
    private bool guideOpen = false;
    

    public void ToggleOptions()
    {
        optionsOpen = !optionsOpen;
        optionsPanel.SetActive(optionsOpen);
        UpdateDarkBackground();
    }

    public void ShowGuide()
    {
        guideOpen = true;
        guideImage.SetActive(true);
        optionsPanel.SetActive(false);
        optionsOpen = false;
        UpdateDarkBackground();
    }

    public void HideGuide()
    {
        if (guideOpen)
        {
            guideOpen = false;
            guideImage.SetActive(false);
            optionsPanel.SetActive(true);
            optionsOpen = true;
            UpdateDarkBackground();
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    public void ResetStars()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    private void UpdateDarkBackground()
    {
        darkBackground.SetActive(optionsOpen || guideOpen);
    }

    private void Start()
    {
        optionsPanel.SetActive(false);
        guideImage.SetActive(false);
        darkBackground.SetActive(false);
        menuToggleButton.onClick.AddListener(ToggleOptions);
        guideButton.onClick.AddListener(ShowGuide);
        restartButton.onClick.AddListener(RestartLevel);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        quitButton.onClick.AddListener(QuitGame);
        if (resetStarsButton != null)
        {
            resetStarsButton.onClick.AddListener(ResetStars);
        }
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            restartButton.gameObject.SetActive(false);
            mainMenuButton.gameObject.SetActive(false);
        }
        else
        {
            restartButton.gameObject.SetActive(true);
            mainMenuButton.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (guideOpen && Input.GetMouseButtonDown(0))
        {
            HideGuide();
        }
    }
}
