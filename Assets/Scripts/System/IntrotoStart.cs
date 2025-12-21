using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroToStart : MonoBehaviour
{
    [SerializeField] private VideoPlayer vp;
    [SerializeField] private string nextSceneName = "Start";

    void Awake()
    {
        if (!vp) vp = GetComponent<VideoPlayer>();
        vp.loopPointReached += OnFinished;
    }

    void OnDestroy()
    {
        if (vp) vp.loopPointReached -= OnFinished;
    }

    void OnFinished(VideoPlayer _)
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
