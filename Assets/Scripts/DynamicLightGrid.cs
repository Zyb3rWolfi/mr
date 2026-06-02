using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class DynamicLightGrid : MonoBehaviour
{
    [Header("Light Configuration")]
    [SerializeField] private GameObject _lampPrefab; // Drag your warm yellow lamp prefab here

    [Header("Grid Layout Settings")]
    [SerializeField] private float _spacingX = 1.5f; // Drop a light every 1.5 meters along the width
    [SerializeField] private float _spacingZ = 1.5f; // Drop a light every 1.5 meters along the length

    private void Start()
    {
        transform.Rotate(90, 0, 0);
        GenerateLightGrid();
    }

    private void GenerateLightGrid()
    {
        BoxCollider ceilingBounds = GetComponent<BoxCollider>();
        if (ceilingBounds == null || _lampPrefab == null) return;

        // Extract the exact physical dimensions MRUK assigned to this ceiling tile
        Vector3 size = ceilingBounds.size;
        Vector3 center = ceilingBounds.center;

        // Calculate the boundaries of your physical lab roof
        float minX = center.x - (size.x / 2f) + (_spacingX / 2f);
        float maxX = center.x + (size.x / 2f);
        float minZ = center.z - (size.z / 2f) + (_spacingZ / 2f);
        float maxZ = center.z + (size.z / 2f);

        // Run a double layout loop across the grid coordinates
        for (float x = minX; x < maxX; x += _spacingX)
        {
            for (float z = minZ; z < maxZ; z += _spacingZ)
            {
                // Calculate local coordinate point
                Vector3 localSpawnPos = new Vector3(x, center.y - 0.05f, z); // Lower slightly so it sits below the plaster

                // Convert to real-world room coordinates
                Vector3 worldSpawnPos = transform.TransformPoint(localSpawnPos);

                // Spawn your warm yellow shop lamp!
                GameObject lightFixture = Instantiate(_lampPrefab, worldSpawnPos, Quaternion.identity);

                // Parent it to this ceiling container so your project hierarchy stays pristine
                lightFixture.transform.SetParent(this.transform);
            }
        }

        Debug.Log("[Ceiling Grid] Modular lighting array mapped to lab dimensions successfully.");
    }
}
