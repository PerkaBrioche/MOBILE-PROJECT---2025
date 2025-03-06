using GooglePlayGames;
using GooglePlayGames.BasicApi;
using TMPro;
using UnityEngine;

public class PlayGamesController : MonoBehaviour
{
    [SerializeField] private GameObject obj;
    [SerializeField] private GameObject obj2;
    [SerializeField] private TextMeshProUGUI text;

    private static PlayGamesController _instance;
    public static PlayGamesController Instance { get => _instance; }

    private void Awake()
    {
        if (!_instance)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate(status => { });
    }

    public void UnlockAchievement(string achievementID)
    {
        
        PlayGamesPlatform.Instance.ReportProgress(achievementID, 100.0f, success =>
        {
            if (success)
            {
                //Debug.Log("Achievement debloque : " + achievementID);
            }
            else
            {
                //Debug.LogWarning("Impossible de debloquer l'achievement : " + achievementID);
            }
        });
    }

    public void ShowAchievements()
    {
        Social.ShowAchievementsUI();
    }
}