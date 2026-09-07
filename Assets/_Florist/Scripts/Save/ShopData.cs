using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using DecorationType = DecorationManager.DecorationType;

public enum ItemType
{
    Flower,
    Wrapper,
    Ribbon,
    Upgrade,
    SpeechBubble,
    SpeechBubbleButton,
    Decor,
}

[Serializable]
public class ShopData
{
    public enum ItemState
    {
        Locked,
        Purchasable,
        Purchased,
        Selected
    }

    [Serializable]
    public class ItemData
    {
        public ItemState ItemState;
        public ItemType ItemType;
        public int ConfigIndex;
        public DecorationType DecorationType;

        public ItemData(ItemType itemType, int configIndex, ItemState itemState = ItemState.Purchasable,
            DecorationType decorationType = default)
        {
            ItemState = itemState;
            ItemType = itemType;
            ConfigIndex = configIndex;
            DecorationType = decorationType;
        }
    }

    public SerializedDictionary<string, ItemData> Items;
    public bool IsInitialized;

    public ShopData()
    {
        Items = new();
        IsInitialized = false;
    }

    public void Initialize()
    {
        Items ??= new SerializedDictionary<string, ItemData>();

        // TODO: determine default states
        for (int i = 0; i < Configs.ShopConfig.FlowerItems.Count; i++)
        {
            AddMissingItem(Configs.ShopConfig.FlowerItems[i].Id, new ItemData(ItemType.Flower, i, Configs.ShopConfig.FlowerItems[i].DefaultItemState));
        }
        for (int i = 0; i < Configs.ShopConfig.WrapperItems.Count; i++)
        {
            AddMissingItem(Configs.ShopConfig.WrapperItems[i].Id, new ItemData(ItemType.Wrapper, i, Configs.ShopConfig.WrapperItems[i].DefaultItemState));
        }
        for (int i = 0; i < Configs.ShopConfig.RibbonItems.Count; i++)
        {
            AddMissingItem(Configs.ShopConfig.RibbonItems[i].Id, new ItemData(ItemType.Ribbon, i, Configs.ShopConfig.RibbonItems[i].DefaultItemState));
        }
        for (int i = 0; i < Configs.ShopConfig.UpgradeItems.Count; i++)
        {
            AddMissingItem(Configs.ShopConfig.UpgradeItems[i].Id, new ItemData(ItemType.Upgrade, i, Configs.ShopConfig.UpgradeItems[i].DefaultItemState));
        }
        for (int i = 0; i < Configs.ShopConfig.SpeechBubbleItems.Count; i++)
        {
            AddMissingItem(Configs.ShopConfig.SpeechBubbleItems[i].Id, new ItemData(ItemType.SpeechBubble, i, Configs.ShopConfig.SpeechBubbleItems[i].DefaultItemState));
        }
        for (int i = 0; i < Configs.ShopConfig.SpeechBubbleButtonItems.Count; i++)
        {
            AddMissingItem(Configs.ShopConfig.SpeechBubbleButtonItems[i].Id, new ItemData(ItemType.SpeechBubbleButton, i, Configs.ShopConfig.SpeechBubbleButtonItems[i].DefaultItemState));
        }

        foreach (var category in Configs.ShopConfig.DecorationItems)
        {
            string savedSelectionId = GetSelectedDecorationId(category);

            for (int i = 0; i < category.Items.Count; i++)
            {
                var item = category.Items[i];
                ItemState initialState = item.DefaultItemState;
                if (savedSelectionId != null && initialState == ItemState.Selected && item.Id != savedSelectionId)
                    initialState = ItemState.Purchased;

                AddMissingItem(item.Id, new ItemData(ItemType.Decor, i, initialState, category.Type));
            }

            NormalizeDecorationSelection(category, savedSelectionId);
        }

        IsInitialized = true;
    }

    public void SetPurchasedState(string itemId)
    {
        Items[itemId].ItemState = ItemState.Purchased;
    }

    public void SetSelectedState(string itemId)
    {
        Items[itemId].ItemState = ItemState.Selected;
    }

    public void SelectDecoration(DecorationType decorationType, string itemId)
    {
        foreach (var item in Items)
        {
            if (item.Value.ItemType != ItemType.Decor || item.Value.DecorationType != decorationType)
                continue;

            if (item.Key == itemId)
                item.Value.ItemState = ItemState.Selected;
            else if (item.Value.ItemState == ItemState.Selected)
                item.Value.ItemState = ItemState.Purchased;
        }
    }

    public void UnlockItem(string itemId)
    {
        Debug.LogWarning($"Unlocking item: {itemId}");
        Items[itemId].ItemState = ItemState.Purchasable;
    }

