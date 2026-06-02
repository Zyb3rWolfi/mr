using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CheckoutManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private GameObject Grid;
    [SerializeField] private GameObject textPrefab;
    private BoxCollider _boxCollider;

    private float totalPrice = 0;

    void Start()
    {
        _boxCollider = GetComponent<BoxCollider>();

        // Initialize total display on game start
        if (priceText != null) priceText.text = "$0.00";
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. SAFEGUARD: Try to fetch the component first
        StoreProduct product = other.GetComponent<StoreProduct>();

        // If the entering object is NOT a grocery product, quietly ignore it!
        if (product == null) return;

        print("CHECKOUT: Detected item");
        ProductData data = product.Data;

        if (data == null)
        {
            Debug.LogError("[Checkout] Product has no valid Data file assigned!");
            return;
        }

        print($"CHECKOUT: item is: {data.productName}");

        // 2. GRID PARENT FIX: Pass 'Grid.transform' as the second parameter 
        // This forces the new text prefab to instantly snap inside your UI layout group list
        GameObject newtext = Instantiate(textPrefab, Grid.transform);

        newtext.GetComponent<TextMeshProUGUI>().text = $"{data.productName} : ${data.price:F2}";

        // 3. UI VISUAL UPDATE FIX: Add to math pool, then update your screen text
        totalPrice += data.price;
        if (priceText != null)
        {
            // :F2 formats the float number automatically to 2 decimal points like a real cash register
            priceText.text = $"Total: ${totalPrice:F2}";
        }
    }
}