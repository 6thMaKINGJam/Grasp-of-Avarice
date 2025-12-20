using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    [SerializeField] private string startSceneName = "Main";
    [SerializeField] private float transitionFadeTime = 0.5f;

    public void OnClickStart()
    {
        SceneLoader.Load("Main");
    }

    IEnumerator LoadSceneAfterBgmFade(string sceneName)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.FadeOutBgm(transitionFadeTime);

        // 페이드아웃 끝날 때까지 대기
        yield return new WaitForSecondsRealtime(transitionFadeTime);

        SceneManager.LoadScene(sceneName);
    }

    public void OnClickExit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
