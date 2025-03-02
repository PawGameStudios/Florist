using System;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using Sirenix.OdinInspector;
using Conversa.Runtime;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Paw/Configs/Level")]
public class LevelConfig : ScriptableObject
{
    public enum CustomerType
    {
        Regular, Random, Opponent
    }

    public enum FlowerType
    {
        Gypsum, Eucalyptus, Daisy, Rose
    }

    public enum BouquetType
    {
        None, Custom, Daisy, Rose,
    }

    public enum ConversationType
    {
        FirstCustomer, Type1
    }

    public enum Gender
    {
        None, Boy, Girl, Random
    }

    public enum SpecialEvents
    {
        None, InroduceRose, IntroduceDaisy, IntroduceBoy, IntroduceGirl, GiveReward, OpenMezat
    }

    [Serializable]
    public class CustomerInfo
    {
        public List<Sprite> Sprites;
        public string Name;
        public Gender Gender;
        public List<BouquetType> BouquetTypes;
        [Tooltip("Flower types are only used when BouquetType is Custom")]
        public List<FlowerType> FlowerTypes;
        public List<Conversation> InitialConversations;
        [Tooltip("When GoodbyeConversation is null, a conversation will be selected based on customer happiness")]
        public Conversation GoodbyeConversation = null;
        public int BouquetCount = 1;
        [Tooltip("How much money will be gained (in %) based on the cost of flower bouquet")]
        public Vector2 ProfitPercentage = new(10, 50);
        [Tooltip("Between 1 and 100")][Range(0, 100)] public float TipGiveRatio;
        public Vector2 TipPercentage = new(10, 50);

        public Conversation GetInitialConversation()
        {
            return InitialConversations[Random.Range(0, InitialConversations.Count)];
        }

        public Sprite GetRandomSprite()
        {
            return Sprites[Random.Range(0, Sprites.Count)];
        }

        public float GetProfitPercentage()
        {
            return Random.Range(ProfitPercentage.x, ProfitPercentage.y);
        }

        public float GetTipPercentage()
        {
            return Random.Range(TipPercentage.x, TipPercentage.y);
        }
    }

    [Serializable]
    public class DayInfo
    {
        public List<CustomerType> Customers;
        public SpecialEvents SpecialEvent;
        [HideIf(nameof(SpecialEvent), SpecialEvents.None)]
        public bool IsSpecialEventOnDayStart;
    }

    [Serializable]
    public class StaticDayInfo
    {
        public int DayStartTime;
        public int DayEndTime;
        [Tooltip("In minutes")] public float DayDuration;
    }

    [Serializable]
    public class GoodbyeConversationsInfo
    {
        public List<Conversation> Happy;
        public List<Conversation> WaitedLong;
        public List<Conversation> MissingFlowers;
        public List<Conversation> DifferentFlowers;
        public List<Conversation> MoreFlowers;
    }

    [Serializable]
    public class FlowerCount
    {
        public FlowerType FlowerType;
        public int Count;

        public FlowerCount(FlowerType flowerType, int count)
        {
            FlowerType = flowerType;
            Count = count;
        }
    }

    [Serializable]
    public class Recipe
    {
        public List<FlowerCount> Flowers;
    }

    public SerializedDictionary<CustomerType, CustomerInfo> Customers;
    public SerializedDictionary<FlowerType, float> FlowerPrices;
    public SerializedDictionary<FlowerType, float> FlowerCosts;
    public SerializedDictionary<BouquetType, Recipe> BouquetRecipes;
    public StaticDayInfo DayTimeInfo;
    public GoodbyeConversationsInfo GoodbyeConversations;
    public List<DayInfo> Days;

    public float GetFlowerCost(FlowerType flowerType)
    {
        return FlowerCosts[flowerType];
    }

    public float GetFlowerPrice(FlowerType flowerType)
    {
        return FlowerPrices[flowerType];
    }

}

