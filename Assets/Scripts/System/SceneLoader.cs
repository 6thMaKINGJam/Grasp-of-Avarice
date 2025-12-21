using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;
    public static bool IsTransitioning => Instance != null && Instance._isTransitioning;

    [SerializeField] private float defaultFadeTime = 0.5f;

    private bool _isTransitioning;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 어디서든 호출하는 공용 함수
    public static void Load(string sceneName, float fadeTime = -1f)
    {
        if (Instance == null)
        {
            Debug.LogError("[SceneLoader] SceneLoader가 씬에 없음. 첫 씬에 SceneLoader 오브젝트 추가해줘.");
            return;
        }

        if (fadeTime < 0f) fadeTime = Instance.defaultFadeTime;

        if (Instance._isTransitioning) return; // 연타 방지
        Instance.StartCoroutine(Instance.CoLoad(sceneName, fadeTime));
    }

    private IEnumerator CoLoad(string sceneName, float fadeTime)
    {
        _isTransitioning = true;

        // 1) 화면 페이드아웃 + BGM 페이드아웃 동시에
        if (AudioManager.Instance != null)
            AudioManager.Instance.FadeOutBgm(fadeTime);

        if (SceneFader.Instance != null)
            yield return SceneFader.Instance.FadeOut(fadeTime);
        else
            yield return new WaitForSecondsRealtime(fadeTime);

        if (AudioManager.Instance != null)
        AudioManager.Instance.PlaySfx(SfxType.Transition);

        // 2) 씬 로드(Async 추천)
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        // 3) 새 씬에서 화면 페이드인
        if (SceneFader.Instance != null)
            yield return SceneFader.Instance.FadeIn(fadeTime);

        _isTransitioning = false;
    }
}
