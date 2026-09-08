using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using DecorationType = DecorationManager.DecorationType;

[CreateAssetMenu(fileName = "ShopConfig", menuName = "Paw/Configs/Shop")]
public class ShopConfig : SerializedScriptableObject
{
    public enum UpgradeEffectType { None, WrappingMachineLevelBonus, DayDurationMultiplier }
    [System.Serializable]
    public class ShopItemInfo
    {
        [TableColumnWidth(60, Resizable = false)]
        [PreviewField(Height = 80, Alignment = ObjectFieldAlignment.Center)]
        public Sprite Icon;

        [VerticalGroup("Info")]
        public string Name;

        [Tooltip("Optional full localization key; bypasses the legacy color_name naming convention.")]
        public string NameLocalizationKey;

        [VerticalGroup("Info")]
        public string Id;

        [VerticalGroup("Info")]
        public long Price;

        [Tooltip("Keep unfinished products visible, but prevent charging for an effect that is not implemented.")]
        public bool PurchaseDisabled;

        [VerticalGroup("State")]
        public bool IsSelectable;

        [VerticalGroup("State")]
        [LabelText("State")]
        public ShopData.ItemState DefaultItemState = ShopData.ItemState.Purchasable;

        [VerticalGroup("State")]
        [ShowIf("DefaultItemState", ShopData.ItemState.Locked)]
        public int UnlockDay = 0;

        [Tooltip("Only applies to UpgradeItems. None means no gameplay effect is connected.")]
        public UpgradeEffectType UpgradeEffect;
        [Min(0)] public float UpgradeValue = 1;
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

    public int GetMachineLevelBonus(ShopData ownership)
    {
        float bonus = 0;
        if (UpgradeItems != null)
            foreach (var item in UpgradeItems)
                if (IsOwnedUpgrade(item, ownership) && item.UpgradeEffect == UpgradeEffectType.WrappingMachineLevelBonus)
                    bonus += Mathf.Max(0, item.UpgradeValue);
        return Mathf.FloorToInt(bonus);
    }

    public float GetDayDurationMultiplier(ShopData ownership)
    {
        float multiplier = 1;
        if (UpgradeItems != null)
            foreach (var item in UpgradeItems)
                if (IsOwnedUpgrade(item, ownership) && item.UpgradeEffect == UpgradeEffectType.DayDurationMultiplier)
                    multiplier *= Mathf.Max(0.01f, item.UpgradeValue);
        return multiplier;
    }

    private static bool IsOwnedUpgrade(ShopItemInfo item, ShopData ownership)
    {
        return item != null && !string.IsNullOrEmpty(item.Id) && ownership?.Items != null &&
            ownership.Items.TryGetValue(item.Id, out var saved) && saved.ItemType == ItemType.Upgrade &&
            (saved.ItemState == ShopData.ItemState.Purchased || saved.ItemState == ShopData.ItemState.Selected);
    }

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
