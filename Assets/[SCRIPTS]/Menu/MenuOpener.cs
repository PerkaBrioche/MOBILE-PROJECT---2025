using UnityEngine;

public class MenuOpener : MonoBehaviour
{
    public GameObject menuPanel;

    private void OnMouseDown()
    {
        if(menuPanel != null)
        {
            menuPanel.SetActive(true);
        }
    }
}