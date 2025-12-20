using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class SceneBgm
    {
        public string sceneName;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.7f;
    }

    [Header("BGM (Scene -> Clip)")]
    public List<SceneBgm> sceneBgms = new List<SceneBgm>();

    [Header("Fade")]
    public float fadeTime = 0.8f;

    [Header("SFX")]
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private AudioSource bgm;
    private Dictionary<string, SceneBgm> bgmMap;
    private Coroutine fadeCo;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgm = gameObject.AddComponent<AudioSource>();
        bgm.loop = true;
        bgm.playOnAwake = false;
        bgm.spatialBlend = 0f; // 2D
        bgm.volume = 0f;

        bgmMap = new Dictionary<string, SceneBgm>();
        foreach (var b in sceneBgms)
        {
            if (!string.IsNullOrWhiteSpace(b.sceneName) && b.clip != null && !bgmMap.ContainsKey(b.sceneName))
                bgmMap.Add(b.sceneName, b);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (bgmMap.TryGetValue(scene.name, out var data))
            PlayBgm(data.clip, data.volume);
    }

    public void PlayBgm(AudioClip clip, float targetVolume = 0.7f)
    {
        if (clip == null) return;

        // 같은 곡이면 볼륨만 맞추고 종료
        if (bgm.clip == clip && bgm.isPlaying)
        {
            if (fadeCo != null) StopCoroutine(fadeCo);
            bgm.volume = targetVolume;
            return;
        }

        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeSwap(clip, targetVolume, fadeTime));
    }

    public void StopBgm()
    {
        if (fadeCo != null) StopCoroutine(fadeCo);
        bgm.Stop();
        bgm.clip = null;
        bgm.volume = 0f;
    }
    
    public void FadeOutBgm(float time)
    {
        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeOutOnly(time));
    }

    IEnumerator FadeOutOnly(float time)
    {
        float t = 0f;
        float startVol = bgm.volume;

        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            float k = (time <= 0f) ? 1f : Mathf.Clamp01(t / time);
            bgm.volume = Mathf.Lerp(startVol, 0f, k);
            yield return null;
        }

        bgm.volume = 0f;
        bgm.Stop();
        fadeCo = null;
    }

    IEnumerator FadeSwap(AudioClip next, float targetVol, float time)
    {
        float t = 0f;
        float startVol = bgm.volume;

        // Fade Out (현재 재생 중일 때만)
        if (bgm.isPlaying && bgm.clip != null && startVol > 0f)
        {
            while (t < time)
            {
                t += Time.unscaledDeltaTime;
                float k = (time <= 0f) ? 1f : Mathf.Clamp01(t / time);
                bgm.volume = Mathf.Lerp(startVol, 0f, k);
                yield return null;
            }
        }

        bgm.Stop();
        bgm.clip = next;
        bgm.Play();

        // Fade In
        t = 0f;
        bgm.volume = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            float k = (time <= 0f) ? 1f : Mathf.Clamp01(t / time);
            bgm.volume = Mathf.Lerp(0f, targetVol, k);
            yield return null;
        }
        bgm.volume = targetVol;

        fadeCo = null;
    }

    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;

        var sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.spatialBlend = 0f;
        sfx.volume = Mathf.Clamp01(sfxVolume * volumeScale);
        sfx.PlayOneShot(clip);

        Destroy(sfx, clip.length + 0.05f);
    }
}
