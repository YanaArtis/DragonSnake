using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro; // Add TextMeshPro namespace

namespace DragonSnake
{
  /// <summary>
  /// Handles UI buttons for game state transitions (start, pause, resume, etc), and manages UI windows visibility.
  /// Uses InputActionReference for pausing the game.
  /// Shows/hides appropriate windows based on game state.
  /// Includes Credits window functionality.
  /// </summary>
  public class GameUI : MonoBehaviour
  {
    [Header("UI Windows")]
    [SerializeField] private GameObject startGameWindow;
    [SerializeField] private GameObject pauseMenuWindow;
    [SerializeField] private GameObject creditsWindow;

    [Header("Start Game Window")]
    [SerializeField] private Button btnStartGame;
    [SerializeField] private Button btnQuitFromStart; // New Quit button for Start Game Window
    [SerializeField] private Button btnCredits; // New Credits button

    [Header("Pause Menu Window")]
    [SerializeField] private Button btnResume;
    [SerializeField] private Button btnFinishSession;
    [SerializeField] private Button btnQuit; // Existing Quit button for Pause Menu

    [Header("Credits Window")]
    [SerializeField] private Button btnCreditsOK; // OK button in Credits window
    [SerializeField] private TextMeshProUGUI txtDevelopers; // Text component for developers info
    [SerializeField] private TextMeshProUGUI txtVersion; // Text component for version info

    [Header("Input Actions")]
    [SerializeField] private InputActionReference pauseAction; // Assign the pause action here

    [Header("Game Information")]
    [SerializeField] private string gameVersion = "1.0.0";
    [SerializeField] private string[] developers = { "Code Maestro\n(coding)", "Yana Artishcheva\n(game architecture and code review)" };

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
      // Wire up Start Game Window button events
      if (btnStartGame != null)
        btnStartGame.onClick.AddListener(() => GameManager.Instance?.StartGame());
      if (btnQuitFromStart != null)
        btnQuitFromStart.onClick.AddListener(() => GameManager.Instance?.QuitGame());
      if (btnCredits != null)
        btnCredits.onClick.AddListener(ShowCreditsWindow);

      // Wire up Pause Menu Window button events
      if (btnResume != null)
        btnResume.onClick.AddListener(() => GameManager.Instance?.ResumeGame());
      if (btnFinishSession != null)
        btnFinishSession.onClick.AddListener(() => GameManager.Instance?.FinishSession());
      if (btnQuit != null)
        btnQuit.onClick.AddListener(() => GameManager.Instance?.QuitGame());

      // Wire up Credits Window button events
      if (btnCreditsOK != null)
        btnCreditsOK.onClick.AddListener(HideCreditsWindow);

      // Subscribe to game state changes
      if (GameManager.Instance != null)
      {
        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
      }

      // Initialize credits text content
      InitializeCreditsContent();

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
    /// Shows the Credits window and hides the Start Game window.
    /// </summary>
    private void ShowCreditsWindow()
    {
      if (startGameWindow != null)
        startGameWindow.SetActive(false);
      if (creditsWindow != null)
        creditsWindow.SetActive(true);

      Debug.Log("Credits window opened");
    }

    /// <summary>
    /// Hides the Credits window and returns to the Start Game window.
    /// </summary>
    private void HideCreditsWindow()
    {
      if (creditsWindow != null)
        creditsWindow.SetActive(false);
      if (startGameWindow != null)
        startGameWindow.SetActive(true);

      Debug.Log("Credits window closed, returned to Start Game menu");
    }

    /// <summary>
    /// Initializes the credits content with developer names and version information.
    /// </summary>
    private void InitializeCreditsContent()
    {
      // Set developers text
      if (txtDevelopers != null)
      {
        string developersText = "Developed by:\n";
        for (int i = 0; i < developers.Length; i++)
        {
          developersText += developers[i];
          if (i < developers.Length - 1)
            developersText += "\n";
        }
        txtDevelopers.text = developersText;
      }

      // Set version text
      if (txtVersion != null)
      {
        txtVersion.text = $"Version: {gameVersion}";
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
      if (creditsWindow != null)
        creditsWindow.SetActive(false);

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

    /// <summary>
    /// Updates the game version string (useful for build automation).
    /// </summary>
    public void SetGameVersion(string version)
    {
      gameVersion = version;
      InitializeCreditsContent();
    }

    /// <summary>
    /// Updates the developers list (useful for dynamic content).
    /// </summary>
    public void SetDevelopers(string[] devs)
    {
      developers = devs;
      InitializeCreditsContent();
    }
  }
}
