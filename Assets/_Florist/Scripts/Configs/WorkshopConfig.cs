using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace Config
{
    public enum FlowerColor
    {
        None, Red, Pink, White, Yellow, Blue, Purple, Orange,
    }

    public enum FlowerType
    {
        Gypsum, Eucalyptus, Daisy, Rose, Anemone,
        Gladiolus, Carnation, Tulip,
    }

    public enum BouquetType
    {
        None, Custom, Daisy, Rose,
    }

    public enum WrappingPaperType
    {
        None, Newspaper, Rainbow, Sunset, Valentine, Grid, Red,
        Brown, Purple, Pink, PolkaDot, Yellow, Gray
    }

    public enum RibbonType
    {
        None, Yellow, PolkaDot, Pink, Purple, Red, Grid, Valentine, Gray
    }

    [Serializable]
    public class Recipe
    {
        public BouquetModel Bouquet;
    }

    [Serializable]
    public class Order
    {
        public BouquetType BouquetType;
        public RibbonType RibbonType;
        public WrappingPaperType WrappingPaperType;
        [ShowIf("BouquetType", BouquetType.Custom)] public List<BouquetFlowerInfo> CustomFlowers;
    }

    [Serializable]
    public class WorkshopItem
    {
        [HorizontalGroup("Icons", order: 0)]
        [TableColumnWidth(150, Resizable = false)]
        [PreviewField(Height = 80, Alignment = ObjectFieldAlignment.Center)]
        [HideLabel]
        public Sprite Sprite;

        [VerticalGroup("Info")]
        public string Id;
        [VerticalGroup("Info")]
        public float Cost, Price;
    }

    [Serializable]
    public class FlowerInfo : WorkshopItem
    {
        [HorizontalGroup("Icons", order: 0)]
        [PreviewField(Height = 80, Alignment = ObjectFieldAlignment.Center)]
        [HideLabel]
        public Sprite FlowerInBoxImage;

        [VerticalGroup("Info")]
        public string Name;
        [VerticalGroup("Info")]
        public FlowerType FlowerType;

        [VerticalGroup("Info")]
        public FlowerColor Color;

        [VerticalGroup("Info")]
        public Flower Prefab;
    }

    [Serializable]
    public class RibbonInfo : WorkshopItem
    {
        [VerticalGroup("Info")]
        public RibbonType RibbonType;
    }

    [Serializable]
    public class WrappingPaperInfo : WorkshopItem
    {
        [VerticalGroup("Info")]
        public WrappingPaperType WrappingPaperType;

        [VerticalGroup("Info")]
        public GameObject PaperOpenAnimation;
    }

    [Serializable]
    public class MachineInfo
    {
        public float BaseDuration;
        public float DurationGainPerLevel;

        public float CalculateDuration(int level)
        {
            return Mathf.Max(.1f, BaseDuration - DurationGainPerLevel * (level - 1));
        }
    }

    [CreateAssetMenu(fileName = "WorkshopConfig", menuName = "Paw/Configs/Workshop")]
    public class WorkshopConfig : SerializedScriptableObject
    {
        public Dictionary<BouquetType, Recipe> BouquetRecipes;

        [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
        public List<FlowerInfo> FlowerInfo;

        [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
        public List<RibbonInfo> RibbonInfo;

        [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
        public List<WrappingPaperInfo> WrappingPaperInfo;

        public MachineInfo MachineInfo;

        public Sprite GetFlowerSprite(FlowerType flowerType, FlowerColor flowerColor)
        {
            foreach (var flowerInfo in FlowerInfo)
            {
                if (flowerInfo.FlowerType == flowerType && flowerInfo.Color == flowerColor)
                {
                    return flowerInfo.Sprite;
                }
            }
            Debug.LogError($"Flower type {flowerType} with color {flowerColor} not found in FlowerInfo list.");
            return null;
        }

        public Sprite GetRibbonSprite(RibbonType ribbonType)
        {
            foreach (var ribbonInfo in RibbonInfo)
            {
                if (ribbonInfo.RibbonType == ribbonType)
                {
                    return ribbonInfo.Sprite;
                }
            }
            Debug.LogError($"Ribbon type {ribbonType} not found in RibbonInfo list.");
            return null;
        }

        public Sprite GetWrappingPaperSprite(WrappingPaperType wrappingPaperType)
        {
            foreach (var wrappingPaperInfo in WrappingPaperInfo)
            {
                if (wrappingPaperInfo.WrappingPaperType == wrappingPaperType)
                {
                    return wrappingPaperInfo.Sprite;
                }
            }
            Debug.LogError($"Wrapping paper type {wrappingPaperType} not found in WrappingPaperInfo list.");
            return null;
        }

        public float GetFlowerCost(FlowerType flowerType)
        {
            foreach (var flowerInfo in FlowerInfo)
            {
                if (flowerInfo.FlowerType == flowerType)
                {
                    return flowerInfo.Cost;
                }
            }
            Debug.LogError($"Flower type {flowerType} not found in FlowerInfo list.");
            return -1;
        }

        public float GetRibbonCost(RibbonType ribbonType)
        {
            foreach (var ribbonInfo in RibbonInfo)
            {
                if (ribbonInfo.RibbonType == ribbonType)
                {
                    return ribbonInfo.Cost;
                }
            }
            Debug.LogError($"Ribbon type {ribbonType} not found in RibbonInfo list.");
            return -1;
        }

        public float GetWrappingPaperCost(WrappingPaperType wrappingPaperType)
        {
            foreach (var wrappingPaperInfo in WrappingPaperInfo)
            {
                if (wrappingPaperInfo.WrappingPaperType == wrappingPaperType)
                {
                    return wrappingPaperInfo.Cost;
                }
            }
            Debug.LogError($"Wrapping paper type {wrappingPaperType} not found in WrappingPaperInfo list.");
            return -1;
        }

        public float GetFlowerPrice(FlowerType flowerType)
        {
            foreach (var flowerInfo in FlowerInfo)
            {
                if (flowerInfo.FlowerType == flowerType)
                {
                    return flowerInfo.Price;
                }
            }
            Debug.LogError($"Flower type {flowerType} not found in FlowerInfo list.");
            return -1;
        }

        public float GetRibbonPrice(RibbonType ribbonType)
        {
            foreach (var ribbonInfo in RibbonInfo)
            {
                if (ribbonInfo.RibbonType == ribbonType)
                {
                    return ribbonInfo.Price;
                }
            }
            Debug.LogError($"Ribbon type {ribbonType} not found in RibbonInfo list.");
            return -1;
        }

        public float GetWrappingPaperPrice(WrappingPaperType wrappingPaperType)
        {
            foreach (var wrappingPaperInfo in WrappingPaperInfo)
            {
                if (wrappingPaperInfo.WrappingPaperType == wrappingPaperType)
                {
                    return wrappingPaperInfo.Price;
                }
            }
            Debug.LogError($"Wrapping paper type {wrappingPaperType} not found in WrappingPaperInfo list.");
            return -1;
        }
    }
}

