using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewProductData", menuName = "Store/Product Data")]
public class ProductData : ScriptableObject
{
    public string productName;
    public float price;
    public string productID;
}
