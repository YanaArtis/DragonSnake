using UnityEngine;

namespace DragonSnake
{
  /// <summary>
  /// Handles collision events for the snake head's front collision detector.
  /// This is a separate component to handle the OnTriggerEnter events.
  /// </summary>
  public class SnakeHeadFrontCollisionHandler : MonoBehaviour
  {
    private SnakeHeadCollisionDetector parentDetector;

    public void Initialize(SnakeHeadCollisionDetector detector)
    {
      parentDetector = detector;
    }

    private void OnTriggerEnter(Collider other)
    {
      if (parentDetector != null)
      {
        parentDetector.OnFrontCollisionDetected(other);
      }
    }
  }
}