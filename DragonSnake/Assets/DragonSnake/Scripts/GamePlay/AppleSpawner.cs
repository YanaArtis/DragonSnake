using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonSnake
{
  /// <summary>
  /// Spawns apples at regular intervals in free space within the game area.
  /// Uses object pooling for performance optimization.
  /// </summary>
  public class AppleSpawner : MonoBehaviour
  {
    public static AppleSpawner Instance { get; private set; }

    [Header("Spawning Settings")]
    [SerializeField] private GameObject applePrefab;
    [SerializeField] private float spawnInterval = 5f; // seconds
    [SerializeField] private int maxApples = 5; // Maximum apples on field at once
    [SerializeField] private float appleRadius = 0.5f; // Should match snake segment radius
    [SerializeField] private int maxSpawnAttempts = 50; // Max attempts to find free space

    [Header("Object Pool Settings")]
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private bool expandPoolIfNeeded = true; // Allow pool to grow if needed

    [Header("Collision Detection")]
    [SerializeField] private LayerMask obstacleLayerMask = -1; // What layers to consider as obstacles
    [SerializeField] private float collisionCheckRadius = 0.6f; // Slightly larger than apple radius for safety

    private readonly List<Apple> activeApples = new List<Apple>();
    private readonly Queue<Apple> applePool = new Queue<Apple>();
    private Coroutine spawnCoroutine;
    private Transform poolParent; // Parent object for pooled apples

    private void Awake()
    {
      if (Instance != null && Instance != this)
      {
        Destroy(gameObject);
        return;
      }
      Instance = this;

      // Create a parent object for pooled apples to keep hierarchy clean
      GameObject poolContainer = new GameObject("Apple Pool");
      poolContainer.transform.SetParent(transform);
      poolParent = poolContainer.transform;

      InitializePool();
    }

    private void Start()
    {
      // Subscribe to game state changes
      if (GameManager.Instance != null)
      {
        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
      }
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
      switch (newState)
      {
        case GameState.Playing:
          StartSpawning();
          break;
        case GameState.NotStarted:
        case GameState.LevelFailed:
        case GameState.LevelCompleted:
        case GameState.GameOver:
          StopSpawning();
          break;
        case GameState.Paused:
          // Only stop spawning new apples, but don't clear existing ones
          if (spawnCoroutine != null)
          {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
          }
          break;
        // GameState.Countdown - no action needed, let spawning continue from previous state
      }
    }

    private void StartSpawning()
    {
      if (spawnCoroutine == null)
      {
        spawnCoroutine = StartCoroutine(SpawnAppleRoutine());
      }
    }

    private void StopSpawning()
    {
      if (spawnCoroutine != null)
      {
        StopCoroutine(spawnCoroutine);
        spawnCoroutine = null;
      }

      // Return all active apples to pool
      ClearAllApples();
    }

    private IEnumerator SpawnAppleRoutine()
    {
      while (true)
      {
        yield return new WaitForSeconds(spawnInterval);

        // Only spawn if we haven't reached the maximum
        if (activeApples.Count < maxApples)
        {
          TrySpawnApple();
        }
      }
    }

    private void TrySpawnApple()
    {
      if (applePrefab == null || GameArea.Instance == null)
      {
        Debug.LogWarning("AppleSpawner: Missing applePrefab or GameArea.Instance");
        return;
      }

      Vector3 spawnPosition;
      if (FindFreeSpawnPosition(out spawnPosition))
      {
        SpawnAppleAt(spawnPosition);
      }
      else
      {
        Debug.Log("AppleSpawner: Could not find free space to spawn apple");
      }
    }

    private bool FindFreeSpawnPosition(out Vector3 position)
    {
      position = Vector3.zero;

      for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
      {
        // Generate random position within game area
        Vector2 areaMin = GameArea.Instance.areaMin;
        Vector2 areaMax = GameArea.Instance.areaMax;

        float x = Random.Range(areaMin.x + appleRadius, areaMax.x - appleRadius);
        float z = Random.Range(areaMin.y + appleRadius, areaMax.y - appleRadius);
        Vector3 candidatePosition = new Vector3(x, 0, z);

        // Check if this position is free
        if (IsPositionFree(candidatePosition))
        {
          position = candidatePosition;
          return true;
        }
      }

      return false;
    }

    private bool IsPositionFree(Vector3 position)
    {
      // Check collision with physics objects (obstacles, walls, etc.)
      if (Physics.CheckSphere(position, collisionCheckRadius, obstacleLayerMask))
      {
        return false;
      }

      // Check collision with snake segments
      if (SnakeController.Instance != null && IsCollidingWithSnake(position))
      {
        return false;
      }

      // Check collision with existing active apples
      foreach (var apple in activeApples)
      {
        if (apple != null && apple.gameObject.activeSelf)
        {
          float distance = Vector3.Distance(position, apple.transform.position);
          if (distance < collisionCheckRadius * 2) // Two apple radii apart
          {
            return false;
          }
        }
      }

      return true;
    }

    private bool IsCollidingWithSnake(Vector3 position)
    {
      if (SnakeController.Instance == null)
        return false;

      var segments = SnakeController.Instance.GetSegments();
      foreach (var segment in segments)
      {
        if (segment != null)
        {
          float distance = Vector3.Distance(position, segment.position);
          if (distance < collisionCheckRadius + SnakeController.Instance.GetSegmentRadius())
          {
            return true;
          }
        }
      }

      return false;
    }

    private void SpawnAppleAt(Vector3 position)
    {
      Apple apple = GetPooledApple();
      if (apple != null)
      {
        apple.transform.position = position;
        apple.transform.rotation = Quaternion.identity;
        apple.transform.SetParent(null); // Remove from pool parent
        apple.gameObject.SetActive(true);
        apple.ResetApple();
        activeApples.Add(apple);
        Debug.Log($"AppleSpawner: Spawned apple at {position}");
      }
      else
      {
        Debug.LogError("AppleSpawner: Could not get apple from pool!");
      }
    }

    public void ReturnApple(Apple apple)
    {
      if (apple == null) return;

      // Remove from active list
      activeApples.Remove(apple);

      // Deactivate and return to pool
      apple.gameObject.SetActive(false);
      apple.SetPooled(true);
      apple.transform.SetParent(poolParent); // Move to pool parent for organization
      applePool.Enqueue(apple);
    }

    private void ClearAllApples()
    {
      // Create a copy of the list to avoid modification during iteration
      var applesToReturn = new List<Apple>(activeApples);
      foreach (var apple in applesToReturn)
      {
        if (apple != null)
        {
          ReturnApple(apple);
        }
      }
      activeApples.Clear();
    }

    // --- Object Pooling ---
    private void InitializePool()
    {
      for (int i = 0; i < initialPoolSize; i++)
      {
        CreateNewApple();
      }
      Debug.Log($"AppleSpawner: Initialized pool with {initialPoolSize} apples");
    }

    private Apple CreateNewApple()
    {
      GameObject appleObj = Instantiate(applePrefab, poolParent);
      Apple apple = appleObj.GetComponent<Apple>();

      if (apple != null)
      {
        apple.gameObject.SetActive(false);
        apple.SetPooled(true);
        applePool.Enqueue(apple);
        return apple;
      }
      else
      {
        Debug.LogError("AppleSpawner: Apple prefab doesn't have Apple component!");
        Destroy(appleObj);
        return null;
      }
    }

    private Apple GetPooledApple()
    {
      if (applePool.Count > 0)
      {
        Apple apple = applePool.Dequeue();
        apple.SetPooled(false);
        return apple;
      }
      else if (expandPoolIfNeeded)
      {
        // If the pool is empty, create a new apple (expand the pool)
        Debug.Log("AppleSpawner: Pool empty, expanding pool size");
        return CreateNewApple()?.GetComponent<Apple>();
      }
      else
      {
        Debug.LogWarning("AppleSpawner: Pool is empty and expansion is disabled");
        return null;
      }
    }

    public int GetActiveAppleCount() => activeApples.Count;
    public int GetPooledAppleCount() => applePool.Count;

    // Debug method to check pool status
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void LogPoolStatus()
    {
      Debug.Log($"Apple Pool Status - Active: {activeApples.Count}, Pooled: {applePool.Count}");
    }
  }
}