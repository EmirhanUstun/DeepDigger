
using UnityEngine;
using UnityEngine.SceneManagement;

public class PageNavigator : MonoBehaviour
{
    public void OpenShop()
    {
        SceneManager.LoadScene("Shop");
    }

    public void OpenCollection()
    {
        SceneManager.LoadScene("Collection");
    }

    public void OpenUpgrades()
    {
        SceneManager.LoadScene("Upgrades");
    }

    public void StartMining()
    {
        SceneManager.LoadScene("MainGame");
    }

    public void BackToHub()
    {
        SceneManager.LoadScene("SurfaceHub");
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
