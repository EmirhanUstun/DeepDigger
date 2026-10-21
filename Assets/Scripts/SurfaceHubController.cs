
using UnityEngine;
using UnityEngine.SceneManagement;

public class SurfaceHubController : MonoBehaviour
{
    // KAZMAYA BASLA
    public void StartMining()
    {
        if (!Application.CanStreamedLevelBeLoaded("MainGame"))
        {
            Debug.LogError(
                "MainGame sahnesi Scene List'te bulunamadi!"
            );
            return;
        }

        Debug.Log("Oyuncu madene giriyor...");

        SceneManager.LoadScene("MainGame");
    }
}
