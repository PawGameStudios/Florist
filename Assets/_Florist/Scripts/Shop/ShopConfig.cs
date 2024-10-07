using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopConfig", menuName = "Paw/Configs/Shop")]
public class ShopConfig : ScriptableObject
{
    [System.Serializable]
    public class ShopItemInfo
    {
        public string Name;
        public int Id;
        public long Price;
        public Sprite Icon;
    }

    public List<ShopItemInfo> FlowerItems;
    public List<ShopItemInfo> WrapperItems;
    public List<ShopItemInfo> RibbonItems;
    public List<ShopItemInfo> UpgradeItems;
    public List<ShopItemInfo> AccessoryItems;
    public List<ShopItemInfo> WallpaperItems;
    public List<ShopItemInfo> FloorItems;
    public List<ShopItemInfo> SignItems;
    public List<ShopItemInfo> CounterItems;
    public List<ShopItemInfo> SpeechBubbleItems;
}
