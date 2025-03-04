using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    
    public void StartGame()
    {
        StartCoroutine(loadScene());
    }
    
    private IEnumerator loadScene()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(1);
    }
    
    public void ExitGame()
    {
        Application.Quit();
    }
    
    public void ShowCredits()
    {
      //  SceneLoader.Instance.LoadCreditsScene();
    }
    
    public void ShowOptions()
    {
       // SceneLoader.Instance.LoadOptionsScene();
    }
}
