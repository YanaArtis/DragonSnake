using System.Collections.Generic;
using UnityEngine;

namespace DragonSnake
{
  /// <summary>
  /// Object pool for snake segments to avoid frequent instantiation/destruction.
  /// Supports separate pools for head and body segments.
  /// </summary>
  public class SnakeSegmentPool : MonoBehaviour
  {
    public static SnakeSegmentPool Instance { get; private set; }

    [Header("Prefab References")]
    [SerializeField] private GameObject headPrefab;
    [SerializeField] private GameObject bodySegmentPrefab;

    [Header("Pool Settings")]
    [SerializeField] private int initialHeadPoolSize = 5; // Usually only need 1-2 heads
    [SerializeField] private int initialBodyPoolSize = 50; // Need many body segments

    private readonly Queue<GameObject> headPool = new Queue<GameObject>();
    private readonly Queue<GameObject> bodyPool = new Queue<GameObject>();

    private Transform headPoolParent;
    private Transform bodyPoolParent;

    private void Awake()
    {
      if (Instance != null && Instance != this)
      {
        Destroy(gameObject);
        return;
      }
      Instance = this;

      CreatePoolParents();
      InitializePools();
    }

    private void CreatePoolParents()
    {
      // Create parent objects for organization
      GameObject headPoolContainer = new GameObject("Head Pool");
      headPoolContainer.transform.SetParent(transform);
      headPoolParent = headPoolContainer.transform;

      GameObject bodyPoolContainer = new GameObject("Body Pool");
      bodyPoolContainer.transform.SetParent(transform);
      bodyPoolParent = bodyPoolContainer.transform;
    }

    private void InitializePools()
    {
      // Initialize head pool
      for (int i = 0; i < initialHeadPoolSize; i++)
      {
        CreateHeadSegment();
      }

      // Initialize body pool
      for (int i = 0; i < initialBodyPoolSize; i++)
      {
        CreateBodySegment();
      }

      Debug.Log($"SnakeSegmentPool: Initialized pools - Heads: {initialHeadPoolSize}, Bodies: {initialBodyPoolSize}");
    }

    public GameObject GetHeadSegment()
    {
      GameObject segment;

      if (headPool.Count > 0)
      {
        segment = headPool.Dequeue();
        segment.SetActive(true);
      }
      else
      {
        // Create new head segment if pool is empty
        segment = CreateHeadSegment();
        segment.SetActive(true);
        Debug.Log("SnakeSegmentPool: Head pool empty, created new head segment");
      }

      return segment;
    }

    public GameObject GetBodySegment()
    {
      GameObject segment;

      if (bodyPool.Count > 0)
      {
        segment = bodyPool.Dequeue();
        segment.SetActive(true);
      }
      else
      {
        // Create new body segment if pool is empty
        segment = CreateBodySegment();
        segment.SetActive(true);
        Debug.Log("SnakeSegmentPool: Body pool empty, created new body segment");
      }

      return segment;
    }

    public void ReturnHeadSegment(GameObject segment)
    {
      if (segment == null) return;

      segment.SetActive(false);
      segment.transform.SetParent(headPoolParent);
      headPool.Enqueue(segment);
    }

    public void ReturnBodySegment(GameObject segment)
    {
      if (segment == null) return;

      segment.SetActive(false);
      segment.transform.SetParent(bodyPoolParent);
      bodyPool.Enqueue(segment);
    }

    // Legacy method for backward compatibility
    public GameObject GetSegment()
    {
      // Default to body segment for backward compatibility
      return GetBodySegment();
    }

    // Legacy method for backward compatibility
    public void ReturnSegment(GameObject segment)
    {
      if (segment == null) return;

      // Determine which pool to return to based on tag
      if (segment.CompareTag("SnakeHead"))
      {
        ReturnHeadSegment(segment);
      }
      else
      {
        ReturnBodySegment(segment);
      }
    }

    private GameObject CreateHeadSegment()
    {
      if (headPrefab == null)
      {
        Debug.LogError("SnakeSegmentPool: Head prefab is not assigned!");
        return null;
      }

      GameObject segment = Instantiate(headPrefab, headPoolParent);
      ConfigureSegmentPhysics(segment);
      segment.SetActive(false);
      headPool.Enqueue(segment);
      return segment;
    }

    private GameObject CreateBodySegment()
    {
      if (bodySegmentPrefab == null)
      {
        Debug.LogError("SnakeSegmentPool: Body segment prefab is not assigned!");
        return null;
      }

      GameObject segment = Instantiate(bodySegmentPrefab, bodyPoolParent);
      ConfigureSegmentPhysics(segment);
      segment.SetActive(false);
      bodyPool.Enqueue(segment);
      return segment;
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

    public void Prewarm(int headCount, int bodyCount)
    {
      // Prewarm head pool
      for (int i = 0; i < headCount; i++)
      {
        if (headPool.Count >= initialHeadPoolSize + headCount) break;
        CreateHeadSegment();
      }

      // Prewarm body pool
      for (int i = 0; i < bodyCount; i++)
      {
        if (bodyPool.Count >= initialBodyPoolSize + bodyCount) break;
        CreateBodySegment();
      }

      Debug.Log($"SnakeSegmentPool: Prewarmed {headCount} heads and {bodyCount} body segments");
    }

    public int GetHeadPoolCount() => headPool.Count;
    public int GetBodyPoolCount() => bodyPool.Count;
  }
}
