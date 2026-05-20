using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    public bool loop = false;
    public SoundType type = SoundType.SFX;
    
    [HideInInspector]
    public AudioSource source;
}

public enum SoundType
{
    Music,
    SFX
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Music")]
    public Sound menuMusic;
    public Sound gameplayMusic;
    public Sound bossMusic;
    
    [Header("Sound Effects")]
    public Sound shootSound;
    public Sound explosionSound;
    public Sound enemyDeathSound;
    public Sound levelUpSound;
    public Sound playerDeathSound;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;
    [Range(0f, 1f)]
    public float sfxVolume = 0.7f;
    
    private AudioSource musicSource;
    private AudioSource sfxSource;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Create audio sources
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.volume = sfxVolume;
    }
    
    void Start()
    {
        // Apply volume settings
        UpdateVolumes();
    }
    
    void UpdateVolumes()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume;
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }
    
    public void PlayMusic(Sound sound)
    {
        if (sound == null || sound.clip == null) return;
        
        musicSource.clip = sound.clip;
        musicSource.volume = musicVolume * sound.volume;
        musicSource.pitch = sound.pitch;
        musicSource.loop = sound.loop;
        musicSource.Play();
        
        Debug.Log("AudioManager: Playing music - " + sound.name);
    }
    
    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
    
    public void PlaySFX(Sound sound)
    {
        if (sound == null || sound.clip == null) return;
        
        // Play SFX on a temporary source to allow overlapping sounds
        AudioSource tempSource = gameObject.AddComponent<AudioSource>();
        tempSource.clip = sound.clip;
        tempSource.volume = sfxVolume * sound.volume;
        tempSource.pitch = sound.pitch;
        tempSource.loop = sound.loop;
        tempSource.Play();
        
        // Destroy the temporary source after the clip finishes
        Destroy(tempSource, sound.clip.length);
        
        Debug.Log("AudioManager: Playing SFX - " + sound.name);
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }
    
    public void LoadVolumeSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.7f);
        UpdateVolumes();
    }
}
