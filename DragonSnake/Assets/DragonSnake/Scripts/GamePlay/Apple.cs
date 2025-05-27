using UnityEngine;

namespace DragonSnake
{
  /// <summary>
  /// Represents an apple that the snake can eat for points.
  /// Uses object pooling for efficient memory management.
  /// Only the snake head can eat apples.
  /// Lifetime countdown pauses when game is paused.
  /// Triggers snake growth when eaten.
  /// </summary>
  public class Apple : MonoBehaviour
  {
    [Header("Apple Settings")]
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private float lifetime = 30f; // Apple disappears after 30 seconds if not eaten

    private float spawnTime;
    private float pausedTime = 0f; // Total time spent paused
    private bool isPooled = false;

    private void OnEnable()
    {
      // Reset spawn time and paused time when apple is activated from pool
      spawnTime = Time.time;
      pausedTime = 0f;
    }

    private void Update()
    {
      // Only check lifetime if apple is active and not pooled
      if (!isPooled)
      {
        // Only decrease lifetime when game is in Playing state
        if (GameManager.Instance != null && GameManager.Instance.State == GameState.Playing)
        {
          float actualLifetime = Time.time - spawnTime - pausedTime;
          if (actualLifetime > lifetime)
          {
            ReturnToPool();
          }
        }
        // If game is paused, track the paused time
        else if (GameManager.Instance != null && GameManager.Instance.State == GameState.Paused)
        {
          pausedTime += Time.deltaTime;
        }
      }
    }

    private void OnTriggerEnter(Collider other)
    {
      Debug.Log($"Apple.OnTriggerEnter(\"{other.gameObject.name}\")");

      // Check if snake head touched the apple
      // Only snake head can eat apples - check specifically for "SnakeHead" tag
      // Also only allow eating during Playing state
      if (!isPooled && other.CompareTag("SnakeHead") &&
        GameManager.Instance != null && GameManager.Instance.State == GameState.Playing)
      {
        // Add score
        GameManager.Instance.AddScore(scoreValue);

        // Trigger snake growth
        if (SnakeController.Instance != null)
        {
          SnakeController.Instance.GrowSnake();
          Debug.Log($"Apple eaten! Score: +{scoreValue}, Snake will grow by 2 segments");
        }
        else
        {
          Debug.LogWarning("Apple eaten but SnakeController.Instance is null!");
        }

        ReturnToPool();
      }
      // Ignore collisions with other snake segments (tagged as "Snake") or during non-playing states
    }

    private void ReturnToPool()
    {
      AppleSpawner.Instance?.ReturnApple(this);
    }

    public int GetScoreValue() => scoreValue;

    public void SetPooled(bool pooled)
    {
      isPooled = pooled;
    }

    public bool IsPooled() => isPooled;

    public void ResetApple()
    {
      spawnTime = Time.time;
      pausedTime = 0f;
      // Reset any other apple-specific state here if needed
    }

    /// <summary>
    /// Gets the remaining lifetime of this apple (for debugging purposes)
    /// </summary>
    public float GetRemainingLifetime()
    {
      if (isPooled) return 0f;
      float actualLifetime = Time.time - spawnTime - pausedTime;
      return Mathf.Max(0f, lifetime - actualLifetime);
    }
  }
}