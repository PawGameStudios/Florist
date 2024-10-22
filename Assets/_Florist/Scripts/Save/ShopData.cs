using AYellowpaper.SerializedCollections;

[System.Serializable]
public class ShopData
{
    public enum ItemState
    {
        Locked,
        Purchasable,
        Purchased,
        Selected
    }
    public SerializedDictionary<string, ItemState> Items;
    public bool IsInitialized;

    public ShopData()
    {
        Items = new();
        IsInitialized = false;
    }

    public void Initialize()
    {
        if (IsInitialized)
            return;

        // TODO: determine default states
        for (int i = 0; i < Configs.ShopConfig.FlowerItems.Count; i++)
            Items.Add(Configs.ShopConfig.FlowerItems[i].Id, ItemState.Purchasable);
        for (int i = 0; i < Configs.ShopConfig.WrapperItems.Count; i++)
            Items.Add(Configs.ShopConfig.WrapperItems[i].Id, ItemState.Purchasable);
        for (int i = 0; i < Configs.ShopConfig.RibbonItems.Count; i++)
            Items.Add(Configs.ShopConfig.RibbonItems[i].Id, ItemState.Purchasable);
        for (int i = 0; i < Configs.ShopConfig.UpgradeItems.Count; i++)
            Items.Add(Configs.ShopConfig.UpgradeItems[i].Id, ItemState.Purchasable);
        for (int i = 0; i < Configs.ShopConfig.AccessoryItems.Count; i++)
            Items.Add(Configs.ShopConfig.AccessoryItems[i].Id, ItemState.Purchasable);
        for (int i = 0; i < Configs.ShopConfig.WallpaperItems.Count; i++)
            Items.Add(Configs.ShopConfig.WallpaperItems[i].Id, ItemState.Purchasable);
        for (int i = 0; i < Configs.ShopConfig.FloorItems.Count; i++)
            Items.Add(Configs.ShopConfig.FloorItems[i].Id, ItemState.Purchasable);
        for (int i = 0; i < Configs.ShopConfig.SignItems.Count; i++)
            Items.Add(Configs.ShopConfig.SignItems[i].Id, ItemState.Purchasable);
        for (int i = 0; i < Configs.ShopConfig.CounterItems.Count; i++)
            Items.Add(Configs.ShopConfig.CounterItems[i].Id, ItemState.Purchasable);
        for (int i = 0; i < Configs.ShopConfig.SpeechBubbleItems.Count; i++)
            Items.Add(Configs.ShopConfig.SpeechBubbleItems[i].Id, ItemState.Purchasable);

        IsInitialized = true;
    }

    public void SetPurchasedState(string itemId)
    {
        Items[itemId] = ItemState.Purchased;
    }

    public void SetSelectedState(string itemId)
    {
        Items[itemId] = ItemState.Selected;
    }

    public void UnlockedItem(string itemId)
    {
        Items[itemId] = ItemState.Purchasable;
    }

    public bool HasItem(string itemId)
    {
        return Items[itemId] == ItemState.Purchased || Items[itemId] == ItemState.Selected;
    }

    public bool IsSelected(string itemId)
    {
        return Items[itemId] == ItemState.Selected;
    }

    public bool IsPurchased(string itemId)
    {
        return Items[itemId] == ItemState.Purchased;
    }

    public bool IsPurchasable(string itemId)
    {
        return Items[itemId] == ItemState.Purchasable;
    }

    public ItemState GetItemState(string itemId)
    {
        return Items[itemId];
    }
}
