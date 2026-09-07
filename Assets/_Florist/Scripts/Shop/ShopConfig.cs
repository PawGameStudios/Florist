using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using DecorationType = DecorationManager.DecorationType;

[CreateAssetMenu(fileName = "ShopConfig", menuName = "Paw/Configs/Shop")]
public class ShopConfig : SerializedScriptableObject
{
    [System.Serializable]
    public class ShopItemInfo
    {
        [TableColumnWidth(60, Resizable = false)]
        [PreviewField(Height = 80, Alignment = ObjectFieldAlignment.Center)]
        public Sprite Icon;

        [VerticalGroup("Info")]
        public string Name;

        [VerticalGroup("Info")]
        public string Id;

        [VerticalGroup("Info")]
        public long Price;

        [VerticalGroup("State")]
        public bool IsSelectable;

        [VerticalGroup("State")]
        [LabelText("State")]
        public ShopData.ItemState DefaultItemState = ShopData.ItemState.Purchasable;

        [VerticalGroup("State")]
        [ShowIf("DefaultItemState", ShopData.ItemState.Locked)]
        public int UnlockDay = 0;
    }

    [System.Serializable]
    public class DecorationItemInfo : ShopItemInfo
    {
        [PreviewField(Height = 80, Alignment = ObjectFieldAlignment.Center)]
        public Sprite DecorationSprite;
    }

    [System.Serializable]
    public class DecorationItemGroup
    {
        public DecorationType Type;
        public List<DecorationItemInfo> Items = new();
    }

    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
    public List<ShopItemInfo> FlowerItems;

    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
    public List<ShopItemInfo> WrapperItems;

    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
    public List<ShopItemInfo> RibbonItems;

    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
    public List<ShopItemInfo> UpgradeItems;

    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
    public List<ShopItemInfo> SpeechBubbleItems;

    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
    public List<ShopItemInfo> SpeechBubbleButtonItems;

    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 6)]
    public List<DecorationItemGroup> DecorationItems = new();

    public List<ShopItemInfo> GetItems(ItemType itemType, DecorationType decorationType = default)
    {
        return itemType switch
        {
            ItemType.Flower => FlowerItems,
            ItemType.Wrapper => WrapperItems,
            ItemType.Ribbon => RibbonItems,
            ItemType.Upgrade => UpgradeItems,
            ItemType.SpeechBubble => SpeechBubbleItems,
            ItemType.SpeechBubbleButton => SpeechBubbleButtonItems,
            ItemType.Decor => GetDecorationItemsAsShopItems(decorationType),
            _ => null
        };
    }

    public ShopItemInfo GetItemById(ItemType itemType, string id)
    {
        if (itemType == ItemType.Decor)
        {
            foreach (var category in DecorationItems)
            {
                foreach (DecorationItemInfo decoration in category.Items)
                {
                    if (decoration.Id == id)
                        return decoration;
                }
            }

            return null;
        }

        foreach (var item in GetItems(itemType))
        {
            if (item.Id == id)
            {
                return item;
            }
        }
        return null;
    }

    public DecorationItemGroup GetDecorationGroup(DecorationType decorationType)
    {
        foreach (DecorationItemGroup group in DecorationItems)
        {
            if (group.Type == decorationType)
                return group;
        }

        return null;
    }

    public List<DecorationItemInfo> GetDecorationItems(DecorationType decorationType)
    {
        return GetDecorationGroup(decorationType)?.Items;
    }

    private List<ShopItemInfo> GetDecorationItemsAsShopItems(DecorationType decorationType)
    {
        var items = GetDecorationItems(decorationType);
        if (items != null)
            return items.ConvertAll(item => (ShopItemInfo)item);

        return null;
    }

}
