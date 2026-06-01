using UnityEngine;
using System.Collections;
using Meta.XR.MRUtilityKit; // Modern Meta SDK compatibility

public class MRUKMultiShelfTiler : MonoBehaviour
{
    [Header("Modular Target Asset")]
    [Tooltip("Drag the single standalone 3D shelf prefab mesh asset here.")]
    [SerializeField] private GameObject _shelfPrefab;

    [Header("Fixed Asset Dimensions")]
    [Tooltip("The exact physical length of your shelf prefab unit along its row axis in meters (e.g., 1.0 = 1 meter long).")]
    [SerializeField] private float _singleShelfLength = 1.0f;

    [Header("Rotation Tools")]
    [Tooltip("Rotates the individual shelf meshes themselves if they spawn facing the wrong wall.")]
    [SerializeField] private Vector3 _meshRotationOffset = Vector3.zero;

    private void Start()
    {
        // Allow the MRUtilityKit to safely process and populate surface tracking arrays first
        StartCoroutine(DelayedInitialization());
    }

    private IEnumerator DelayedInitialization()
    {
        yield return new WaitForSeconds(0.1f);

        MRUKAnchor tableAnchor = GetComponent<MRUKAnchor>();

        // Confirm valid tracking volume coordinates exist from your scan map
        if (tableAnchor != null && tableAnchor.VolumeBounds.HasValue && _shelfPrefab != null)
        {
            TilePrefabsToFitAnchor(tableAnchor.VolumeBounds.Value);
        }
        else if (_shelfPrefab != null)
        {
            // Fallback default spawn for testing in empty scenes
            SpawnShelfInstance(transform.position);
        }
    }

    /// <summary>
    /// Measures the dominant long axis of the table box and duplicates prefabs to fill it perfectly.
    /// </summary>
    private void TilePrefabsToFitAnchor(Bounds bounds)
    {
        float sizeX = bounds.size.x;
        float sizeZ = bounds.size.z;

        float tableLength = 0f;
        bool tileAlongX = false;

        // 1. AUTOMATIC LONG AXIS DETECTION
        // Compares dimensions to find out whether the table runs horizontally or vertically
        if (sizeX > sizeZ)
        {
            tableLength = sizeX;
            tileAlongX = true; // Table runs along the local X-axis
        }
        else
        {
            tableLength = sizeZ;
            tileAlongX = false; // Table runs along the local Z-axis
        }

        // 2. CAPACITY MATH
        // Divide the overall table length by the length of one single shelf unit
        // Mathf.FloorToInt ensures we don't spawn a shelf that clips out into mid-air past the desk edge
        int shelfCount = Mathf.FloorToInt(tableLength / _singleShelfLength);
        if (shelfCount < 1) shelfCount = 1; // Always render at least one base segment

        // 3. PERFECT CENTERING MATH
        // Calculate the starting offset point so that the group of shelves sits perfectly centered on the table
        float startOffset = -(shelfCount - 1) * _singleShelfLength * 0.5f;

        // 4. INSTANTIATION LOOP
        for (int i = 0; i < shelfCount; i++)
        {
            Vector3 localOffset = Vector3.zero;
            float stepPosition = startOffset + (i * _singleShelfLength);

            if (tileAlongX)
            {
                // Align them straight along the local X-axis, keeping Y and Z locked to center
                localOffset = new Vector3(stepPosition, 0f, 0f);
            }
            else
            {
                // Align them straight along the local Z-axis, keeping X and Y locked to center
                localOffset = new Vector3(0f, 0f, stepPosition);
            }

            // Convert local spacing matrix coordinates out into world space position coordinates
            Vector3 spawnPosition = transform.TransformPoint(localOffset);
            SpawnShelfInstance(spawnPosition);
        }

        Debug.Log($"[Tiler] Table Length: {tableLength:F2}m. Spawned {shelfCount} modular units to fill the volume.");
    }

    /// <summary>
    /// Spawns a single shelf unit and applies your custom local orientation offset angle.
    /// </summary>
    private void SpawnShelfInstance(Vector3 position)
    {
        Quaternion rotationCorrection = Quaternion.Euler(_meshRotationOffset);
        Quaternion finalRotation = transform.rotation * rotationCorrection;

        GameObject modularUnit = Instantiate(_shelfPrefab, position, finalRotation, transform);
        modularUnit.name = "Shelf_Aisle_Segment_Aligned";
    }
}