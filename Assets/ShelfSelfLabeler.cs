using UnityEngine;
using TMPro;

public class ShelfSelfLabeler : MonoBehaviour
{
    // A global static counter shared across every single instance of this script.
    // It stays in memory and increments one-by-side as each shelf spawns!
    private static int _globalAisleCounter = 0;

    [Header("UI Canvas Asset")]
    [Tooltip("Drag your World Space Canvas prefab template here.")]
    [SerializeField] private GameObject _labelPrefab;

    [Header("Front Label Positioning Layout")]
    [Tooltip("How many meters above the shelf floor level should the text float?")]
    [SerializeField] private float _heightOffset = 1.8f;

    [Tooltip("How many meters FORWARD should the text push to stand in front of the aisle entrance?")]
    [SerializeField] private float _frontOffset = 1.5f;

    private Transform _mainCameraTransform;

    // Optional: Reset the counter if you reload the scene or restart playmode
    private void Awake()
    {
        // If this is the very first shelf initializing, ensure our counter starts at zero
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

        // Generate the label immediately upon being spawned by MRUK
        GenerateLabel();
    }

    private void GenerateLabel()
    {
        if (_labelPrefab == null) return;

        // 1. Increment our static global counter by 1 for this specific instance
        _globalAisleCounter++;
        int assignedNumber = _globalAisleCounter;

        // 2. Start at the raw base position of the shelf model
        Vector3 spawnPosition = transform.position;

        // 3. Elevate it cleanly into the air
        spawnPosition.y += _heightOffset;

        // 4. Push the label straight out in front of the aisle using the shelf's forward direction
        spawnPosition += transform.forward * _frontOffset;

        // 5. Instantiate the floating text canvas flat in the world environment
        GameObject labelInstance = Instantiate(_labelPrefab, spawnPosition, Quaternion.identity);

        // 6. Parent it to this shelf so it moves or clears out with the shelf seamlessly
        labelInstance.transform.SetParent(this.transform, true);
        labelInstance.name = $"Aisle_Label_UI_{assignedNumber}";

        // 7. Update the text string inside the TextMeshPro component dynamically
        TMP_Text textMesh = labelInstance.GetComponentInChildren<TMP_Text>();
        if (textMesh != null)
        {
            textMesh.text = $"Aisle {assignedNumber}";
            textMesh.ForceMeshUpdate();
        }

        // 8. Inject our smooth billboarding logic so the text turns toward your Quest lenses
        AisleSignBillboard billboard = labelInstance.AddComponent<AisleSignBillboard>();
        billboard.Initialize(_mainCameraTransform);
    }

    // Automatically clean up our static memory if the game session stops or changes
    private void OnDestroy()
    {
        _globalAisleCounter = 0;
    }
}

// The billboarding helper component class
public class AisleSignBillboard : MonoBehaviour
{
    private Transform _cameraTransform;

    public void Initialize(Transform headsetCamera)
    {
        _cameraTransform = headsetCamera;
    }

    private void Update()
    {
        if (_cameraTransform != null)
        {
            // Rotate the text box smoothly around the vertical axis to match your viewing angle
            transform.LookAt(transform.position + _cameraTransform.rotation * Vector3.forward,
                             _cameraTransform.rotation * Vector3.up);
        }
    }
}