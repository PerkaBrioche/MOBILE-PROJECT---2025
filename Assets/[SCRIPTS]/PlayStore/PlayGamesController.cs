using GooglePlayGames;
using GooglePlayGames.BasicApi;
using TMPro;
using UnityEngine;

public class PlayGamesController : MonoBehaviour
{
    [SerializeField] private GameObject obj;
    [SerializeField] private GameObject obj2;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI debugText; // Champ pour afficher les messages de d�bogage

    void Start()
    {
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate(x => debugText.text = x.ToString());
        //PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
    }

    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            text.text = PlayGamesPlatform.Instance.GetUserId();
            debugText.text = "Connexion r�ussie : " + PlayGamesPlatform.Instance.GetUserId();
        }
        else
        {
            text.text = "Non connect�";
            debugText.text = "�chec de la connexion : " + status.ToString();
            switch (status)
            {
                case SignInStatus.InternalError:
                    debugText.text += "\nErreur interne lors de la connexion.";
                    break;
                case SignInStatus.Canceled:
                    debugText.text += "\nConnexion annul�e par l'utilisateur.";
                    break;
                default:
                    debugText.text += "\nStatut de connexion inconnu.";
                    break;
            }
        }
    }

    #region Instance
    private static PlayGamesController _instance;

    public static PlayGamesController Instance { get => _instance; }

    public void Awake()
    {
        if (!_instance)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion

    // Debloquer un achievement
    public void UnlockAchievement(string achievementID)
    {
        GameObject objet = Instantiate(obj);
        objet.transform.position = new Vector3(-8, 0, 0);
        PlayGamesPlatform.Instance.ReportProgress(achievementID, 100.0f, success =>
        {
            if (success)
            {
                debugText.text = "Achievement d�bloqu� !";
                GameObject objet = Instantiate(obj2);
                objet.transform.position = new Vector3(0, 0, 0);
            }
            else 
            {
                GameObject objet = Instantiate(obj);
                objet.transform.position = new Vector3(1, 0, 0);
                debugText.text = "�chec du d�blocage";
            }
        });
    }

    private void Connect()
    {
        PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
    }

    //exemple d'utilisation

    //UnlockAchievement("CgkIj9xxxxxxEAIQAQ"); // Remplace par l�ID de l�achievement

    //exemple pour voir les succes sur un bouton

    public void ShowAchievements()
    {
        Social.ShowAchievementsUI();
    }
}
