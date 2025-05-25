using UnityEngine;
using TMPro;

namespace DragonSnake
{
  /// <summary>
  /// Displays the player's score and remaining lives on the HUD.
  /// </summary>
  public class HUD : MonoBehaviour
  {
    [Header("HUD References")]
    [SerializeField] private TextMeshProUGUI txtScore;
    [SerializeField] private TextMeshProUGUI txtLives;

    private void Start()
    {
      // Subscribe to GameManager events
      if (GameManager.Instance != null)
      {
        GameManager.Instance.OnLivesChanged += UpdateLives;
        GameManager.Instance.OnLevelRestart += UpdateHUD;
        GameManager.Instance.OnGameOver += UpdateHUD;
      }
      UpdateHUD();
    }

    private void OnDestroy()
    {
      if (GameManager.Instance != null)
      {
        GameManager.Instance.OnLivesChanged -= UpdateLives;
        GameManager.Instance.OnLevelRestart -= UpdateHUD;
        GameManager.Instance.OnGameOver -= UpdateHUD;
      }
    }

    /// <summary>
    /// Call this to update both score and lives (e.g., on level restart or game over).
    /// </summary>
    private void UpdateHUD()
    {
      UpdateScore();
      UpdateLives(GameManager.Instance != null ? GameManager.Instance.GetCurrentLives() : 0);
    }

    /// <summary>
    /// Updates the score display.
    /// </summary>
    private void UpdateScore()
    {
      // TODO: Replace with actual score logic when implemented
      int score = 0;
      txtScore.text = $"Score: {score}";
    }

    /// <summary>
    /// Updates the lives display.
    /// </summary>
    private void UpdateLives(int lives)
    {
      txtLives.text = $"Lives: {lives}";
    }
  }
}