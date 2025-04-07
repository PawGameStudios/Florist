using System;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace Config
{
    [Serializable]
    public class Recipe
    {
        public BouquetModel Bouquet;
    }

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
    public class FlowerInfo
    {
        [TableColumnWidth(80, Resizable = false)]
        [PreviewField(Alignment = ObjectFieldAlignment.Center)]
        public Sprite Sprite;

        [VerticalGroup("Info")]
        public FlowerType FlowerType;
        [VerticalGroup("Info")]
        public float Cost, Price;
    }

    [Serializable]
    public class RibbonInfo
    {
        [TableColumnWidth(80, Resizable = false)]
        [PreviewField(Alignment = ObjectFieldAlignment.Center)]
        public Sprite Sprite;

        [VerticalGroup("Info")]
        public RibbonType RibbonType;
        [VerticalGroup("Info")]
        public float Cost, Price;
    }

    [Serializable]
    public class WrappingPaperInfo
    {
        [TableColumnWidth(80, Resizable = false)]
        [PreviewField(Alignment = ObjectFieldAlignment.Center)]
        public Sprite Sprite;

        [VerticalGroup("Info")]
        public WrappingPaperType WrappingPaperType;
        [VerticalGroup("Info")]
        public float Cost, Price;
    }


    [CreateAssetMenu(fileName = "WorkshopConfig", menuName = "Paw/Configs/Workshop")]
    public class WorkshopConfig : SerializedScriptableObject
    {
        public SerializedDictionary<BouquetType, Recipe> BouquetRecipes;
        [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
        public List<FlowerInfo> FlowerInfo;
        [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
        public List<RibbonInfo> RibbonInfo;
        [TableList(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
        public List<WrappingPaperInfo> WrappingPaperInfo;

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

