using System.Collections;
using UnityEngine;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;

    [SerializeField] private CanvasGroup canvasGroup;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public IEnumerator FadeOut(float time)
    {
        canvasGroup.blocksRaycasts = true;

        float t = 0f;
        float start = canvasGroup.alpha;

        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            float k = (time <= 0f) ? 1f : Mathf.Clamp01(t / time);
            canvasGroup.alpha = Mathf.Lerp(start, 1f, k);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    public IEnumerator FadeIn(float time)
    {
        float t = 0f;
        float start = canvasGroup.alpha;

        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            float k = (time <= 0f) ? 1f : Mathf.Clamp01(t / time);
            canvasGroup.alpha = Mathf.Lerp(start, 0f, k);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }
}
