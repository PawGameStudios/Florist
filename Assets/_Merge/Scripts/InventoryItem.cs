namespace Florist.Merge
{
using UnityEngine;
using System;

[System.Serializable]
public class InventoryItem
{
    public ItemData itemData;
    public ProductionItemData productionItemData;
    public int level;
    public int count;
    public DateTime collectedTime;

    // Normal ItemData için constructor
    public InventoryItem(ItemData data, int itemLevel, int itemCount = 1)
    {
        itemData = data;
        productionItemData = null;
        level = itemLevel;
        count = itemCount;
        collectedTime = DateTime.Now;
    }

    // ProductionItemData için constructor
    public InventoryItem(ProductionItemData data, int itemLevel, int itemCount = 1)
    {
        itemData = null;
        productionItemData = data;
        level = itemLevel;
        count = itemCount;
        collectedTime = DateTime.Now;
    }

    public void AddToStack(int amount)
    {
        // Negatif değer ekleme kontrolü
        if (amount < 0)
        {
            Debug.LogWarning($"AddToStack: Attempting to add negative amount {amount}");
            return;
        }

        count += amount;
    }

    // Hangi tür item olduğunu kontrol et
    public bool IsProductionItem => productionItemData != null;
    public bool IsRegularItem => itemData != null;

    // Item adını al
    public string GetItemName()
    {
        if (IsProductionItem)
            return productionItemData.itemName;
        else if (IsRegularItem)
            return itemData.ItemType.ToString();
        return "Unknown Item";
    }

    // Item sprite'ını al
    public Sprite GetItemSprite()
    {
        if (IsProductionItem)
            return productionItemData.itemSprite;
        else if (IsRegularItem && level > 0 && level <= itemData.Sprites.Length)
            return itemData.Sprites[level - 1];
        return null;
    }
}

}
