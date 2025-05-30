using UnityEngine;
using DragonSnake; // Required for GameState and GameManager access

// Ensures an AudioSource component is available on this GameObject for background music.
// You might add more AudioSources for SFX if you want separate volume controls or effects.
[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
  public static SoundManager Instance { get; private set; }

  [Header("Audio Clips")]
  [SerializeField] private AudioClip backgroundMusicClip; // Assign your music clip here

  private AudioSource backgroundMusicSource;
  // No need for _musicWasPaused flag with the refined logic below for this specific case.

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject); // Optional: if you want sound to persist across scenes

    // Setup AudioSource for background music
    backgroundMusicSource = GetComponent<AudioSource>();
    if (backgroundMusicClip != null)
    {
      backgroundMusicSource.clip = backgroundMusicClip;
    }
    backgroundMusicSource.loop = true;
    backgroundMusicSource.playOnAwake = false; // We control playback via game state
  }

  private void Start()
  {
    // Subscribe to GameManager events
    if (GameManager.Instance != null)
    {
      GameManager.Instance.OnGameStateChanged += HandleGameStateChangedForMusic;
      // Initialize music state based on current game state, in case SoundManager initializes after GameManager
      HandleGameStateChangedForMusic(GameState.NotStarted, GameManager.Instance.State);
    }
    else
    {
      Debug.LogError("SoundManager: GameManager.Instance is null in Start. Music control might not work correctly.");
    }
  }

  private void OnDestroy()
  {
    if (GameManager.Instance != null)
    {
      GameManager.Instance.OnGameStateChanged -= HandleGameStateChangedForMusic;
    }
    if (Instance == this)
    {
      Instance = null;
    }
  }

  private void HandleGameStateChangedForMusic(GameState previousState, GameState newState)
  {
    if (backgroundMusicSource == null)
    {
      Debug.LogError("SoundManager: BackgroundMusicSource component is missing. Music control unavailable.");
      return;
    }

    if (backgroundMusicClip == null)
    {
      if (backgroundMusicSource.isPlaying) backgroundMusicSource.Stop();
      if (newState == GameState.Playing)
      {
        Debug.LogWarning("SoundManager: BackgroundMusicClip is not assigned. Cannot play music.");
      }
      return;
    }

    // Ensure the AudioSource has the correct clip assigned.
    // This is useful if you plan to change music clips dynamically later.
    if (backgroundMusicSource.clip != backgroundMusicClip)
    {
      backgroundMusicSource.clip = backgroundMusicClip;
    }

    switch (newState)
    {
      case GameState.Playing:
        if (previousState == GameState.Paused)
        {
          // If the music is not already playing (it shouldn't be if game was paused)
          if (!backgroundMusicSource.isPlaying)
          {
            // Attempt to unpause first. This handles the case where music was properly paused,
            // including at time == 0 (your edge case).
            backgroundMusicSource.UnPause();
            Debug.Log("SoundManager: Attempted UnPause for background music.");

            // If UnPause() didn't make it play (e.g., if it was fully stopped, not just paused,
            // and UnPause() on a stopped source does nothing), then call Play().
            if (!backgroundMusicSource.isPlaying)
            {
              backgroundMusicSource.Play();
              Debug.Log("SoundManager: UnPause didn't start music (it might have been stopped), called Play().");
            }
            else
            {
              Debug.Log("SoundManager: Background music Resumed via UnPause.");
            }
          }
          // If for some reason it's already playing, we don't need to do anything.
        }
        else if (!backgroundMusicSource.isPlaying) // Transitioning from a non-Paused state (e.g., Countdown)
        {
          backgroundMusicSource.Play();
          Debug.Log("SoundManager: Background music Playing (transition from non-paused state).");
        }
        /*
        else if (!backgroundMusicSource.isPlaying)
        {
          backgroundMusicSource.Play();
          Debug.Log("SoundManager: Background music Playing.");
        }
        */
        break;

      case GameState.Paused:
        if (backgroundMusicSource.isPlaying)
        {
          backgroundMusicSource.Pause();
          Debug.Log("SoundManager: Background music Paused.");
        }
        // If not playing, it's already effectively paused or stopped, so no action needed.
        break;

      // States where music should be definitively stopped.
      case GameState.NotStarted:
      case GameState.Countdown: // Music stops during countdown
      case GameState.GameOver:
        if (backgroundMusicSource.isPlaying || backgroundMusicSource.time > 0) // Stop if playing or paused (time > 0 indicates it was paused mid-clip)
        {
          backgroundMusicSource.Stop(); // Stop() also resets time to 0 for the next Play()
          Debug.Log($"SoundManager: Background music Stopped (State: {newState}).");
        }
        break;

      // Transient states: LevelCompleted, LevelFailed.
      // Music continues playing until Countdown, which then stops it.
      case GameState.LevelCompleted:
      case GameState.LevelFailed:
        // No specific action for music here. It will be handled by the subsequent state (Countdown).
        break;
    }
  }

  // Example for future SFX
  // public void PlaySoundEffect(AudioClip clip, Vector3 position, float volume = 1.0f)
  // {
  //     if (clip != null)
  //     {
  //         // For 3D sound effects, you might spawn a temporary AudioSource
  //         // AudioSource.PlayClipAtPoint(clip, position, volume);
  //
  //         // Or use a dedicated SFX AudioSource on this SoundManager
  //         // sfxAudioSource.PlayOneShot(clip, volume);
  //     }
  // }
}
