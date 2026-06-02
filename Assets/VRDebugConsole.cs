using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class VRDebugConsole : MonoBehaviour
{
    // Static instance allows any other script in your game to call VRDebugConsole.LogError(...) instantly!
    public static VRDebugConsole Instance { get; private set; }

    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI _consoleTextField;

    [Header("Follow Settings")]
    [SerializeField] private float _followSpeed = 5.0f;
    [SerializeField] private float _forwardDistance = 1.5f; // How far in front of your face it floats
    [SerializeField] private Vector3 _heightOffset = new Vector3(0, -0.2f, 0); // Floats slightly below eye-level

    private Transform _centerEyeCamera;
    private List<string> _logHistory = new List<string>();
    private int _maxLines = 12; // Prevents the text from clipping out of the bottom of the box

    private void Awake()
    {
        // Setup Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Automatically find the main VR center-eye camera
        if (Camera.main != null)
        {
            _centerEyeCamera = Camera.main.transform;
        }

        if (_consoleTextField != null)
        {
            _consoleTextField.text = "<color=green>[SYSTEM INITIALIZED]</color>\nWaiting for logs...";
        }
    }

    private void LateUpdate()
    {
        if (_centerEyeCamera == null) return;

        // 1. Calculate target position right in front of the player's face
        Vector3 targetPosition = _centerEyeCamera.position + (_centerEyeCamera.forward * _forwardDistance) + _heightOffset;

        // 2. Smoothly slide the console towards that position over time
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * _followSpeed);

        // 3. Rotate the canvas so it always cleanly faces the player
        transform.rotation = Quaternion.LookRotation(transform.position - _centerEyeCamera.position);
    }

    /// <summary>
    /// Call this from ANY script to push an error code onto your screen.
    /// Example: VRDebugConsole.Instance.LogError("404", "Product data missing!");
    /// </summary>
    public void LogError(string errorCode, string message)
    {
        string formattedMessage = $"<color=red>[ERR_{errorCode}]</color> {message}";
        _logHistory.Add(formattedMessage);

        // Truncate old lines so the console doesn't overflow
        if (_logHistory.Count > _maxLines)
        {
            _logHistory.RemoveAt(0);
        }

        UpdateConsoleUI();
    }

    /// <summary>
    /// Call this to log general system updates that aren't errors.
    /// </summary>
    public void LogInfo(string message)
    {
        _logHistory.Add($"<color=yellow>[INFO]</color> {message}");
        if (_logHistory.Count > _maxLines) _logHistory.RemoveAt(0);
        UpdateConsoleUI();
    }

    private void UpdateConsoleUI()
    {
        if (_consoleTextField == null) return;

        // Join our log list into a single long string separated by newlines
        _consoleTextField.text = string.Join("\n", _logHistory);
    }
}