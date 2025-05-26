using UnityEngine;
using UnityEngine.UI;

namespace DragonSnake
{
  /// <summary>
  /// Handles UI buttons for game state transitions (start, pause, resume, etc), and manages UI windows visibility.
  /// Shows/hides appropriate windows based on game state.
  /// </summary>
  public class GameUI : MonoBehaviour
  {
    [Header("UI Windows")]
    [SerializeField] private GameObject startGameWindow;
    [SerializeField] private GameObject pauseMenuWindow;

    [Header("Start Game Window")]
    [SerializeField] private Button btnStartGame;

    [Header("Pause Menu Window")]
    [SerializeField] private Button btnResume;
    [SerializeField] private Button btnFinishSession;
    [SerializeField] private Button btnQuit;

    [Header("In-Game Controls")]
    [SerializeField] private Button btnPause; // This button should be visible during gameplay

    private void Start()
    {
      // Wire up button events
      if (btnStartGame != null)
        btnStartGame.onClick.AddListener(() => GameManager.Instance?.StartGame());
      if (btnPause != null)
        btnPause.onClick.AddListener(() => GameManager.Instance?.PauseGame());
      if (btnResume != null)
        btnResume.onClick.AddListener(() => GameManager.Instance?.ResumeGame());
      if (btnFinishSession != null)
        btnFinishSession.onClick.AddListener(() => GameManager.Instance?.FinishSession());
      if (btnQuit != null)
        btnQuit.onClick.AddListener(() => GameManager.Instance?.QuitGame());

      // Subscribe to game state changes
      if (GameManager.Instance != null)
      {
        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
      }

      // Initialize UI state
      UpdateUIForState(GameManager.Instance != null ? GameManager.Instance.State : GameState.NotStarted);
    }

    private void OnDestroy()
    {
      if (GameManager.Instance != null)
      {
        GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
      }
    }

    private void OnGameStateChanged(GameState prevState, GameState newState)
    {
      UpdateUIForState(newState);
    }

    /// <summary>
    /// Updates UI window visibility based on the current game state.
    /// </summary>
    private void UpdateUIForState(GameState state)
    {
      // Hide all windows first
      if (startGameWindow != null)
        startGameWindow.SetActive(false);
      if (pauseMenuWindow != null)
        pauseMenuWindow.SetActive(false);

      // Show pause button only during gameplay
      if (btnPause != null)
        btnPause.gameObject.SetActive(state == GameState.Playing);

      // Show appropriate window based on state
      switch (state)
      {
        case GameState.NotStarted:
        case GameState.GameOver:
          if (startGameWindow != null)
            startGameWindow.SetActive(true);
          break;

        case GameState.Paused:
          if (pauseMenuWindow != null)
            pauseMenuWindow.SetActive(true);
          break;

        case GameState.Countdown:
        case GameState.Playing:
        case GameState.LevelCompleted:
        case GameState.LevelFailed:
          // No special windows needed for these states
          // HUD overlay handles the display
          break;
      }
    }
  }
}
