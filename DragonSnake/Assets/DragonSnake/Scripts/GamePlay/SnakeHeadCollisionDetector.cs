using UnityEngine;
using System.Collections.Generic;

namespace DragonSnake
{
  /// <summary>
  /// Detects when the snake head collides with its own body segments.
  /// Uses a frontal collision detector to avoid false positives with adjacent segments.
  /// </summary>
  public class SnakeHeadCollisionDetector : MonoBehaviour
  {
    [Header("Collision Detection")]
    [SerializeField] private float frontColliderOffset = 0.3f; // Distance in front of head center
    [SerializeField] private float frontColliderRadius = 0.2f; // Radius of front collision detector
    [SerializeField] private int minSegmentDistance = 3; // Minimum segments away to trigger collision

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Color gizmoColor = Color.red;

    private SphereCollider frontCollider;
    private GameObject frontColliderObject;
    private SnakeController snakeController;

    private void Start()
    {
      snakeController = GetComponentInParent<SnakeController>();
      if (snakeController == null)
      {
        Debug.LogError("SnakeHeadCollisionDetector: Could not find SnakeController in parent!");
        return;
      }

      CreateFrontCollider();
    }

    private void CreateFrontCollider()
    {
      // Create a child GameObject for the front collision detector
      frontColliderObject = new GameObject("FrontCollisionDetector");
      frontColliderObject.transform.SetParent(transform);
      frontColliderObject.transform.localPosition = Vector3.forward * frontColliderOffset;
      frontColliderObject.transform.localRotation = Quaternion.identity;

      // Add and configure the sphere collider
      frontCollider = frontColliderObject.AddComponent<SphereCollider>();
      frontCollider.radius = frontColliderRadius;
      frontCollider.isTrigger = true;

      // Add the collision handler component
      var collisionHandler = frontColliderObject.AddComponent<SnakeHeadFrontCollisionHandler>();
      collisionHandler.Initialize(this);

      // Set layer if needed (optional)
      frontColliderObject.layer = gameObject.layer;
    }

    /// <summary>
    /// Called by the front collision handler when a collision is detected.
    /// </summary>
    public void OnFrontCollisionDetected(Collider other)
    {
      // Only process during playing state
      if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
        return;

      // Check if the collided object is a snake segment
      if (!other.CompareTag("Snake") && !other.CompareTag("SnakeHead"))
        return;

      // Get the segment index of the collided segment
      int collidedSegmentIndex = GetSegmentIndex(other.transform);

      // If we can't find the segment or it's too close to the head, ignore
      if (collidedSegmentIndex < 0 || collidedSegmentIndex < minSegmentDistance)
        return;

      // Snake hit itself - trigger level failure
      Debug.Log($"Snake head collision detected with segment {collidedSegmentIndex}!");
      if (GameManager.Instance != null)
      {
        GameManager.Instance.LoseLife();
      }
    }

    /// <summary>
    /// Gets the index of a segment in the snake's segment list.
    /// </summary>
    private int GetSegmentIndex(Transform segmentTransform)
    {
      if (snakeController == null)
        return -1;

      var segments = snakeController.GetSegments();
      for (int i = 0; i < segments.Count; i++)
      {
        if (segments[i] == segmentTransform)
          return i;
      }

      return -1; // Not found
    }

    private void OnDrawGizmos()
    {
      if (!showDebugGizmos)
        return;

      // Draw the front collision detector sphere
      Gizmos.color = gizmoColor;
      Vector3 frontPosition = transform.position + transform.forward * frontColliderOffset;
      Gizmos.DrawWireSphere(frontPosition, frontColliderRadius);
    }

    private void OnDestroy()
    {
      if (frontColliderObject != null)
      {
        DestroyImmediate(frontColliderObject);
      }
    }
  }
}