using UnityEngine;
using TMPro;

public class ProductUIPopup : MonoBehaviour
{
    [Header("UI Text References")]
    [SerializeField] private GameObject _parent;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private TMP_Text _idText;

    [Header("Animation Settings")]
    [SerializeField] private bool _facePlayer = true;

    [Header("Positioning Layout")]
    [Tooltip("How much to raise the popup above the product center so it doesn't completely cover the item mesh.")]
    [SerializeField] private float _heightOffset = 0.05f;

    [Tooltip("How far the popup pushes forward from the product center directly TOWARDS the player's face.")]
    [SerializeField] private float _frontOffset = 0.2f; // Increased slightly to clear the front of the item cleanly

    private Transform _mainCameraTransform;

    private void Start()
    {
        if (Camera.main != null)
        {
            _mainCameraTransform = Camera.main.transform;
        }
        HidePopup();
    }

    private void Update()
    {
        if (_facePlayer && _mainCameraTransform != null && _parent.activeSelf)
        {
            // Keep facing the player's Quest eyes smoothly
            transform.LookAt(transform.position + _mainCameraTransform.rotation * Vector3.forward,
                             _mainCameraTransform.rotation * Vector3.up);
        }
    }

    /// <summary>
    /// Updates the visual text elements and activates the canvas panel.
    /// </summary>
    public void DisplayProductInfo(StoreProduct product)
    {
        if (product == null)
        {
            Debug.LogError("[UI Popup] Received a null product reference!");
            return;
        }
        if (product.Data == null)
        {
            Debug.LogError($"[UI Popup] {product.gameObject.name} is missing its ProductData ScriptableObject file!");
            return;
        }

        // --- FIXED FRONT-AND-CENTER POSITIONING MATH ---
        // 1. Start at the exact world center position of the product item
        Vector3 targetPosition = product.transform.position;

        // 2. Add the vertical height adjustment so it floats cleanly near eye-level or slightly above the item base
        targetPosition.y += _heightOffset;

        // 3. THE FIX: Calculate the direct direction line vector straight from the product to your headset lenses
        if (_mainCameraTransform != null)
        {
            Vector3 dirToPlayer = (_mainCameraTransform.position - product.transform.position);
            dirToPlayer.y = 0; // Flatten the vector on the Y axis so the popup doesn't fly upwards if you stand tall
            dirToPlayer.Normalize(); // Turn it into a clean 1-meter direction unit vector

            // Push the popup straight out in front of the product along this line directly toward you
            targetPosition += dirToPlayer * _frontOffset;
        }

        // Apply the newly calculated front-centered coordinates to this UI Canvas root transform
        transform.position = targetPosition;

        // Explicit Text Assignments
        _nameText.text = product.Data.productName;
        _priceText.text = $"${product.Data.price:F2}";
        _idText.text = $"ID: {product.Data.productID}";

        // Force TextMeshPro components to instantly rebuild their visual layouts
        _nameText.ForceMeshUpdate();
        _priceText.ForceMeshUpdate();
        _idText.ForceMeshUpdate();

        // Reveal the panel AFTER the text properties have been successfully updated
        _parent.gameObject.SetActive(true);

        Debug.Log($"[UI Popup] Displaying details for: {product.Data.productName}");
    }

    public void HidePopup()
    {
        _parent.gameObject.SetActive(false);
    }
}