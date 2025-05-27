using UnityEngine;

namespace DragonSnake
{
  /// <summary>
  /// Represents an apple that the snake can eat for points.
  /// Uses object pooling for efficient memory management.
  /// Only the snake head can eat apples.
  /// </summary>
  public class Apple : MonoBehaviour
  {
    [Header("Apple Settings")]
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private float lifetime = 30f; // Apple disappears after 30 seconds if not eaten

    private float spawnTime;
    private bool isPooled = false;

    private void OnEnable()
    {
      // Reset spawn time when apple is activated from pool
      spawnTime = Time.time;
    }

    private void Update()
    {
      // Only check lifetime if apple is active and not pooled
      if (!isPooled && Time.time - spawnTime > lifetime)
      {
        ReturnToPool();
      }
    }

    private void OnTriggerEnter(Collider other)
    {
      Debug.Log($"Apple.OnTriggerEnter(\"{other.gameObject.name}\")");

      // Check if snake head touched the apple
      // Only snake head can eat apples - check specifically for "SnakeHead" tag
      if (!isPooled && other.CompareTag("SnakeHead"))
      {
        // Add score
        if (GameManager.Instance != null)
        {
          GameManager.Instance.AddScore(scoreValue);
        }

        ReturnToPool();
      }
      // Ignore collisions with other snake segments (tagged as "Snake")
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
      // Reset any other apple-specific state here if needed
    }
  }
}