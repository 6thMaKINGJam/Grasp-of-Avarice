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

    [Header("BGM Auto Change Skip Scenes")]
    public List<string> bgmSkipScenes = new List<string> { "Start" };

    // ---------------- SFX ----------------
    [System.Serializable]
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

    // ---------------- Internals ----------------
    private AudioSource bgm;
    private Dictionary<string, SceneBgm> bgmMap;
    private Dictionary<SfxType, SfxData> sfxMap;
    private Coroutine fadeCo;

    // 🔒 WebGL autoplay lock
    private bool userUnlockedAudio = false;
    private SceneBgm pendingBgm; // 유저 입력 전 대기 중인 BGM

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // BGM source
        bgm = gameObject.AddComponent<AudioSource>();
        bgm.loop = true;
        bgm.playOnAwake = false;
        bgm.spatialBlend = 0f;
        bgm.volume = 0f;

        // Maps
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

    void Update()
    {
        // 🔓 첫 유저 입력으로 오디오 잠금 해제
        if (!userUnlockedAudio && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            UnlockAudio();
        }
    }

    private void UnlockAudio()
    {
        userUnlockedAudio = true;

        // 대기 중이던 BGM 있으면 이제 재생
        if (pendingBgm != null)
        {
            PlayBgmInternal(pendingBgm.clip, pendingBgm.volume);
            pendingBgm = null;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (bgmSkipScenes != null && bgmSkipScenes.Contains(scene.name))
            return;

        if (bgmMap.TryGetValue(scene.name, out var data))
        {
            // 🔒 아직 유저 입력 없으면 대기
            if (!userUnlockedAudio)
            {
                pendingBgm = data;
                return;
            }

            PlayBgmInternal(data.clip, data.volume);
        }
    }

    // ---------------- BGM API ----------------
    public void PlayBgm(AudioClip clip, float targetVolume = 0.7f)
    {
        if (clip == null) return;

        if (!userUnlockedAudio)
        {
            // 유저 입력 전이면 예약만
            pendingBgm = new SceneBgm { clip = clip, volume = targetVolume };
            return;
        }

        PlayBgmInternal(clip, targetVolume);
    }

    private void PlayBgmInternal(AudioClip clip, float targetVolume)
    {
        // 같은 곡이면 재시작 금지
        if (bgm.clip == clip)
        {
            if (!bgm.isPlaying) bgm.Play();
            bgm.volume = targetVolume;
            return;
        }

        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeSwap(clip, targetVolume, fadeTime));
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
                bgm.volume = Mathf.Lerp(startVol, 0f, t / time);
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
            bgm.volume = Mathf.Lerp(0f, targetVol, t / time);
            yield return null;
        }
        bgm.volume = targetVol;

        fadeCo = null;
    }

    // ---------------- SFX API ----------------
    public void PlaySfx(SfxType type)
    {
        if (!userUnlockedAudio) return;
        if (!sfxMap.TryGetValue(type, out var data) || data.clip == null) return;

        AudioSource sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.spatialBlend = 0f;
        sfx.volume = Mathf.Clamp01(data.volume * sfxVolume);
        sfx.PlayOneShot(data.clip);

        Destroy(sfx, data.clip.length + 0.05f);
    }

    public void FadeOutBgm(float time)
    {
        if (!userUnlockedAudio) return;

        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeOutOnly(time));
    }

    private IEnumerator FadeOutOnly(float time)
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
}
