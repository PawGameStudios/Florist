using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Conversa.Runtime;
using Random = UnityEngine.Random;

namespace Config
{
    public enum CustomerType
    {
        Regular, Random, Opponent
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
        [TableColumnWidth(100, Resizable = false)]
        [PreviewField(Height = 150, Alignment = ObjectFieldAlignment.Center)]
        public Sprite Sprite;

        [VerticalGroup("Info")]
        public CustomerType CustomerType;
        [VerticalGroup("Info")]
        public string Name;
        [VerticalGroup("Info")]
        public Gender Gender;

        [VerticalGroup("Flowers")]
        public List<BouquetType> BouquetTypes;
        [VerticalGroup("Flowers")]
        [ShowIf("@this.BouquetTypes.Contains(BouquetType.Custom)")]
        public List<FlowerType> FlowerTypes;

        [VerticalGroup("Conversations")]
        public bool UseCustomConvo;
        [VerticalGroup("Conversations")]
        [ShowIf("UseCustomConvo")]
        public Conversation InitialConversation = null;
        [VerticalGroup("Conversations")]
        [ShowIf("UseCustomConvo")]
        public Conversation GoodbyeConversation = null;

        [VerticalGroup("Money")]
        [Tooltip("Between 1 and 100")]
        [Range(0, 100)]
        public float TipGiveRatio;

        [VerticalGroup("Money")]
        public Vector2 TipPercentage = new(10, 50);

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
    public class DayTimeInfo
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

    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Paw/Configs/Level")]
    public class LevelConfig : SerializedScriptableObject
    {
        [TableList(ShowIndexLabels = true)]
        public List<CustomerInfo> Customers;
        public DayTimeInfo DayTimeInfo;
        public GoodbyeConversationsInfo GoodbyeConversations;
        public List<Conversation> InitialConversations;
        public List<DayInfo> Days;

        public CustomerInfo GetCustomer(CustomerType customerType)
        {
            List<CustomerInfo> customerList = new();
            foreach (var customer in Customers)
            {
                if (customer.CustomerType == customerType)
                {
                    customerList.Add(customer);
                }
            }
            return customerList[Random.Range(0, customerList.Count)];
        }

        public Conversation GetInitialConvo(CustomerInfo customerInfo)
        {
            foreach (var customer in Customers)
            {
                if (customer.CustomerType == customerInfo.CustomerType && customer.Name == customerInfo.Name && customer.UseCustomConvo)
                {
                    return customer.InitialConversation;
                }
            }

            return InitialConversations[Random.Range(0, InitialConversations.Count)];
        }
    }
}
