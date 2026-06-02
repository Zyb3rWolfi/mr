using UnityEngine;
using System.Collections;
using Meta.XR.MRUtilityKit; // Modern Meta SDK compatibility

public class MRUKCombinedShelfScaler : MonoBehaviour
{
    [Header("Combined Asset")]
    [Tooltip("Drag your pre-combined 3-shelf prefab group asset here.")]
    [SerializeField] private GameObject _threeShelfGroupPrefab;

    [Header("Rotation Tools")]
    [Tooltip("Adjust Y to 90 or -90 if the combined model stretches front-to-back instead of side-to-side.")]
    [SerializeField] private Vector3 _meshRotationOffset = Vector3.zero;

    private void Start()
    {
        StartCoroutine(DelayedInitialization());
    }

    private IEnumerator DelayedInitialization()
    {
        yield return new WaitForSeconds(0.1f);

        MRUKAnchor tableAnchor = GetComponent<MRUKAnchor>();

        if (tableAnchor != null && tableAnchor.VolumeBounds.HasValue && _threeShelfGroupPrefab != null)
        {
            ScaleAndCenterGroup(tableAnchor.VolumeBounds.Value);
        }
        else if (_threeShelfGroupPrefab != null)
        {
            // Fallback for empty testing scenes
            Instantiate(_threeShelfGroupPrefab, transform.position, transform.rotation, transform);
        }
    }

    private void ScaleAndCenterGroup(Bounds tableBounds)
    {
        // 1. Determine the long axis length of the scanned table boundary box
        float sizeX = tableBounds.size.x;
        float sizeZ = tableBounds.size.z;
        float tableLongestSide = (sizeX > sizeZ) ? sizeX : sizeZ;

        // 2. FIRST PASS: Spawn the object temporarily at center to measure its absolute grouped geometry
        Quaternion correctionRotation = Quaternion.Euler(_meshRotationOffset);
        Quaternion finalRotation = transform.rotation * correctionRotation;

        GameObject spawnedGroup = Instantiate(_threeShelfGroupPrefab, transform.position, finalRotation, transform);
        spawnedGroup.name = "Shelf_Group_Perfect_Alignment";

        // 3. 🛡️ THE UNIFIED BOUNDS FIX: Loop through EVERY child mesh renderer to calculate the true group boundaries
        MeshRenderer[] childRenderers = spawnedGroup.GetComponentsInChildren<MeshRenderer>();

        if (childRenderers == null || childRenderers.Length == 0)
        {
            Debug.LogError("[Scaler] No MeshRenderers found anywhere inside your shelf prefab group!");
            return;
        }

        // Initialize our tracking box using the very first child mesh found
        Bounds combinedGroupBounds = childRenderers[0].bounds;

        // Loop through all remaining sub-shelves and stretch our tracking box to encapsulate them all
        for (int i = 1; i < childRenderers.Length; i++)
        {
            combinedGroupBounds.Encapsulate(childRenderers[i].bounds);
        }

        // 4. Calculate dimensions based on the true combined boundary results
        float truePhysicalLength = Mathf.Max(combinedGroupBounds.size.x, combinedGroupBounds.size.z);

        // Prevent division-by-zero errors
        if (truePhysicalLength <= 0f) truePhysicalLength = 1.0f;

        // 5. Compute the exact scale multiplier factor needed
        float requiredXScale = tableLongestSide / truePhysicalLength;

        // 6. PIXOT CORRECTION OFFSET: Calculate the gap between the spawned center position and the true geometric center of the combined group
        Vector3 geometricCenterOffset = transform.position - combinedGroupBounds.center;

        // Scale the gap offset vector so it stays proportional after the object stretches along its X axis
        geometricCenterOffset.x *= requiredXScale;

        // Apply the correction shift vector relative to the object's parent space
        spawnedGroup.transform.position += geometricCenterOffset;

        // 7. Scale ONLY the local X axis
        spawnedGroup.transform.localScale = new Vector3(requiredXScale, 1.0f, 1.0f);

        Debug.Log($"[Alignment Fixed] Table: {tableLongestSide:F2}m | Combined Mesh Group Size: {truePhysicalLength:F2}m. Center alignment successfully locked.");
    }
}