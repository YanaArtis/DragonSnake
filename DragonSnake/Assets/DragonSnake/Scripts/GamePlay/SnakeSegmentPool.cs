using System.Collections.Generic;
using UnityEngine;

namespace DragonSnake
{
  /// <summary>
  /// Object pool for snake segments to avoid frequent instantiation/destruction.
  /// </summary>
  public class SnakeSegmentPool : MonoBehaviour
  {
    public static SnakeSegmentPool Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private GameObject segmentPrefab;
    [SerializeField] private int initialPoolSize = 20;

    private readonly Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
      if (Instance != null && Instance != this)
      {
        Destroy(gameObject);
        return;
      }
      Instance = this;
      InitializePool();
    }

    public GameObject GetSegment()
    {
      GameObject segment;

      if (pool.Count > 0)
      {
        segment = pool.Dequeue();
        segment.SetActive(true);
      }
      else
      {
        // If pool is empty, instantiate a new one (optional: log warning)
        segment = Instantiate(segmentPrefab, transform);
        // Configure physics components for newly instantiated segments
        ConfigureSegmentPhysics(segment);
      }

      return segment;
    }

    public void ReturnSegment(GameObject segment)
    {
      segment.SetActive(false);
      segment.transform.SetParent(transform); // Ensure it's parented to the pool
      pool.Enqueue(segment);
    }

    private void InitializePool()
    {
      for (int i = 0; i < initialPoolSize; i++)
      {
        // Create segment with proper parent and physics configuration
        GameObject segment = Instantiate(segmentPrefab, transform);
        // Configure physics components for pre-pooled segments
        ConfigureSegmentPhysics(segment);
        segment.SetActive(false);
        pool.Enqueue(segment);
      }
      Debug.Log($"SnakeSegmentPool: Initialized pool with {initialPoolSize} segments");
    }

    // Remove the old Prewarm method or fix it to use ConfigureSegmentPhysics
    public void Prewarm(int count)
    {
      for (int i = 0; i < count; i++)
      {
        if (pool.Count >= initialPoolSize + count) break;

        // Create segment with proper parent and physics configuration
        GameObject segment = Instantiate(segmentPrefab, transform);
        ConfigureSegmentPhysics(segment);
        segment.SetActive(false);
        pool.Enqueue(segment);
      }
      Debug.Log($"SnakeSegmentPool: Prewarmed {count} additional segments");
    }

    // When creating snake segments, ensure they have proper Rigidbody setup
    private void ConfigureSegmentPhysics(GameObject segment)
    {
      Rigidbody rb = segment.GetComponent<Rigidbody>();
      if (rb == null)
      {
        rb = segment.AddComponent<Rigidbody>();
      }

      // Configure for kinematic movement (controlled by script, not physics)
      rb.isKinematic = true;
      rb.useGravity = false;

      // Ensure collider is present and properly configured
      Collider col = segment.GetComponent<Collider>();
      if (col == null)
      {
        col = segment.AddComponent<SphereCollider>();
      }

      // Snake segments should NOT be triggers (apple is the trigger)
      col.isTrigger = false;
    }

    public int GetPoolCount() => pool.Count;
  }
}
