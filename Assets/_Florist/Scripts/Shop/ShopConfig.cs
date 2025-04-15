using System.Collections.Generic;
using Config;
using Sirenix.OdinInspector;
using UnityEngine;

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
    public List<ShopItemInfo> AccessoryItems;
    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
    public List<ShopItemInfo> WallpaperItems;
    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
    public List<ShopItemInfo> FloorItems;
    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
    public List<ShopItemInfo> SignItems;
    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
    public List<ShopItemInfo> CounterItems;
    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
    public List<ShopItemInfo> SpeechBubbleItems;
    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
    public List<ShopItemInfo> SpeechBubbleButtonItems;
    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
    public List<ShopItemInfo> OutsideDukkanItems;
    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
    public List<ShopItemInfo> DoorItems;
    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
    public List<ShopItemInfo> FlowerStandItems;
    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
    public List<ShopItemInfo> DecorItems;
    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
    public List<ShopItemInfo> PcItems;
    [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
    public List<ShopItemInfo> PosItems;

    public List<ShopItemInfo> GetItems(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Flower => FlowerItems,
            ItemType.Wrapper => WrapperItems,
            ItemType.Ribbon => RibbonItems,
            ItemType.Upgrade => UpgradeItems,
            ItemType.Accessory => AccessoryItems,
            ItemType.Wallpaper => WallpaperItems,
            ItemType.Floor => FloorItems,
            ItemType.Sign => SignItems,
            ItemType.Counter => CounterItems,
            ItemType.SpeechBubble => SpeechBubbleItems,
            ItemType.SpeechBubbleButton => SpeechBubbleButtonItems,
            ItemType.OutsideDukkan => OutsideDukkanItems,
            ItemType.Door => DoorItems,
            ItemType.FlowerStand => FlowerStandItems,
            ItemType.Decor => DecorItems,
            ItemType.Pc => PcItems,
            ItemType.Pos => PosItems,
            _ => null
        };
    }

}
