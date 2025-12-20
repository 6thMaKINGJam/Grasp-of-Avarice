using UnityEngine;

public class StartMenu : MonoBehaviour
{
    [SerializeField] private string startSceneName = "Main";
    [SerializeField] private float transitionFadeTime = 0.5f;

    public void OnClickStart()
    {
        SceneLoader.Load(startSceneName, transitionFadeTime);
    }

    public void OnClickExit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
