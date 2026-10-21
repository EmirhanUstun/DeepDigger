
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void Oyna()
    {
        SceneManager.LoadScene("SurfaceHub");
    }

    public void Ayarlar()
    {
        SceneManager.LoadScene("Settings");
    }

    public void AnaMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
