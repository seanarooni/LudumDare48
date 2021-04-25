using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    // void Start()
    // {
    //     Screen.SetResolution(640, 480, true);
    // }

    public void PlayButtonPressed()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitButtonPressed()
    {
#if UNITY_EDITOR
        Debug.Log("quit button pressed");
#endif
        Application.Quit();
    }
}
