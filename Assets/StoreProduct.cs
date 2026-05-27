using UnityEngine;
using Oculus.Interaction;

public class StoreProduct : MonoBehaviour
{
    [Header("Product Configuration")]
    [SerializeField] private ProductData _data;

    public ProductData Data => _data;

    private void Awake()
    {
        // Moved to Awake to ensure data fields bind immediately upon dynamic instantiation
        ProductUIPopup popupUI = Object.FindFirstObjectByType<ProductUIPopup>(FindObjectsInactive.Include);

        print("running fine");
        print("PopUI is found at:" + popupUI.name);

        if (popupUI != null)
        {
            var eventWrapper = GetComponent<InteractableUnityEventWrapper>();

            if (eventWrapper != null)
            {
                print("event wrapper is not null");
                eventWrapper.WhenHover.RemoveAllListeners();
                eventWrapper.WhenUnhover.RemoveAllListeners();

                // Bind the data mapping
                eventWrapper.WhenHover.AddListener(() => popupUI.DisplayProductInfo(this));
                eventWrapper.WhenUnhover.AddListener(() => popupUI.HidePopup());
            }
        }
    }
}