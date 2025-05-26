using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace DragonSnake
{
  /// <summary>
  /// Handles UI buttons for game state transitions (start, pause, resume, etc), and manages UI windows visibility.
  /// Uses InputActionReference for pausing the game.
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

    [Header("Input Actions")]
    [SerializeField] private InputActionReference pauseAction; // Assign the pause action here

    private void Awake()
    {
      // Ensure the pause action is enabled when the GameUI is initialized
      if (pauseAction != null && pauseAction.action != null)
      {
        // Only listen to 'performed' for button press events
        pauseAction.action.performed += OnPauseAction;
        pauseAction.action.Enable();
      }
      else
      {
        Debug.LogError("Pause Action is not assigned in GameUI!");
      }
    }

    private void Start()
    {
      // Wire up button events
      if (btnStartGame != null)
        btnStartGame.onClick.AddListener(() => GameManager.Instance?.StartGame());
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

      // Clean up the pause action
      if (pauseAction != null && pauseAction.action != null)
      {
        pauseAction.action.performed -= OnPauseAction;
        pauseAction.action.Disable();
      }
    }

    private void OnGameStateChanged(GameState prevState, GameState newState)
    {
      UpdateUIForState(newState);
    }

    /// <summary>
    /// Handles the pause action input. Only responds to button press (performed), not release.
    /// </summary>
    private void OnPauseAction(InputAction.CallbackContext context)
    {
      if (GameManager.Instance == null) return;

      // Only handle the action if it was actually performed (button pressed)
      if (!context.performed) return;

      switch (GameManager.Instance.State)
      {
        case GameState.Playing:
          GameManager.Instance.PauseGame();
          Debug.Log("Game paused via controller input");
          break;
        case GameState.Paused:
          GameManager.Instance.ResumeGame();
          Debug.Log("Game resumed via controller input");
          break;
        // Optionally handle other states if needed
        default:
          // Do nothing for other states
          break;
      }
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
