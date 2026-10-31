
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    public void OpenShop()
    {
        Load("ShopScene");
    }

    public void OpenCollection()
    {
        Load("CollectionScene");
    }

    public void OpenUpgrade()
    {
        Load("UpgradeScene");
    }

    public void BackToSurface()
    {
        Load("SurfaceHub");
    }

    private void Load(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                "Sahne bulunamadi: " + sceneName
            );
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
