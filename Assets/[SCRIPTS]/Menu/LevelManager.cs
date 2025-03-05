using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public enum LevelObjective
{
    DestroyAllEnemies,
    Escape,
    DestroyMotherShip
}

[System.Serializable]
public class LevelData
{   
    public Button levelButton;
    public int sceneIndex;
    public LevelObjective objective;
    public int turnThresholdForTwoStars;
    public int turnThresholdForThreeStars;
    public Image star1;
    public Image star2;
    public Image star3;
}

public class LevelManager : MonoBehaviour
{
    public List<LevelData> levels;
    [SerializeField] private Animator _animator;
    public GameObject levelInfoPanel;
    public GameObject levelInfoBackground;
    public TextMeshProUGUI levelTitleText;
    public TextMeshProUGUI star1ConditionText;
    public TextMeshProUGUI star2ConditionText;
    public TextMeshProUGUI star3ConditionText;
    public Button playButton;
    public Button closeButton;
    public Image infoStar1Image;
    public Image infoStar2Image;
    public Image infoStar3Image;
    public Sprite fullStarSprite;
    public Sprite emptyStarSprite;

    private void Start()
    {

        levelInfoPanel.SetActive(false);
        if (levelInfoBackground != null)
            levelInfoBackground.SetActive(false);

        foreach (LevelData level in levels)
        {
            if (level.levelButton == null)
                continue;

            Button btn = level.levelButton;
            LevelData tempLevel = level;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => { ShowLevelInfo(tempLevel); });
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(HideLevelInfo);
        }
    }


    private void OnEnable()
    {
        UpdateLevelStars();
    }

    public void ShowLevelInfo(LevelData level)
    {
        levelTitleText.text = "Level " + (level.sceneIndex-1);

        string objectiveText = "";
        switch (level.objective)
        {
            case LevelObjective.DestroyAllEnemies:
                objectiveText = "Destroy all enemies";
                break;
            case LevelObjective.Escape:
                objectiveText = "Escape";
                break;
            case LevelObjective.DestroyMotherShip:
                objectiveText = "Destroy the MotherShip";
                break;
        }
        star1ConditionText.text = objectiveText;
        star2ConditionText.text = "Complete the level in less than " + level.turnThresholdForTwoStars + " turns";
        star3ConditionText.text = "Complete the level in less than " + level.turnThresholdForThreeStars + " turns";

        int bestStars = PlayerPrefs.GetInt("LevelStars_" + level.sceneIndex, 0);
        infoStar1Image.sprite = bestStars >= 1 ? fullStarSprite : emptyStarSprite;
        infoStar2Image.sprite = bestStars >= 2 ? fullStarSprite : emptyStarSprite;
        infoStar3Image.sprite = bestStars >= 3 ? fullStarSprite : emptyStarSprite;

        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(delegate { LoadLevel(level.sceneIndex); });

        levelInfoPanel.SetActive(true);
        if (levelInfoBackground != null)
            levelInfoBackground.SetActive(true);
    }

    public void HideLevelInfo()
    {
        levelInfoPanel.SetActive(false);
        if (levelInfoBackground != null)
            levelInfoBackground.SetActive(false);
    }

    public void LoadLevel(int sceneIndex)
    {
        _animator.SetTrigger("Out");
        StartCoroutine(WaitForTransition(sceneIndex));
    }

    private IEnumerator WaitForTransition(int sceneIndex)
    {
        yield return new WaitForSeconds(1.2f);
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
        int block = 0;
        foreach (LevelData level in levels)
        {
            int bestStars = PlayerPrefs.GetInt("LevelStars_" + level.sceneIndex, 0);

            if (bestStars >= 1)
            {
                level.star1.sprite = fullStarSprite;
                print("ONE FULL STAR SPRITE");
            }
            else
            {
                level.star1.sprite = emptyStarSprite;
                print("EMPTY ONE STAR SPRITE");
            }

            if (bestStars >= 2)
            {
                level.star2.sprite = fullStarSprite; 
                print(" two FULL STAR SPRITE");
            }
            else
            {
                level.star2.sprite = emptyStarSprite;
                print("EMPTY ONE STAR SPRITE");
            }

            if (bestStars >= 3)
            {
                level.star3.sprite = fullStarSprite;
                print("THREE FULL STAR SPRITE");
            }
            else
                {
                    level.star3.sprite = emptyStarSprite;
                    print("EMPTY ONE STAR SPRITE");
                }
        }
    }
}
