using System.Collections.Generic;
using UnityEngine;

namespace DragonSnake
{
  /// <summary>
  /// Object pool for snake segments to avoid frequent instantiation/destruction.
  /// </summary>
  public class SnakeSegmentPool : MonoBehaviour
  {
    [SerializeField] private GameObject segmentPrefab;
    [SerializeField] private int initialPoolSize = 32;

    private readonly Queue<GameObject> pool = new Queue<GameObject>();

    public static SnakeSegmentPool Instance { get; private set; }

    private void Awake()
    {
      if (Instance != null && Instance != this)
      {
        Destroy(gameObject);
        return;
      }
      Instance = this;
      Prewarm();
    }

    private void Prewarm()
    {
      for (int i = 0; i < initialPoolSize; i++)
      {
        var obj = Instantiate(segmentPrefab, transform);
        obj.SetActive(false);
        pool.Enqueue(obj);
      }
    }

    public GameObject GetSegment()
    {
      if (pool.Count > 0)
      {
        var obj = pool.Dequeue();
        obj.SetActive(true);
        return obj;
      }
      // If pool is empty, instantiate a new one (optional: log warning)
      var newObj = Instantiate(segmentPrefab, transform);
      newObj.SetActive(true);
      return newObj;
    }

    public void ReturnSegment(GameObject segment)
    {
      segment.SetActive(false);
      pool.Enqueue(segment);
    }
  }
}
