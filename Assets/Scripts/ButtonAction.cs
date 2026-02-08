using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonAction : MonoBehaviour
{
    public void PlayButtonAction()
    {
        AudioManager.instance.PlayUIPressClip();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void MenuButtonAction()
    {
        AudioManager.instance.PlayUIPressClip();
        SceneManager.LoadScene(0);
    }
}
