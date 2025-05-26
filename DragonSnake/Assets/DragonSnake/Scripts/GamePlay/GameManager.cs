using UnityEngine;
using System;
using System.Collections;

namespace DragonSnake
{
  /// <summary>
  /// Handles game state, score, lives, level restart logic, and manages the state machine.
  /// </summary>
  public class GameManager : MonoBehaviour
  {
    public static GameManager Instance { get; private set; }

    // Events for code-based subscriptions
    public event Action<GameState, GameState> OnGameStateChanged;
    public event Action OnLevelRestart;
    public event Action OnGameOver;
    public event Action<int> OnLivesChanged;
    public event Action<int> OnScoreChanged;

    [Header("Game Settings")]
    [SerializeField] private int startingLives = 3;
    [SerializeField] private float countdownDuration = 3f; // seconds
    [SerializeField] private float levelCompletedDuration = 2f; // seconds
    [SerializeField] private float levelFailedDuration = 2f; // seconds
    [SerializeField] private float gameOverDuration = 2f; // seconds

    private int currentLives;
    private int currentScore;
    private int currentLevel = 1;

    private GameState _state = GameState.NotStarted;
    public GameState State => _state;

    private Coroutine _stateCoroutine;

    private void Awake()
    {
      if (Instance != null && Instance != this)
      {
        Destroy(gameObject);
        return;
      }
      Instance = this;
      currentLives = startingLives;
      currentScore = 0;
    }

    private void Start()
    {
      SetState(GameState.NotStarted);
    }

    /// <summary>
    /// Main method to change game state.
    /// </summary>
    public void SetState(GameState newState)
    {
      if (_state == newState)
        return;

      GameState prevState = _state;
      _state = newState;
      Debug.Log($"GameManager: State changed from {prevState} to {_state}");

      OnGameStateChanged?.Invoke(prevState, _state);

      // Stop any previous state coroutine
      if (_stateCoroutine != null)
        StopCoroutine(_stateCoroutine);

      // Handle state entry logic
      switch (_state)
      {
        case GameState.NotStarted:
          // Wait for player to press "Start Game"
          break;
        case GameState.Countdown:
          _stateCoroutine = StartCoroutine(CountdownCoroutine());
          break;
        case GameState.Playing:
          // Game logic runs
          break;
        case GameState.Paused:
          // Show pause menu
          break;
        case GameState.LevelCompleted:
          _stateCoroutine = StartCoroutine(LevelCompletedCoroutine());
          break;
        case GameState.LevelFailed:
          _stateCoroutine = StartCoroutine(LevelFailedCoroutine());
          break;
        case GameState.GameOver:
          _stateCoroutine = StartCoroutine(GameOverCoroutine());
          break;
      }
    }

    /// <summary>
    /// Call this from UI to start the game.
    /// </summary>
    public void StartGame()
    {
      currentLives = startingLives;
      currentScore = 0;
      currentLevel = 1;
      OnLivesChanged?.Invoke(currentLives);
      OnScoreChanged?.Invoke(currentScore);
      SetState(GameState.Countdown);
    }

    /// <summary>
    /// Call this to pause the game.
    /// </summary>
    public void PauseGame()
    {
      if (_state == GameState.Playing)
        SetState(GameState.Paused);
    }

    /// <summary>
    /// Call this to resume the game from pause.
    /// </summary>
    public void ResumeGame()
    {
      if (_state == GameState.Paused)
        SetState(GameState.Playing);
    }

    /// <summary>
    /// Call this to finish the game session and return to NotStarted.
    /// </summary>
    public void FinishSession()
    {
      SetState(GameState.NotStarted);
    }

    /// <summary>
    /// Call this to quit the application.
    /// </summary>
    public void QuitGame()
    {
      Application.Quit();
    }

    /// <summary>
    /// Call this when the player completes the level.
    /// </summary>
    public void CompleteLevel()
    {
      SetState(GameState.LevelCompleted);
    }

    /// <summary>
    /// Call this when the player loses a life (e.g., leaves game area).
    /// </summary>
    public void LoseLife()
    {
Debug.Log("LoseLife()");
      if (_state != GameState.Playing)
        return;

      currentLives--;
      OnLivesChanged?.Invoke(currentLives);

      if (currentLives > 0)
      {
        SetState(GameState.LevelFailed);
      }
      else
      {
        SetState(GameState.GameOver);
      }
    }

    /// <summary>
    /// Call this to add score.
    /// </summary>
    public void AddScore(int amount)
    {
      currentScore += amount;
      OnScoreChanged?.Invoke(currentScore);
    }

    public int GetCurrentLives() => currentLives;
    public int GetCurrentScore() => currentScore;
    public int GetCurrentLevel() => currentLevel;

    // --- State coroutines ---

    private IEnumerator CountdownCoroutine()
    {
      float timer = countdownDuration;
      while (timer > 0)
      {
        // Optionally: fire event to update countdown UI
        yield return null;
        timer -= Time.deltaTime;
      }
      SetState(GameState.Playing);
    }

    private IEnumerator LevelCompletedCoroutine()
    {
      yield return new WaitForSeconds(levelCompletedDuration);
      currentLevel++;
      SetState(GameState.Countdown);
    }

    private IEnumerator LevelFailedCoroutine()
    {
      yield return new WaitForSeconds(levelFailedDuration);
      SetState(GameState.Countdown);
    }

    private IEnumerator GameOverCoroutine()
    {
      OnGameOver?.Invoke();
      yield return new WaitForSeconds(gameOverDuration);
      SetState(GameState.NotStarted);
    }
  }
}