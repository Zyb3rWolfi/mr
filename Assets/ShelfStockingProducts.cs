using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelfStockingProducts : MonoBehaviour
{
    [Header("Product Configuration")]
    [Tooltip("Each element here will correspond to a row. Index 0 = Row 0, Index 1 = Row 1, etc.")]
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

        int rowIndex = 0;

        // 1. Loop through each Row container (Row_0, Row_1, Row_2, etc.)
        foreach (Transform row in _slotsContainer)
        {
            // Safeguard: Assign a specific product for this row based on the row index.
            // If you have more rows than assigned prefabs, loop back around using modulo (%)
            int productToUseIndex = rowIndex % _productPrefabsList.Count;
            GameObject rowProduct = _productPrefabsList[productToUseIndex];

            Debug.Log($"[{gameObject.name}] Stocking Row '{row.name}' with product: {rowProduct.name}");

            // 2. Loop through every individual slot inside this specific row
            foreach (Transform slot in row)
            {
                // Spawn the specific row product instead of a random one
                GameObject spawnedProduct = Instantiate(rowProduct, slot.position, slot.rotation);

                // Preserve local positioning architecture
                spawnedProduct.transform.SetParent(this.transform, false); // Keeps scale clean!
                spawnedProduct.transform.position = slot.position;
                spawnedProduct.transform.rotation = slot.rotation;
                spawnedProduct.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

                Debug.Log($"[{gameObject.name}] Stocked {spawnedProduct.name} onto slot {slot.name} in {row.name}");
            }

            // Move to the next product prefab for the next row loop iteration
            rowIndex++;
        }
    }
}