using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DragonSnake
{
  /// <summary>
  /// Controls the snake's movement and notifies the GameManager if the snake leaves the game area.
  /// Movement is only active in the Playing state.
  /// Now includes self-collision detection.
  /// The snake's head follows the player's head (camera) orientation on the horizontal plane.
  /// The XR Origin (XR Rig) is moved so that the Main Camera stays on the snake's head, giving the player the feeling of riding the snake.
  /// Uses world positions for robust alignment regardless of XR rig hierarchy or scaling.
  /// The body segments follow the head smoothly, maintaining a fixed distance between each segment.
  /// Uses separate prefabs for head and body segments.
  /// </summary>
  public class SnakeController : MonoBehaviour
  {
    public static SnakeController Instance { get; private set; }

    [Header("Snake Settings")]
    [SerializeField] private int initialLength = 5;
    [SerializeField] private float segmentRadius = 1f;
    [SerializeField] private float moveSpeed = 2.5f; // Units per second

    [Header("Prefab References")]
    [SerializeField] private GameObject headPrefab;
    [SerializeField] private GameObject bodySegmentPrefab;

    [Header("References")]
    [SerializeField] private Transform playerHead; // Assign XR camera here (e.g., "Main Camera")
    [SerializeField] private Transform xrOrigin;   // Assign XR Origin (XR Rig) here

    private readonly List<Transform> segments = new List<Transform>();
    private readonly Queue<Vector3> headPositions = new Queue<Vector3>();
    private SnakeHeadCollisionDetector headCollisionDetector;

    private void Awake()
    {
      if (Instance != null && Instance != this)
      {
        Destroy(gameObject);
        return;
      }
      Instance = this;

      InitializeSnake();
    }

    // Use Start instead of OnEnable to ensure GameManager.Instance is initialized
    // private void OnEnable()
    private void Start()
    {
      // Subscribe to a fixed tick event from a central game loop or timer
      SnakeGameTick.OnTick += OnTick;
      if (GameManager.Instance != null)
      {
        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
      }
else Debug.Log("SnakeController.Start(): GameManager.Instance == null");
    }

    private void OnDisable()
    {
      SnakeGameTick.OnTick -= OnTick;
      if (GameManager.Instance != null)
      {
        GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
      }
else Debug.Log("SnakeController.OnDisable(): GameManager.Instance == null");
    }

    private void InitializeSnake()
    {
      // Instead of destroying, return segments to pool
      // Return all segments to their respective pools
      foreach (var seg in segments)
      {
        if (seg != null)
          if (seg.CompareTag("SnakeHead"))
          {
            SnakeSegmentPool.Instance.ReturnHeadSegment(seg.gameObject);
          }
          else
          {
            SnakeSegmentPool.Instance.ReturnBodySegment(seg.gameObject);
          }
      }
      segments.Clear();
      headPositions.Clear();

      // Create head and body segments from pool
      Vector3 startPos = Vector3.zero;
      Vector3 dir = Vector3.forward;

      for (int i = 0; i < initialLength; i++)
      {
        Vector3 pos = startPos - dir * segmentRadius * i;
        GameObject segObj;

        // Create head segment (first segment) or body segment
        if (i == 0)
        {
          segObj = SnakeSegmentPool.Instance.GetHeadSegment();
          segObj.tag = "SnakeHead";

          // Add head collision detector if not already present
          headCollisionDetector = segObj.GetComponent<SnakeHeadCollisionDetector>();
          if (headCollisionDetector == null)
          {
            headCollisionDetector = segObj.AddComponent<SnakeHeadCollisionDetector>();
          }
        }
        else
        {
          segObj = SnakeSegmentPool.Instance.GetBodySegment();
          segObj.tag = "Snake";
        }

        segObj.transform.SetParent(transform);
        segObj.transform.position = pos;
        segObj.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        segments.Add(segObj.transform);
        headPositions.Enqueue(pos);
      }
    }

    // If you ever need to remove a segment (e.g., on collision), return it to the pool:
    private void RemoveTailSegment()
    {
      if (segments.Count > 0)
      {
        var tail = segments.Last();
        SnakeSegmentPool.Instance.ReturnSegment(tail.gameObject);
        segments.RemoveAt(segments.Count - 1);
      }
    }

    private void OnGameStateChanged(GameState prev, GameState next)
    {
      if (next == GameState.Countdown || next == GameState.NotStarted || next == GameState.LevelFailed || next == GameState.GameOver)
      {
        // Reset snake and XR Origin
        if (xrOrigin != null)
        {
          Vector3 originPos = xrOrigin.position;
          xrOrigin.position = new Vector3(0, originPos.y, 0);
        }
        InitializeSnake();
      }
    }

    /// <summary>
    /// Called on each fixed tick (e.g., 60Hz) by the game tick system.
    /// </summary>
    /// <param name="deltaTime">Time since last tick.</param>
    private void OnTick(float deltaTime)
    {
      // Only move the snake in the Playing state
      if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
        return;

      if (segments.Count == 0 || playerHead == null || xrOrigin == null)
        return;

      // Move head in the direction the player's head is facing, but only on the horizontal plane
      Vector3 headForward = playerHead.forward;
      headForward.y = 0;
      headForward.Normalize();

      Vector3 prevHeadPos = segments[0].position;
      Vector3 newHeadPos = prevHeadPos + headForward * moveSpeed * deltaTime;
      newHeadPos.y = 0; // Keep on horizontal plane

      segments[0].position = newHeadPos;
      segments[0].rotation = Quaternion.LookRotation(headForward, Vector3.up);

      // Store head position for body following
      headPositions.Enqueue(newHeadPos);

      // Move each segment to follow the previous one, maintaining fixed distance
      for (int i = 1; i < segments.Count; i++)
      {
        Transform prev = segments[i - 1];
        Transform curr = segments[i];

        Vector3 dirToPrev = prev.position - curr.position;
        dirToPrev.y = 0;
        float dist = dirToPrev.magnitude;

        if (dist > segmentRadius)
        {
          Vector3 moveDir = dirToPrev.normalized;
          curr.position += moveDir * (dist - segmentRadius);
          curr.position = new Vector3(curr.position.x, 0, curr.position.z);
          curr.rotation = Quaternion.LookRotation(moveDir, Vector3.up);
        }
      }

      // Remove old head positions to keep the queue size reasonable
      while (headPositions.Count > initialLength * 10)
        headPositions.Dequeue();

      // --- XR Origin Movement Logic ---
      // Move the XR Origin so that the Main Camera's X and Z match the snake's head segment (using world positions)
      // (Y is not changed to avoid interfering with headset height/tracking)
      Vector3 cameraWorldPos = playerHead.position;
      Vector3 xrOriginWorldPos = xrOrigin.position;
      Vector3 headSegmentWorldPos = segments[0].position;

      // Calculate the offset needed to align Main Camera's XZ to the snake's head XZ
      Vector3 desiredXROriginPos = xrOriginWorldPos;
      desiredXROriginPos.x += headSegmentWorldPos.x - cameraWorldPos.x;
      desiredXROriginPos.z += headSegmentWorldPos.z - cameraWorldPos.z;
      // Y remains unchanged

      xrOrigin.position = desiredXROriginPos;

      // Game Area Boundary Check (delegated to GameArea)
      if (GameArea.Instance != null && !GameArea.Instance.IsWithinBounds(headSegmentWorldPos))
      {
        if (GameManager.Instance != null)
          GameManager.Instance.LoseLife();
else Debug.Log("SnakeController.OnTick(): trying to call LoseLife() but GameManager.Instance == null");
      }
    }

    /// <summary>
    /// Returns a read-only list of snake segments for collision detection.
    /// </summary>
    public IReadOnlyList<Transform> GetSegments() => segments.AsReadOnly();

    public float GetSegmentRadius() => segmentRadius;
  }
}