using System;
using UnityEngine;

namespace DragonSnake
{
  /// <summary>
  /// Provides a fixed tick event for the game, running at 60Hz.
  /// Avoids direct use of Update; use this for game logic ticks.
  /// </summary>
  public class SnakeGameTick : MonoBehaviour
  {
    public static event Action<float> OnTick;

    [SerializeField] private float tickRate = 60f; // 60Hz

    private float tickInterval;
    private float tickTimer;

    private void Awake()
    {
      tickInterval = 1f / tickRate;
      tickTimer = 0f;
    }

    private void OnEnable()
    {
      Application.onBeforeRender += OnBeforeRender;
    }

    private void OnDisable()
    {
      Application.onBeforeRender -= OnBeforeRender;
    }

    private void OnBeforeRender()
    {
      float deltaTime = Time.deltaTime;
      tickTimer += deltaTime;

      while (tickTimer >= tickInterval)
      {
        OnTick?.Invoke(tickInterval);
        tickTimer -= tickInterval;
      }
    }
  }
}