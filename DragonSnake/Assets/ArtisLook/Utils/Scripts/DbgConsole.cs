using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace com.ArtisLook.utils
{
  public class DbgConsole : MonoBehaviour
  {
    [SerializeField] private Camera _cam;
    [SerializeField] private float _startDistanceFromCam = 0.4f;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Collider _collider;
    [SerializeField] private TextMeshProUGUI _txt;

    public static DbgConsole Instance;

    private Queue<string> _msgQueue;
    private Vector3 _v3ToCam = Vector3.zero;
    private static Transform _transform;
    private static Transform _tCam;

    private void Awake()
    {
      if (Instance != null)
      {
        Destroy(this);
        return;
      }
      Instance = this;

      _msgQueue = new Queue<string>();
      _transform = transform;
      _tCam = _cam.transform;
      _canvas.worldCamera = _cam;
      Application.logMessageReceived += OnMsg;
    }

    private void Update()
    {
      Vector3 v3ToCam = (_transform.position - _tCam.position).normalized;
      v3ToCam.y = 0;
      if (v3ToCam.magnitude > float.Epsilon)
      {
        this.transform.rotation = Quaternion.LookRotation(v3ToCam);
      }

      while (_msgQueue.Count > 0)
      {
        if (_txt.text.Length > 0)
        {
          _txt.text += "\n";
        }
        _txt.text += _msgQueue.Dequeue();
      }
    }

    private void OnDestroy()
    {
      Application.logMessageReceived -= OnMsg;
    }

    public void OnMsg(string message, string stackTrace, LogType type)
    {
      if (type == LogType.Error)
      {
        _msgQueue.Enqueue("Err: <color=red>" + message + "</color>");
      }
      else if (type == LogType.Exception)
      {
        _msgQueue.Enqueue("Exc:  <color=magenta>" + message + "</color>");
      }
      else if (type == LogType.Warning)
      {
        _msgQueue.Enqueue("Warning:  <color=yellow>" + message + "</color>");
      }
      else if (type == LogType.Assert)
      {
        _msgQueue.Enqueue("Assert:  <color=cyan>" + message + "</color>");
      }
      else
      {
        _msgQueue.Enqueue("<color=green>" + message + "</color>");
      }
    }

    public void Clear()
    {
      _txt.text = string.Empty;
    }

    public void SetVisibility(bool isVisible)
    {
      _canvas.enabled = isVisible;
      _collider.enabled = isVisible;
    }

    public void Show()
    {
      SetVisibility(true);
    }

    public void Hide()
    {
      SetVisibility(false);
    }

    public bool GetVisibility()
    {
      return _canvas.enabled;
    }
    public void MoveToCameraView()
    {
      _transform.position = _tCam.position + _tCam.forward * _startDistanceFromCam;
    }
  }
}