using System.Collections.Generic;

[System.Serializable]
public class ShopData
{
    public List<string> PurchasedItems;

    public ShopData()
    {
        PurchasedItems = new List<string>();
    }

    public void AddItem(string itemName)
    {
        PurchasedItems.Add(itemName);
    }

    public bool HasItem(string itemName)
    {
        return PurchasedItems.Contains(itemName);
    }
}
