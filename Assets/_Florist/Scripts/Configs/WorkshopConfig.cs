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
        public int Cost, Price;
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

        [VerticalGroup("Info")]
        public Animator RibbonAnimator;
    }

    [Serializable]
    public class WrappingPaperInfo : WorkshopItem
    {
        [VerticalGroup("Info")]
        public WrappingPaperType WrappingPaperType;

        [VerticalGroup("Info")]
        public Sprite PaperSprite;

        [VerticalGroup("Info")]
        public Sprite PaperRollSprite;
        [VerticalGroup("Info")]
        public Sprite PaperClosedSprite;
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

        public Sprite GetWrappingPaperClosedSprite(WrappingPaperType wrappingPaperType)
        {
            Debug.LogError($"Wrapping paper type {wrappingPaperType}");
            foreach (var wrappingPaperInfo in WrappingPaperInfo)
            {
                if (wrappingPaperInfo.WrappingPaperType == wrappingPaperType)
                {
                    return wrappingPaperInfo.PaperClosedSprite;
                }
            }
            Debug.LogError($"Wrapping paper type {wrappingPaperType} not found in WrappingPaperInfo list.");
            return null;
        }

        public Sprite GetWrappingPaperSprite(WrappingPaperType wrappingPaperType)
        {
            foreach (var wrappingPaperInfo in WrappingPaperInfo)
            {
                if (wrappingPaperInfo.WrappingPaperType == wrappingPaperType)
                {
                    return wrappingPaperInfo.PaperSprite;
                }
            }
            Debug.LogError($"Wrapping paper type {wrappingPaperType} not found in WrappingPaperInfo list.");
            return null;
        }

        public int GetFlowerCost(FlowerType flowerType)
        {
            foreach (var flowerInfo in FlowerInfo)
            {
                if (flowerInfo.FlowerType == flowerType)
                {
                    return flowerInfo.Cost;
                }
            }
            Debug.LogError($"Flower type {flowerType} not found in FlowerInfo list.");
            return 0;
        }

        public int GetRibbonCost(RibbonType ribbonType)
        {
            foreach (var ribbonInfo in RibbonInfo)
            {
                if (ribbonInfo.RibbonType == ribbonType)
                {
                    return ribbonInfo.Cost;
                }
            }
            Debug.LogError($"Ribbon type {ribbonType} not found in RibbonInfo list.");
            return 0;
        }

        public int GetWrappingPaperCost(WrappingPaperType wrappingPaperType)
        {
            foreach (var wrappingPaperInfo in WrappingPaperInfo)
            {
                if (wrappingPaperInfo.WrappingPaperType == wrappingPaperType)
                {
                    return wrappingPaperInfo.Cost;
                }
            }
            Debug.LogError($"Wrapping paper type {wrappingPaperType} not found in WrappingPaperInfo list.");
            return 0;
        }

        public int GetFlowerPrice(FlowerType flowerType)
        {
            foreach (var flowerInfo in FlowerInfo)
            {
                if (flowerInfo.FlowerType == flowerType)
                {
                    return flowerInfo.Price;
                }
            }
            Debug.LogError($"Flower type {flowerType} not found in FlowerInfo list.");
            return 0;
        }

        public int GetRibbonPrice(RibbonType ribbonType)
        {
            foreach (var ribbonInfo in RibbonInfo)
            {
                if (ribbonInfo.RibbonType == ribbonType)
                {
                    return ribbonInfo.Price;
                }
            }
            Debug.LogError($"Ribbon type {ribbonType} not found in RibbonInfo list.");
            return 0;
        }

        public int GetWrappingPaperPrice(WrappingPaperType wrappingPaperType)
        {
            foreach (var wrappingPaperInfo in WrappingPaperInfo)
            {
                if (wrappingPaperInfo.WrappingPaperType == wrappingPaperType)
                {
                    return wrappingPaperInfo.Price;
                }
            }
            Debug.LogError($"Wrapping paper type {wrappingPaperType} not found in WrappingPaperInfo list.");
            return 0;
        }

        public int GetFlowerItemIndex(FlowerType flowerType, FlowerColor flowerColor)
        {
            for (int i = 0; i < FlowerInfo.Count; i++)
            {
                if (FlowerInfo[i].FlowerType == flowerType && FlowerInfo[i].Color == flowerColor)
                {
                    return i;
                }
            }
            return -1; // Not found
        }

        public int GetWrappingPaperItemIndex(WrappingPaperType wrappingPaperType)
        {
            for (int i = 0; i < WrappingPaperInfo.Count; i++)
            {
                if (WrappingPaperInfo[i].WrappingPaperType == wrappingPaperType)
                {
                    return i;
                }
            }
            return -1; // Not found
        }

        public int GetRibbonItemIndex(RibbonType ribbonType)
        {
            for (int i = 0; i < RibbonInfo.Count; i++)
            {
                if (RibbonInfo[i].RibbonType == ribbonType)
                {
                    return i;
                }
            }
            return -1; // Not found
        }

        public Animator GetRibbonAnimator(RibbonType ribbonType)
        {
            foreach (var ribbonInfo in RibbonInfo)
            {
                if (ribbonInfo.RibbonType == ribbonType)
                {
                    return ribbonInfo.RibbonAnimator;
                }
            }
            Debug.LogError($"Ribbon type {ribbonType} not found in RibbonInfo list.");
            return null;
        }



        public string GetFlowerId(FlowerType flowerType, FlowerColor flowerColor)
        {
            foreach (var flowerInfo in FlowerInfo)
            {
                if (flowerInfo.FlowerType == flowerType && flowerInfo.Color == flowerColor)
                {
                    return flowerInfo.Id;
                }
            }
            Debug.LogError($"Flower type {flowerType} with color {flowerColor} not found in FlowerInfo list.");
            return null;
        }

        public string GetRibbonId(RibbonType ribbonType)
        {
            foreach (var ribbonInfo in RibbonInfo)
            {
                if (ribbonInfo.RibbonType == ribbonType)
                {
                    return ribbonInfo.Id;
                }
            }
            Debug.LogError($"Ribbon type {ribbonType} not found in RibbonInfo list.");
            return null;
        }

        public string GetWrappingPaperId(WrappingPaperType wrappingPaperType)
        {
            foreach (var wrappingPaperInfo in WrappingPaperInfo)
            {
                if (wrappingPaperInfo.WrappingPaperType == wrappingPaperType)
                {
                    return wrappingPaperInfo.Id;
                }
            }
            Debug.LogError($"Wrapping paper type {wrappingPaperType} not found in WrappingPaperInfo list.");
            return null;
        }
    }
}

