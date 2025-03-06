using UnityEngine;

public static class AchievementManager
{
    private static int _level1SceneIndex = 2;
    private static string _achievementID1 = "909947561083";
    private static int _level5SceneIndex = 6;
    private static string _achievementID2 = "CgkI-8jn6L0aEAIQAQ";
    private static int _level10SceneIndex = 11;
    private static string _achievementID3 = "909947561083";
    private static int _firstLevelScene = 2;
    private static int _lastLevelScene = 16;
    private static string _achievementID4 = "909947561083";
    private static string _achievementID5 = "909947561083";

    private const string Achievement1Key = "Achievement_1_unlocked";
    private const string Achievement2Key = "Achievement_2_unlocked";
    private const string Achievement3Key = "Achievement_3_unlocked";
    private const string Achievement4Key = "Achievement_4_unlocked";
    private const string Achievement5Key = "Achievement_5_unlocked";

    public static void CheckAchievements()
    {
        if (PlayerPrefs.GetInt(Achievement1Key, 0) == 0)
        {
            if (PlayerPrefs.GetInt("LevelStars_" + _level1SceneIndex, 0) >= 1)
            {
                if (PlayGamesController.Instance != null)
                {
                    PlayGamesController.Instance.UnlockAchievement(_achievementID1);
                    PlayerPrefs.SetInt(Achievement1Key, 1);
                    PlayerPrefs.Save();
                }
            }
        }
        if (PlayerPrefs.GetInt(Achievement2Key, 0) == 0)
        {
            if (PlayerPrefs.GetInt("LevelStars_" + _level5SceneIndex, 0) >= 1)
            {
                if (PlayGamesController.Instance != null)
                {
                    PlayGamesController.Instance.UnlockAchievement(_achievementID2);
                    PlayerPrefs.SetInt(Achievement2Key, 1);
                    PlayerPrefs.Save();
                }
            }
        }
        if (PlayerPrefs.GetInt(Achievement3Key, 0) == 0)
        {
            if (PlayerPrefs.GetInt("LevelStars_" + _level10SceneIndex, 0) >= 1)
            {
                if (PlayGamesController.Instance != null)
                {
                    PlayGamesController.Instance.UnlockAchievement(_achievementID3);
                    PlayerPrefs.SetInt(Achievement3Key, 1);
                    PlayerPrefs.Save();
                }
            }
        }
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
                    PlayGamesController.Instance.UnlockAchievement(_achievementID4);
                    PlayerPrefs.SetInt(Achievement4Key, 1);
                    PlayerPrefs.Save();
                }
            }
        }
        if (PlayerPrefs.GetInt(Achievement5Key, 0) == 0)
        {
            bool allPerfect = true;
            for (int scene = _firstLevelScene; scene <= _lastLevelScene; scene++)
            {
                if (PlayerPrefs.GetInt("LevelStars_" + scene, 0) < 3)
                {
                    allPerfect = false;
                    break;
                }
            }
            if (allPerfect)
            {
                if (PlayGamesController.Instance != null)
                {
                    PlayGamesController.Instance.UnlockAchievement(_achievementID5);
                    PlayerPrefs.SetInt(Achievement5Key, 1);
                    PlayerPrefs.Save();
                }
            }
        }
    }
}
