using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public struct SceneBgm
    {
        public string sceneName;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }

    [System.Serializable]
    public struct SfxData
    {
        public SfxType type;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }

    [Header("BGM Settings")]
    [SerializeField] private List<SceneBgm> sceneBgms = new List<SceneBgm>();
    [SerializeField] private float fadeTime = 0.8f;
    [SerializeField] private List<string> bgmSkipScenes = new List<string> { "Start" };

    [Header("SFX Settings")]
    [SerializeField] private List<SfxData> sfxList = new List<SfxData>();
    [Range(0f, 1f)] [SerializeField] private float globalSfxVolume = 1f;

    private AudioSource bgmSource;
    private AudioSource sfxSource; // SFX 전용 소스 추가 (PlayOneShot 활용)
    
    private readonly Dictionary<string, SceneBgm> bgmMap = new Dictionary<string, SceneBgm>();
    private readonly Dictionary<SfxType, SfxData> sfxMap = new Dictionary<SfxType, SfxData>();
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitAudioSources();
        InitializeMaps();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void InitAudioSources()
    {
        // BGM 소스 설정
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        // SFX 소스 설정 (매번 생성하지 않고 하나로 돌려씀)
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    private void InitializeMaps()
    {
        foreach (var b in sceneBgms)
            if (!string.IsNullOrEmpty(b.sceneName)) bgmMap[b.sceneName] = b;

        foreach (var s in sfxList)
            sfxMap[s.type] = s;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (bgmSkipScenes.Contains(scene.name)) return;

        if (bgmMap.TryGetValue(scene.name, out var data))
        {
            PlayBgm(data.clip, data.volume);
        }
    }

    #region BGM Methods

    public void FadeOutBgm(float duration)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutOnly(duration));
    }

    private IEnumerator FadeOutOnly(float duration)
    {
        // 이미 작성된 공통 로직 FadeVolume 활용
        yield return StartCoroutine(FadeVolume(bgmSource.volume, 0f, duration));
        bgmSource.Stop();
        fadeCoroutine = null;
    }

    public void PlayBgm(AudioClip clip, float volume = 0.7f)
    {
        if (clip == null || bgmSource.clip == clip) return;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeSwap(clip, volume));
    }

    private IEnumerator FadeSwap(AudioClip nextClip, float targetVolume)
    {
        // Fade Out
        if (bgmSource.isPlaying)
        {
            yield return StartCoroutine(FadeVolume(bgmSource.volume, 0f, fadeTime / 2));
        }

        bgmSource.clip = nextClip;
        bgmSource.Play();

        // Fade In
        yield return StartCoroutine(FadeVolume(0f, targetVolume, fadeTime / 2));
        
        fadeCoroutine = null;
    }

    private IEnumerator FadeVolume(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        bgmSource.volume = end;
    }

    #endregion

    #region SFX Methods

    public void PlaySfx(SfxType type)
    {
        if (sfxMap.TryGetValue(type, out var data))
        {
            // PlayOneShot은 여러 소리가 겹쳐서 재생될 수 있어 효과적입니다.
            sfxSource.PlayOneShot(data.clip, data.volume * globalSfxVolume);
        }
    }

    #endregion

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}