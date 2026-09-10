namespace Florist.Merge
{
using UnityEngine;

[CreateAssetMenu(fileName = "ProductionItemData", menuName = "MergeGame/ProductionItemData", order = 2)]
public class ProductionItemData : ScriptableObject
{
    public string itemName;
    public string localizationKey;
    public string DisplayName => MergeLocalization.Name(localizationKey, itemName);
    public Sprite itemSprite;
    public string description;
    public int maxLevel = 1;
    public bool isUnlocked = true;

    // Ürün özellikleri
    public int baseValue = 100; // Temel değer
    [Min(0)] public int minimumCompletedOrders;
    public bool isRare = false; // Nadir ürün mü?
}

}
