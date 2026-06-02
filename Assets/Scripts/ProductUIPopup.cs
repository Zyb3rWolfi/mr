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
    [SerializeField] private float _frontOffset = 0.2f;

    private Transform _mainCameraTransform;
    private StoreProduct _trackedProduct; // Keeps track of the active product mesh reference

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
        // Only run calculations if the popup panel is actively being displayed
        if (_parent.activeSelf)
        {
            // 1. DYNAMIC TRACKING FIX: If we have an active item reference, follow its position live!
            if (_trackedProduct != null)
            {
                UpdatePopupPosition(_trackedProduct);
            }

            // 2. Keep facing the player's Quest eyes smoothly
            if (_facePlayer && _mainCameraTransform != null)
            {
                transform.LookAt(transform.position + _mainCameraTransform.rotation * Vector3.forward,
                                 _mainCameraTransform.rotation * Vector3.up);
            }
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

        // Cache the product reference so the Update loop can track it over time
        _trackedProduct = product;

        // Explicit Text Assignments
        _nameText.text = product.Data.productName;
        _priceText.text = $"${product.Data.price:F2}";
        _idText.text = $"ID: {product.Data.productID}";

        // Force TextMeshPro components to instantly rebuild their visual layouts
        _nameText.ForceMeshUpdate();
        _priceText.ForceMeshUpdate();
        _idText.ForceMeshUpdate();

        // Calculate initial position before revealing to prevent a 1-frame visual pop
        UpdatePopupPosition(product);

        // Reveal the panel AFTER properties are tracked and loaded
        _parent.gameObject.SetActive(true);

        Debug.Log($"[UI Popup] Displaying details for: {product.Data.productName}");
    }

    /// <summary>
    /// Calculates and applies the relative front-and-center spatial coordinates.
    /// </summary>
    private void UpdatePopupPosition(StoreProduct product)
    {
        // 1. Start at the exact live world center position of the grabbed product item
        Vector3 targetPosition = product.transform.position;

        // 2. Add the vertical height adjustment relative to world space
        targetPosition.y += _heightOffset;

        // 3. Calculate the direction line pointing straight from the moving product to your headset lenses
        if (_mainCameraTransform != null)
        {
            Vector3 dirToPlayer = (_mainCameraTransform.position - product.transform.position);
            dirToPlayer.y = 0; // Flatten on the Y axis so the popup doesn't tilt or flip up if you raise the item high
            dirToPlayer.Normalize();

            // Push the popup forward along this line directly toward you
            targetPosition += dirToPlayer * _frontOffset;
        }

        // Apply the coordinates directly to this UI Canvas transform root
        transform.position = targetPosition;
    }

    public void HidePopup()
    {
        _parent.gameObject.SetActive(false);
        _trackedProduct = null; // Clear out our cached pointer to stop unnecessary calculations
    }
}