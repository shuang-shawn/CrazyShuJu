using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Background Music")]
    public AudioClip titleMusic;
    public AudioClip gameMusic;
    public AudioClip endingMusic;

    [Header("Sound Effects")]
    public AudioClip bombPlaceSFX;
    public AudioClip explosionSFX;
    public AudioClip pickupSFX;
    public AudioClip playerHitSFX;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
        PlayMusicForCurrentScene();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForCurrentScene();
    }

    private void PlayMusicForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        AudioClip clipToPlay = null;

        if (sceneName == "TitleScene")
        {
            clipToPlay = titleMusic;
        }
        else if (sceneName == "GameScene")
        {
            clipToPlay = gameMusic;
        }
        else if (sceneName == "JuWinScene" || sceneName == "ShuangWinScene")
        {
            clipToPlay = endingMusic;
        }

        if (clipToPlay != null && musicSource.clip != clipToPlay)
        {
            PlayMusic(clipToPlay);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }
}
