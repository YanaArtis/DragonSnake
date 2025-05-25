using UnityEngine;

namespace DragonSnake
{
  /// <summary>
  /// Defines the game area boundaries and provides utility methods for boundary checks.
  /// </summary>
  public class GameArea : MonoBehaviour
  {
    [Tooltip("Minimum X and Z (bottom-left corner) of the game area rectangle.")]
    public Vector2 areaMin = new Vector2(-10f, -10f);
    [Tooltip("Maximum X and Z (top-right corner) of the game area rectangle.")]
    public Vector2 areaMax = new Vector2(10f, 10f);

    public static GameArea Instance { get; private set; }

    private void Awake()
    {
      if (Instance != null && Instance != this)
      {
        Destroy(gameObject);
        return;
      }
      Instance = this;
    }

    /// <summary>
    /// Checks if the given world position is within the game area.
    /// </summary>
    public bool IsWithinBounds(Vector3 pos)
    {
      return pos.x >= areaMin.x && pos.x <= areaMax.x &&
             pos.z >= areaMin.y && pos.z <= areaMax.y;
    }
  }
}