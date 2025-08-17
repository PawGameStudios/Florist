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

    public Dictionary<DecorationType, List<ShopItemInfo>> DecorationItems;

    public List<ShopItemInfo> GetItems(ItemType itemType, DecorationType decorationType = DecorationType.None)
    {
        return itemType switch
        {
            ItemType.Flower => FlowerItems,
            ItemType.Wrapper => WrapperItems,
            ItemType.Ribbon => RibbonItems,
            ItemType.Upgrade => UpgradeItems,
            ItemType.SpeechBubble => SpeechBubbleItems,
            ItemType.SpeechBubbleButton => SpeechBubbleButtonItems,
            ItemType.Decor => DecorationItems.ContainsKey(decorationType) ? DecorationItems[decorationType] : null,
            _ => null
        };
    }

    public ShopItemInfo GetItemById(ItemType itemType, string id)
    {
        foreach (var item in GetItems(itemType))
        {
            if (item.Id == id)
            {
                return item;
            }
        }
        return null;
    }

}
