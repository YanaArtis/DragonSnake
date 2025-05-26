namespace DragonSnake
{
  /// <summary>
  /// Enumerates all possible game states for the DragonSnake game.
  /// </summary>
  public enum GameState
  {
    NotStarted,        // 1) Game-not-started state
    Countdown,         // 2) Countdown before game session start
    Playing,           // 3) Game state (player rides the snake)
    Paused,            // 4) Game pause state
    LevelCompleted,    // 5) Level completed
    LevelFailed,       // 6) Level failed (lost life, but has lives left)
    GameOver           // 7) Game over (no lives left)
  }
}
