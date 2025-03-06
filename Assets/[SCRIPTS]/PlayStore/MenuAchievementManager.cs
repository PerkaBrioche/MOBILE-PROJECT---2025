using UnityEngine;

public static class AchievementManager
{
    private static int _level1SceneIndex = 2;
    private static int _level5SceneIndex = 6;
    private static int _level10SceneIndex = 11;
    private static int _firstLevelScene = 2;
    private static int _lastLevelScene = 16;

    private const string Achievement1Key = "Achievement_1_unlocked";
    private const string Achievement2Key = "Achievement_2_unlocked";
    private const string Achievement3Key = "Achievement_3_unlocked";
    private const string Achievement4Key = "Achievement_4_unlocked";
    private const string Achievement5Key = "Achievement_5_unlocked";

    public static void CheckAchievements()
    {
        // Achievement 1 : au moins 1 étoile au niveau _level1SceneIndex
        if (PlayerPrefs.GetInt(Achievement1Key, 0) == 0)
        {
            if (PlayerPrefs.GetInt("LevelStars_" + _level1SceneIndex, 0) >= 1)
            {
                if (PlayGamesController.Instance != null)
                {
                    PlayGamesController.Instance.UnlockAchievement("CgkI-8jn6L0aEAIQAQ");
                    PlayerPrefs.SetInt(Achievement1Key, 1);
                    PlayerPrefs.Save();
                }
            }
        }

        // Achievement 2 : au moins 1 étoile au niveau _level5SceneIndex
        if (PlayerPrefs.GetInt(Achievement2Key, 0) == 0)
        {
            if (PlayerPrefs.GetInt("LevelStars_" + _level5SceneIndex, 0) >= 1)
            {
                if (PlayGamesController.Instance != null)
                {
                    PlayGamesController.Instance.UnlockAchievement("CgkI-8jn6L0aEAIQAg");
                    PlayerPrefs.SetInt(Achievement2Key, 1);
                    PlayerPrefs.Save();
                }
            }
        }

        // Achievement 3 : au moins 1 étoile au niveau _level10SceneIndex
        if (PlayerPrefs.GetInt(Achievement3Key, 0) == 0)
        {
            if (PlayerPrefs.GetInt("LevelStars_" + _level10SceneIndex, 0) >= 1)
            {
                if (PlayGamesController.Instance != null)
                {
                    PlayGamesController.Instance.UnlockAchievement("CgkI-8jn6L0aEAIQBg");
                    PlayerPrefs.SetInt(Achievement3Key, 1);
                    PlayerPrefs.Save();
                }
            }
        }

        // Achievement 4 : tous les niveaux (du premier au dernier) doivent avoir au moins 1 étoile
        if (PlayerPrefs.GetInt(Achievement4Key, 0) == 0)
        {
            bool allFinished = true;
            for (int scene = _firstLevelScene; scene <= _lastLevelScene; scene++)
            {
                if (PlayerPrefs.GetInt("LevelStars_" + scene, 0) < 1)
                {
                    allFinished = false;
                    break;
                }
            }
            if (allFinished)
            {
                if (PlayGamesController.Instance != null)
                {
                    PlayGamesController.Instance.UnlockAchievement("CgkI-8jn6L0aEAIQBA");
                    PlayerPrefs.SetInt(Achievement4Key, 1);
                    PlayerPrefs.Save();
                }
            }
        }

        // Achievement 5 : tous les niveaux doivent avoir 3 étoiles (et donc avoir été joués)
        if (PlayerPrefs.GetInt(Achievement5Key, 0) == 0)
        {
            bool allPerfect = true;
            for (int scene = _firstLevelScene; scene <= _lastLevelScene; scene++)
            {
                // On utilise -1 pour les niveaux non joués
                int stars = PlayerPrefs.GetInt("LevelStars_" + scene, -1);
                if (stars == -1 || stars < 3)
                {
                    allPerfect = false;
                    break;
                }
            }
            if (allPerfect)
            {
                if (PlayGamesController.Instance != null)
                {
                    PlayGamesController.Instance.UnlockAchievement("CgkI-8jn6L0aEAIQAw");
                    PlayerPrefs.SetInt(Achievement5Key, 1);
                    PlayerPrefs.Save();
                }
            }
        }
    }
}
