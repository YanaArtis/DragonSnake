using UnityEngine;
using System;

namespace DragonSnake
{
  /// <summary>
  /// Handles game state, score, lives, and level restart logic.
  /// </summary>
  public class GameManager : MonoBehaviour
  {
    public static GameManager Instance { get; private set; }

    public event Action OnLevelRestart;
    public event Action OnGameOver;
    public event Action<int> OnLivesChanged;

    [SerializeField] private int startingLives = 3;
    private int currentLives;

    private void Awake()
    {
      if (Instance != null && Instance != this)
      {
        Destroy(gameObject);
        return;
      }
      Instance = this;
      currentLives = startingLives;
    }

    public void LoseLife()
    {
Debug.Log("LoseLife()");
      currentLives--;
      OnLivesChanged?.Invoke(currentLives);

      if (currentLives > 0)
      {
        RestartLevel();
      }
      else
      {
        GameOver();
      }
    }

    public void RestartLevel()
    {
Debug.Log("GameManager.RestartLevel()");
      OnLevelRestart?.Invoke();
    }

    public void GameOver()
    {
Debug.Log("GameManager.GameOver()");
      OnGameOver?.Invoke();
      // Optionally reset lives and restart for demo
      currentLives = startingLives;
      RestartLevel();
    }

    public void ResetGame()
    {
Debug.Log("GameManager.ResetGame()");
      currentLives = startingLives;
      OnLivesChanged?.Invoke(currentLives);
      RestartLevel();
    }

    public int GetCurrentLives() => currentLives;
  }
}