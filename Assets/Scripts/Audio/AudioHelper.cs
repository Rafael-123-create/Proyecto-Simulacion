using UnityEngine;

public static class AudioHelper
{
    public static void PlayMenuMusic()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.menuMusic != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic);
        }
    }
    
    public static void PlayGameplayMusic()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.gameplayMusic != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.gameplayMusic);
        }
    }
    
    public static void PlayBossMusic()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.bossMusic != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.bossMusic);
        }
    }
    
    public static void StopMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }
    }
    
    public static void PlayShoot()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.shootSound != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.shootSound);
        }
    }
    
    public static void PlayExplosion()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.explosionSound != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.explosionSound);
        }
    }
    
    public static void PlayGameOver()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.gameOverSound != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.gameOverSound);
        }
    }
    
    public static void PlayLevelUp()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.levelUpSound != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.levelUpSound);
        }
    }
    
    public static void PlayPlayerDeath()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.playerDeathSound != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.playerDeathSound);
        }
    }
}
