using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelfStockingProducts : MonoBehaviour
{
    [Header("Product Configuration")]
    [Tooltip("The script will pick one random item from this list to fill an entire row.")]
    [SerializeField] private List<GameObject> _productPrefabsList;

    [Header("Slot References")]
    [Tooltip("This container should hold Row sub-folders, which in turn hold the individual slots.")]
    [SerializeField] private Transform _slotsContainer;

    private void Start()
    {
        StockShelf();
    }

    public void StockShelf()
    {
        if (_productPrefabsList == null || _productPrefabsList.Count == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] No product prefabs assigned to stock.");
            return;
        }

        if (_slotsContainer == null)
        {
            Debug.LogError($"[{gameObject.name}] Missing Slots Container reference!");
            return;
        }

        // 1. Loop through each Row container (Row_0, Row_1, Row_2, etc.)
        foreach (Transform row in _slotsContainer)
        {
            // ✅ RANDOM PER ROW: Pick one random item for this entire row before starting the slots loop
            int randomProductIndex = Random.Range(0, _productPrefabsList.Count);
            GameObject rowProduct = _productPrefabsList[randomProductIndex];

            Debug.Log($"[{gameObject.name}] Stocking Row '{row.name}' with random product: {rowProduct.name}");

            // 2. Loop through every individual slot inside this specific row
            foreach (Transform slot in row)
            {
                GameObject spawnedProduct = Instantiate(rowProduct, slot.position, slot.rotation);

                // Preserve local positioning architecture
                spawnedProduct.transform.SetParent(this.transform, false); // Keeps scale clean!
                spawnedProduct.transform.position = slot.position;
                spawnedProduct.transform.rotation = slot.rotation;
                spawnedProduct.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            }
        }
    }
}