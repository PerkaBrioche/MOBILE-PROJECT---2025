using UnityEngine;

public class AchievementTimer : MonoBehaviour
{
    public float requiredTime = 200f;
    
    private float elapsedTime = 0f;
    private bool achievementUnlocked = false;

    void Update()
    {
        if (!achievementUnlocked)
        {
            elapsedTime += Time.deltaTime;
            
            if (elapsedTime >= requiredTime)
            {
                achievementUnlocked = true;
                if (PlayGamesController.Instance != null)
                {
                    PlayGamesController.Instance.UnlockAchievement("CgkI-8jn6L0aEAIQBQ");
                    Debug.Log("Achievement 'True groover !!' debloque !");
                }
                else
                {
                    Debug.LogWarning("Instance de PlayGamesController introuvable !");
                }
            }
        }
    }
}