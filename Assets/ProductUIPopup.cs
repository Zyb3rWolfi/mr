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
    [SerializeField] private float _rightSideOffset = 0.25f;
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
        if (_facePlayer && _mainCameraTransform != null && gameObject.activeSelf)
        {
            // Keep facing the player's Quest 3S eyes
            transform.LookAt(transform.position + _mainCameraTransform.rotation * Vector3.forward,
                             _mainCameraTransform.rotation * Vector3.up);
        }
    }

    /// <summary>
    /// Updates the visual text elements and activates the canvas panel.
    /// </summary>
    public void DisplayProductInfo(StoreProduct product)
    {
        // 1. Critical Safeguard: If the product script or its ScriptableObject is empty, stop here
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
        Vector3 targetPosition = product.transform.position + (product.transform.right * _rightSideOffset);
        targetPosition.y += 0.05f;

        // 2. Apply the calculated position to this Canvas transform
        transform.position = targetPosition;
        // 2. Explicit Text Assignments
        _nameText.text = product.Data.productName;
        _priceText.text = $"${product.Data.price:F2}";
        _idText.text = $"ID: {product.Data.productID}";

        // 3. Force TextMeshPro components to instantly rebuild their visual layouts
        _nameText.ForceMeshUpdate();
        _priceText.ForceMeshUpdate();
        _idText.ForceMeshUpdate();

        // 4. Reveal the panel AFTER the text properties have been successfully updated
        _parent.gameObject.SetActive(true);

        Debug.Log($"[UI Popup] Displaying details for: {product.Data.productName}");
    }

    public void HidePopup()
    {
        _parent.gameObject.SetActive(false);
    }
}