    public bool HasItem(string itemId)
    {
        return Items[itemId].ItemState == ItemState.Purchased || Items[itemId].ItemState == ItemState.Selected;
    }

    public bool IsSelected(string itemId)
    {
        return Items.ContainsKey(itemId) && Items[itemId].ItemState == ItemState.Selected;
    }

    public bool IsPurchased(string itemId)
    {
        return Items[itemId].ItemState == ItemState.Purchased;
    }

    public bool IsPurchasable(string itemId)
    {
        return Items[itemId].ItemState == ItemState.Purchasable;
    }

    public ItemState GetItemState(string itemId, int unlockDay)
    {
        if (!Items.ContainsKey(itemId))
        {
            Debug.LogError($"Item with ID {itemId} does not exist in ShopData.");
            return ItemState.Locked;
        }

        ItemData itemData = Items[itemId];

        // If the item is locked, check if it should be unlocked based on the day
        if (itemData.ItemState == ItemState.Locked && SaveSystem.Inst.GeneralData.CurrentDayIndex >= unlockDay)
        {
            itemData.ItemState = ItemState.Purchasable;
            return ItemState.Purchasable;
        }
        return Items[itemId].ItemState;
    }

    public Sprite GetSelectedItemSprite(ItemType itemType, DecorationType decorationType = default)
    {
        foreach (var item in Items)
        {
            if (item.Value.ItemType == itemType && item.Value.ItemState == ItemState.Selected &&
                (itemType != ItemType.Decor || item.Value.DecorationType == decorationType))
            {
                switch (itemType)
                {
                    case ItemType.Flower:
                        return Configs.ShopConfig.FlowerItems[item.Value.ConfigIndex].Icon;
                    case ItemType.Wrapper:
                        return Configs.ShopConfig.WrapperItems[item.Value.ConfigIndex].Icon;
                    case ItemType.Ribbon:
                        return Configs.ShopConfig.RibbonItems[item.Value.ConfigIndex].Icon;
                    case ItemType.Upgrade:
                        return Configs.ShopConfig.UpgradeItems[item.Value.ConfigIndex].Icon;
                    case ItemType.SpeechBubble:
                        return Configs.ShopConfig.SpeechBubbleItems[item.Value.ConfigIndex].Icon;
                    case ItemType.SpeechBubbleButton:
                        return Configs.ShopConfig.SpeechBubbleButtonItems[item.Value.ConfigIndex].Icon;
                    case ItemType.Decor:
                        var decorationItems = Configs.ShopConfig.GetDecorationItems(decorationType);
                        if (decorationItems != null && item.Value.ConfigIndex < decorationItems.Count)
                            return decorationItems[item.Value.ConfigIndex].DecorationSprite;
                        break;
                }
            }
        }

        return null;
    }

    public List<int> GetPurchasedItems(ItemType itemType)
    {
        List<int> purchasedItems = new();
        foreach (var item in Items)
        {
            if (item.Value.ItemType == itemType && item.Value.ItemState == ItemState.Purchased)
            {
                purchasedItems.Add(item.Value.ConfigIndex);
            }
        }

        return purchasedItems;
    }

    private void AddMissingItem(string itemId, ItemData itemData)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            Debug.LogError("Shop item has an empty ID and could not be added to save data.");
            return;
        }

        if (!Items.ContainsKey(itemId))
        {
            Items.Add(itemId, itemData);
            return;
        }

        // Keep player state, but refresh config metadata for migrated saves.
        Items[itemId].ItemType = itemData.ItemType;
        Items[itemId].ConfigIndex = itemData.ConfigIndex;
        Items[itemId].DecorationType = itemData.DecorationType;
    }

    private string GetSelectedDecorationId(ShopConfig.DecorationItemGroup group)
    {
        foreach (ShopConfig.DecorationItemInfo item in group.Items)
        {
            if (Items.ContainsKey(item.Id) && Items[item.Id].ItemState == ItemState.Selected)
                return item.Id;
        }

        return null;
    }

    private void NormalizeDecorationSelection(ShopConfig.DecorationItemGroup group, string preferredSelectionId)
    {
        string selectedId = preferredSelectionId;

        if (selectedId == null)
        {
            foreach (ShopConfig.DecorationItemInfo item in group.Items)
            {
                if (Items[item.Id].ItemState != ItemState.Selected)
                    continue;

                selectedId = item.Id;
                break;
            }
        }

        foreach (ShopConfig.DecorationItemInfo item in group.Items)
        {
            if (Items[item.Id].ItemState == ItemState.Selected && item.Id != selectedId)
                Items[item.Id].ItemState = ItemState.Purchased;
        }
    }
}
