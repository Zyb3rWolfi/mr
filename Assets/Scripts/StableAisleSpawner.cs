using Meta.XR.MRUtilityKit;
// CRUCIAL: This namespace allows us to communicate with Meta's Room Utility Kit
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StableAisleSpawner : MonoBehaviour
{
    [Header("Prefabs to Spawn")]
    [Tooltip("The 'Virtual Store Shelf' prefab containing your row-stocking logic.")]
    [SerializeField] private GameObject _shelfPrefab;

    [Header("Dynamic Multi-Shelf Configuration")]
    [Tooltip("How many total shelves you want to spawn across the room layout.")]
    [Min(1)]
    [SerializeField] private int _totalShelvesToSpawn = 4; // Change this to 4, 6, 8, etc.!

    [Tooltip("The physical distance (in meters) between each individual shelf.")]
    [SerializeField] private float _spacingBetweenShelves = 1.0f; // Adjust this to pack them tighter or wider

    [Header("Manual Layout Offsets (Fine-Tuning)")]
    [Tooltip("Manually shift the center of the entire shelf cluster left or right.")]
    [SerializeField] private float _globalShiftX = 0.0f;

    [Tooltip("Manually shift the whole cluster forward or backward.")]
    [SerializeField] private float _globalShiftZ = 0.0f;

    [Tooltip("The vertical height (Y) where the shelves should rest (0 = Floor level).")]
    [SerializeField] private float _shelfHeightOffset = 0.0f;

    private MRUKRoom _targetRoom;
    private GameObject _aisleContainer;

    private void Start()
    {
        // Boot up the synchronization search loop
        TryInitializeSpawner();
    }

    private void TryInitializeSpawner()
    {
        // 1. Try to fetch the live room currently being streamed from the Quest/Mock Data
        if (MRUK.Instance != null)
        {
            _targetRoom = MRUK.Instance.GetCurrentRoom();
        }

        // Fallback: If the singleton isn't fully ready, look directly in the scene hierarchy
        if (_targetRoom == null)
        {
            _targetRoom = Object.FindFirstObjectByType<MRUKRoom>();
        }

        // 2. Safeguard: If MRUK is still loading the room mesh, wait and try again in 0.5 seconds
        if (_targetRoom == null)
        {
            Debug.LogWarning("[Stable Spawner] Waiting for MRUK Room data... Retrying in 0.5s.");
            Invoke(nameof(TryInitializeSpawner), 0.5f);
            return;
        }

        // 3. SUCCESS! Room data hooked. Let's build our stable coordinate structure
        Debug.Log($"[Stable Spawner] Successfully linked to MRUK Room Anchor: {_targetRoom.gameObject.name}");

        CreateStableMasterContainer();
        SpawnDynamicShelfGrid();
    }

    private void CreateStableMasterContainer()
    {
        // Create the top-level parent container
        _aisleContainer = new GameObject("AISLE_MASTER_CONTAINER");
        _aisleContainer.transform.SetParent(this.transform, false);

        // Lock onto the room's physical position (to handle the 2-meter tracking shift)
        _aisleContainer.transform.position = _targetRoom.transform.position;

        // BUT COMPLETELY RESET THE ROTATION TO GLOBAL IDENTITY (Locks it straight down the Z-axis)
        // This acts as a shield to kill random anchor orientation drifts!
        _aisleContainer.transform.rotation = Quaternion.identity;

        Debug.Log("[Stable Spawner] Master container locked to room position, but forced to a flat global matrix.");
    }

    private void SpawnDynamicShelfGrid()
    {
        if (_shelfPrefab == null) return;

        // Calculate the total combined width of the cluster so we can center it cleanly around the origin
        float totalWidth = (_totalShelvesToSpawn - 1) * _spacingBetweenShelves;
        float startX = _globalShiftX - (totalWidth / 2f);

        // Run a loop for exactly how many shelves you requested in the Inspector!
        for (int i = 0; i < _totalShelvesToSpawn; i++)
        {
            // Calculate the exact incremental step position for this specific shelf row
            float currentShelfX = startX + (i * _spacingBetweenShelves);
            float currentShelfZ = _globalShiftZ;

            // Generate local relative positions inside our locked parent coordinate system
            Vector3 shelfLocalPos = new Vector3(currentShelfX, _shelfHeightOffset, currentShelfZ);

            // Map the local coordinates cleanly into world space
            Vector3 shelfWorldPos = _aisleContainer.transform.TransformPoint(shelfLocalPos);

            // Spawn the shelf prefab completely flat and straight (No random twisting)
            GameObject spawnedShelf = Instantiate(_shelfPrefab, shelfWorldPos, _targetRoom.transform.rotation);
            // Group it underneath our stabilizer folder
            spawnedShelf.transform.SetParent(_aisleContainer.transform, true);

            // Explicitly force their local orientations to zero to kill any prefab-level drift

            // Force a predictable model scale so row-by-row stocking physics render uniformly
            spawnedShelf.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            // Rename them in the hierarchy so they are easy to debug
            spawnedShelf.name = $"VirtualStoreShelf_Row_{i}";
        }

        Debug.Log($"[Stable Spawner] Generated {_totalShelvesToSpawn} aisles successfully.");

        // Launch our iron-clad Late-Update coroutine enforcement to override MRUK's late anchor stretching
        StartCoroutine(ForceStrictRotationAlignmentAfterSpawn());
    }

    private IEnumerator ForceStrictRotationAlignmentAfterSpawn()
    {
        // Wait exactly 2 frames for Unity and MRUK to finish processing spatial anchor transforms
        yield return null;
        yield return null;

        // Loop through everything we just parented under our container and forcefully flatten their angles!
        foreach (Transform child in _aisleContainer.transform)
        {
            child.localRotation = Quaternion.identity;

            // Look for internal 3D visual mesh child components or rotation shields and clean them up too
            if (child.childCount > 0)
            {
                child.GetChild(0).localRotation = Quaternion.identity;
            }
        }

        Debug.Log("[Stable Spawner] Late-Update Enforcement Complete. Enforced perfect rotation across all rows.");
    }
}