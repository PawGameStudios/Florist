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

        public ItemData(ItemType itemType, int configIndex, ItemState itemState = ItemState.Purchasable)
        {
            ItemState = itemState;
            ItemType = itemType;
            ConfigIndex = configIndex;
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
        if (IsInitialized)
            return;

        // TODO: determine default states
        for (int i = 0; i < Configs.ShopConfig.FlowerItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.FlowerItems[i].Id, new ItemData(ItemType.Flower, i, Configs.ShopConfig.FlowerItems[i].DefaultItemState));
        }
        for (int i = 0; i < Configs.ShopConfig.WrapperItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.WrapperItems[i].Id, new ItemData(ItemType.Wrapper, i, Configs.ShopConfig.WrapperItems[i].DefaultItemState));
        }
        for (int i = 0; i < Configs.ShopConfig.RibbonItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.RibbonItems[i].Id, new ItemData(ItemType.Ribbon, i, Configs.ShopConfig.RibbonItems[i].DefaultItemState));
        }
        for (int i = 0; i < Configs.ShopConfig.UpgradeItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.UpgradeItems[i].Id, new ItemData(ItemType.Upgrade, i, Configs.ShopConfig.UpgradeItems[i].DefaultItemState));
        }
        for (int i = 0; i < Configs.ShopConfig.SpeechBubbleItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.SpeechBubbleItems[i].Id, new ItemData(ItemType.SpeechBubble, i, Configs.ShopConfig.SpeechBubbleItems[i].DefaultItemState));
        }
        for (int i = 0; i < Configs.ShopConfig.SpeechBubbleButtonItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.SpeechBubbleButtonItems[i].Id, new ItemData(ItemType.SpeechBubbleButton, i, Configs.ShopConfig.FlowerItems[i].DefaultItemState));
        }
        foreach (var decorationType in Configs.ShopConfig.DecorationItems.Keys)
        {
            for (int i = 0; i < Configs.ShopConfig.DecorationItems[decorationType].Count; i++)
            {
                Items.Add(Configs.ShopConfig.DecorationItems[decorationType][i].Id, new ItemData(ItemType.Decor, i, Configs.ShopConfig.DecorationItems[decorationType][i].DefaultItemState));
            }
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
        return Items[itemId].ItemState == ItemState.Selected;
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

    public Sprite GetSelectedItemSprite(ItemType itemType, DecorationType decorationType = DecorationType.None)
    {
        foreach (var item in Items)
        {
            if (item.Value.ItemType == itemType && item.Value.ItemState == ItemState.Selected)
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
                        return Configs.ShopConfig.DecorationItems[decorationType][item.Value.ConfigIndex].Icon;
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
}
