using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    // ---------------- BGM ----------------
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

    // ---------------- SFX ----------------
    [System.Serializable] // ★ 이거 꼭 필요
    public class SfxData
    {
        public SfxType type;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("SFX Table")]
    public List<SfxData> sfxList = new List<SfxData>();

    [Header("SFX")]
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private Dictionary<SfxType, SfxData> sfxMap;

    // ---------------- Internals ----------------
    private AudioSource bgm;
    private Dictionary<string, SceneBgm> bgmMap;
    private Coroutine fadeCo;

    void Awake()
    {
        // 싱글톤
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // BGM AudioSource
        bgm = gameObject.AddComponent<AudioSource>();
        bgm.loop = true;
        bgm.playOnAwake = false;
        bgm.spatialBlend = 0f; // 2D
        bgm.volume = 0f;

        // 맵핑 테이블 만들기
        bgmMap = new Dictionary<string, SceneBgm>();
        foreach (var b in sceneBgms)
        {
            if (!string.IsNullOrWhiteSpace(b.sceneName) && b.clip != null && !bgmMap.ContainsKey(b.sceneName))
                bgmMap.Add(b.sceneName, b);
        }

        sfxMap = new Dictionary<SfxType, SfxData>();
        foreach (var s in sfxList)
        {
            if (s != null && s.clip != null && !sfxMap.ContainsKey(s.type))
                sfxMap.Add(s.type, s);
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

    // ---------------- BGM API ----------------
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

        // Fade Out
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

    // ---------------- SFX API ----------------
    public void PlaySfx(SfxType type)
    {
        if (sfxMap == null || !sfxMap.TryGetValue(type, out var data) || data.clip == null) return;

        AudioSource sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.spatialBlend = 0f;
        sfx.volume = Mathf.Clamp01(data.volume * sfxVolume);
        sfx.PlayOneShot(data.clip);

        Destroy(sfx, data.clip.length + 0.05f);
    }
}
