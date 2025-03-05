using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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

public class LevelManager : MonoBehaviour
{
    public List<LevelData> levels;
    [SerializeField] private Animator _animator;

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
}
