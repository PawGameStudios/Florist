using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public enum ItemType
{
    Flower,
    Wrapper,
    Ribbon,
    Upgrade,
    Accessory,
    Wallpaper,
    Floor,
    Sign,
    Counter,
    SpeechBubble,
    SpeechBubbleButton,
    OutsideDukkan,
    Door,
    FlowerStand,
    Decor,
    Pc,
    Pos
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
            Items.Add(Configs.ShopConfig.WrapperItems[i].Id, new ItemData(ItemType.Wrapper, i));
        }
        for (int i = 0; i < Configs.ShopConfig.RibbonItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.RibbonItems[i].Id, new ItemData(ItemType.Ribbon, i));
        }
        for (int i = 0; i < Configs.ShopConfig.UpgradeItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.UpgradeItems[i].Id, new ItemData(ItemType.Upgrade, i));
        }
        for (int i = 0; i < Configs.ShopConfig.AccessoryItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.AccessoryItems[i].Id, new ItemData(ItemType.Accessory, i));
        }
        for (int i = 0; i < Configs.ShopConfig.WallpaperItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.WallpaperItems[i].Id, new ItemData(ItemType.Wallpaper, i));
        }
        for (int i = 0; i < Configs.ShopConfig.FloorItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.FloorItems[i].Id, new ItemData(ItemType.Floor, i));
        }
        for (int i = 0; i < Configs.ShopConfig.SignItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.SignItems[i].Id, new ItemData(ItemType.Sign, i));
        }
        for (int i = 0; i < Configs.ShopConfig.CounterItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.CounterItems[i].Id, new ItemData(ItemType.Counter, i));
        }
        for (int i = 0; i < Configs.ShopConfig.SpeechBubbleItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.SpeechBubbleItems[i].Id, new ItemData(ItemType.SpeechBubble, i));
        }
        for (int i = 0; i < Configs.ShopConfig.SpeechBubbleButtonItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.SpeechBubbleButtonItems[i].Id, new ItemData(ItemType.SpeechBubbleButton, i));
        }
        for (int i = 0; i < Configs.ShopConfig.OutsideDukkanItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.OutsideDukkanItems[i].Id, new ItemData(ItemType.OutsideDukkan, i));
        }
        for (int i = 0; i < Configs.ShopConfig.DoorItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.DoorItems[i].Id, new ItemData(ItemType.Door, i));
        }
        for (int i = 0; i < Configs.ShopConfig.FlowerStandItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.FlowerStandItems[i].Id, new ItemData(ItemType.FlowerStand, i));
        }
        for (int i = 0; i < Configs.ShopConfig.DecorItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.DecorItems[i].Id, new ItemData(ItemType.Decor, i));
        }
        for (int i = 0; i < Configs.ShopConfig.PcItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.PcItems[i].Id, new ItemData(ItemType.Pc, i));
        }
        for (int i = 0; i < Configs.ShopConfig.PosItems.Count; i++)
        {
            Items.Add(Configs.ShopConfig.PosItems[i].Id, new ItemData(ItemType.Pos, i));
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

    public void UnlockedItem(string itemId)
    {
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

    public ItemState GetItemState(string itemId)
    {
        return Items[itemId].ItemState;
    }

    public Sprite GetSelectedItemSprite(ItemType itemType)
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
                    case ItemType.Accessory:
                        return Configs.ShopConfig.AccessoryItems[item.Value.ConfigIndex].Icon;
                    case ItemType.Wallpaper:
                        return Configs.ShopConfig.WallpaperItems[item.Value.ConfigIndex].Icon;
                    case ItemType.Floor:
                        return Configs.ShopConfig.FloorItems[item.Value.ConfigIndex].Icon;
                    case ItemType.Sign:
                        return Configs.ShopConfig.SignItems[item.Value.ConfigIndex].Icon;
                    case ItemType.Counter:
                        return Configs.ShopConfig.CounterItems[item.Value.ConfigIndex].Icon;
                    case ItemType.SpeechBubble:
                        return Configs.ShopConfig.SpeechBubbleItems[item.Value.ConfigIndex].Icon;
                    case ItemType.SpeechBubbleButton:
                        return Configs.ShopConfig.SpeechBubbleButtonItems[item.Value.ConfigIndex].Icon;
                    case ItemType.OutsideDukkan:
                        return Configs.ShopConfig.OutsideDukkanItems[item.Value.ConfigIndex].Icon;
                    case ItemType.Door:
                        return Configs.ShopConfig.DoorItems[item.Value.ConfigIndex].Icon;
                    case ItemType.FlowerStand:
                        return Configs.ShopConfig.FlowerStandItems[item.Value.ConfigIndex].Icon;
                    case ItemType.Decor:
                        return Configs.ShopConfig.DecorItems[item.Value.ConfigIndex].Icon;
                    case ItemType.Pc:
                        return Configs.ShopConfig.PcItems[item.Value.ConfigIndex].Icon;
                    case ItemType.Pos:
                        return Configs.ShopConfig.PosItems[item.Value.ConfigIndex].Icon;
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
