using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Conversa.Runtime;
using Random = UnityEngine.Random;
using AYellowpaper.SerializedCollections;

namespace Config
{
    public enum CustomerType
    {
        Regular, Random, Opponent
    }

    [Flags]
    public enum HappinessState
    {
        None = 0,
        Happy = 1 << 0,
        WaitedLong = 1 << 1,
        MissingFlowers = 1 << 2,
        DifferentOrder = 1 << 3,
        MoreFlowers = 1 << 4,
        SameOrder = 1 << 5,
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
    public class DayEvent
    {
        public bool IsEvent;
        [ShowIf("IsEvent")] public SpecialEvents EventType;
        [HideIf("IsEvent")] public CustomerType CustomerType;
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
        public bool ChoseOrderRandomly;
        [VerticalGroup("Flowers")]
        [ShowIf("ChoseOrderRandomly")] public int MaxOrderCount;
        [VerticalGroup("Flowers")]
        public List<Order> Orders;

        [VerticalGroup("Conversations")]
        public bool UseCustomConvo;
        [VerticalGroup("Conversations")]
        [ShowIf("UseCustomConvo")]
        public Conversation InitialConversation = null;
        [VerticalGroup("Conversations")]
        [ShowIf("UseCustomConvo")]
        public Conversation GoodbyeConversation = null;

        [VerticalGroup("Happiness")]
        public SerializedDictionary<HappinessState, int> HappinessChange;
        [VerticalGroup("Happiness")]
        public int HappinessTipLimit;
        [VerticalGroup("Happiness")]
        public Vector2 TipPercentage = new(10, 50);
        [VerticalGroup("Happiness")]
        public int AcceptableWaitTime = 60;
    }

    [Serializable]
    public class DayInfo
    {
        public List<DayEvent> Events;
    }

    [Serializable]
    public class DayTimeInfo
    {
        public int DayStartTime;
        public int DayEndTime;
        [Tooltip("In minutes")] public float DayDuration;
    }

    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Paw/Configs/Level")]
    public class LevelConfig : SerializedScriptableObject
    {
        [TableList(ShowIndexLabels = true)]
        public List<CustomerInfo> Customers;
        public DayTimeInfo DayTimeInfo;
        public Dictionary<HappinessState, Conversation> GoodbyeConversations;
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

        public Conversation GetGoodbyeConvo(CustomerInfo customerInfo, HappinessState happinessState)
        {
            foreach (var customer in Customers)
            {
                if (customer.CustomerType == customerInfo.CustomerType && customer.Name == customerInfo.Name && customer.UseCustomConvo)
                    return customer.GoodbyeConversation;
            }

            // TODO:
            return GoodbyeConversations[HappinessState.Happy];
        }
    }
}
