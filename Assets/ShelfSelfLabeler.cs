using Meta.XR.MRUtilityKit;
using System.Collections;
using TMPro;
using UnityEngine;

public class ShelfSelfLabeler : MonoBehaviour
{
    private static int _globalAisleCounter = 0;

    [Header("Modular Tiling Layout")]
    [Tooltip("Drag the standalone 3D shelf model prefab asset here.")]
    [SerializeField] private GameObject _singleShelfMeshPrefab;

    [Tooltip("The actual depth/length of your single shelf model mesh in meters (how long it is down the aisle).")]
    [SerializeField] private float _shelfUnitLength = 1.0f;

    [Header("UI Canvas Asset")]
    [Tooltip("Drag your World Space Canvas prefab template here.")]
    [SerializeField] private GameObject _labelPrefab;

    [Header("Front Label Positioning Layout")]
    [Tooltip("How many meters above the shelf floor level should the text float?")]
    [SerializeField] private float _heightOffset = 1.8f;

    [Tooltip("How many meters FORWARD should the text push to stand in front of the aisle entrance?")]
    [SerializeField] private float _frontOffset = 1.5f;

    private Transform _mainCameraTransform;

    private void Awake()
    {
        if (Object.FindObjectsByType<ShelfSelfLabeler>(FindObjectsSortMode.None).Length <= 1)
        {
            _globalAisleCounter = 0;
        }
    }

    private void Start()
    {
        if (Camera.main != null)
        {
            _mainCameraTransform = Camera.main.transform;
        }

        // Wait a fraction of a second for MRUK data to catch up safely
        StartCoroutine(WaitAndInitialize());
    }

    private IEnumerator WaitAndInitialize()
    {
        yield return new WaitForSeconds(0.1f);

        MRUKAnchor anchor = GetComponent<MRUKAnchor>();

        // Read the volume dimensions provided by the room scanner
        if (anchor != null && anchor.VolumeBounds.HasValue && _singleShelfMeshPrefab != null)
        {
            TileShelvesAcrossAnchor(anchor.VolumeBounds.Value);
        }
        else if (_singleShelfMeshPrefab != null)
        {
            Instantiate(_singleShelfMeshPrefab, transform.position, transform.rotation, transform);
        }

        // Generate exactly ONE label for this row entrance
    }

    private void TileShelvesAcrossAnchor(Bounds bounds)
    {
        // --- AXIS SWAP FIX ---
        // Instead of sizing across X (sideways), we read the Z-axis size (the length of the blue box)
        // If your simulated boxes are wider than they are long, change this to math max/min to always grab the long side.
        float tableLength = Mathf.Max(bounds.size.x, bounds.size.z);

        // Calculate how many shelf pieces can fit end-to-end down the aisle length
        int shelfCount = Mathf.FloorToInt(tableLength / _shelfUnitLength);
        if (shelfCount < 1) shelfCount = 1;

        // Center alignment along the long axis
        float startZ = -(shelfCount - 1) * _shelfUnitLength * 0.5f;

        for (int i = 0; i < shelfCount; i++)
        {
            // LINEAR ALIGNMENT FIX: 
            // We lock X and Y to 0 so they stay perfectly centered on the table, 
            // and we stack them sequentially down the local Z-axis (Forward/Backward).
            Vector3 localOffset = new Vector3(0, 0, startZ + (i * _shelfUnitLength));
            Vector3 spawnPos = transform.TransformPoint(localOffset);

            GameObject modularUnit = Instantiate(_singleShelfMeshPrefab, spawnPos, transform.rotation, transform);
            modularUnit.name = $"Shelf_Mesh_Segment_{i + 1}";
        }

        Debug.Log($"[Modular Spawner] Tiled {shelfCount} units down the table length ({tableLength:F2}m).");
    }

    private void OnDestroy()
    {
        _globalAisleCounter = 0;
    }
}