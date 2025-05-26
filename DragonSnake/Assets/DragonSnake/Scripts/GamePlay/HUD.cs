using UnityEngine;
using TMPro;

namespace DragonSnake
{
  /// <summary>
  /// Displays the player's score and remaining lives on the HUD.
  /// Also displays game state-specific overlays (e.g., countdown, pause, level complete).
  /// </summary>
  public class HUD : MonoBehaviour
  {
    [Header("HUD References")]
    [SerializeField] private TextMeshProUGUI txtScore;
    [SerializeField] private TextMeshProUGUI txtLives;
    [SerializeField] private TextMeshProUGUI txtOverlay; // For countdown, pause, etc.

    private void Start()
    {
      // Subscribe to GameManager events
      if (GameManager.Instance != null)
      {
        GameManager.Instance.OnLivesChanged += UpdateLives;
        GameManager.Instance.OnScoreChanged += UpdateScore;
        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
      }
      UpdateHUD();
    }

    private void OnDestroy()
    {
      if (GameManager.Instance != null)
      {
        GameManager.Instance.OnLivesChanged -= UpdateLives;
        GameManager.Instance.OnScoreChanged -= UpdateScore;
        GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
      }
    }

    /// <summary>
    /// Call this to update both score and lives (e.g., on level restart or game over).
    /// </summary>
    private void UpdateHUD()
    {
      UpdateScore(GameManager.Instance != null ? GameManager.Instance.GetCurrentScore() : 0);
      UpdateLives(GameManager.Instance != null ? GameManager.Instance.GetCurrentLives() : 0);
      UpdateOverlay(GameManager.Instance != null ? GameManager.Instance.State : GameState.NotStarted);
    }

    /// <summary>
    /// Updates the score display.
    /// </summary>
    private void UpdateScore(int score)
    {
      txtScore.text = $"Score: {score}";
    }

    /// <summary>
    /// Updates the lives display.
    /// </summary>
    private void UpdateLives(int lives)
    {
      txtLives.text = $"Lives: {lives}";
    }

    private void OnGameStateChanged(GameState prev, GameState next)
    {
      UpdateOverlay(next);
    }

    private void UpdateOverlay(GameState state)
    {
      switch (state)
      {
        case GameState.NotStarted:
          txtOverlay.text = "Press START GAME to begin";
          txtOverlay.gameObject.SetActive(true);
          break;
        case GameState.Countdown:
          txtOverlay.text = $"Level {GameManager.Instance.GetCurrentLevel()}\nGet Ready!";
          txtOverlay.gameObject.SetActive(true);
          break;
        case GameState.Playing:
          txtOverlay.gameObject.SetActive(false);
          break;
        case GameState.Paused:
          txtOverlay.text = "PAUSED\nResume | Finish | Quit";
          txtOverlay.gameObject.SetActive(true);
          break;
        case GameState.LevelCompleted:
          txtOverlay.text = "Level Completed!";
          txtOverlay.gameObject.SetActive(true);
          break;
        case GameState.LevelFailed:
          txtOverlay.text = "Level Failed!\nTry Again!";
          txtOverlay.gameObject.SetActive(true);
          break;
        case GameState.GameOver:
          txtOverlay.text = "GAME OVER";
          txtOverlay.gameObject.SetActive(true);
          break;
      }
    }
  }
}