using UnityEngine;
using UnityEngine.SceneManagement;

public class Start : MonoBehaviour
{
    [SerializeField] private string startSceneName = "Map 1";

    public void OnClickStart()
    {
        SceneManager.LoadScene("Main");
    }

    public void OnClickExit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
