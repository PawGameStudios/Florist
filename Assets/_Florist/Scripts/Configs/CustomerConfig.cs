using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Conversa.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Config
{
    public enum OrderHappinessType
    {
        Default, Custom
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
        [LabelWidth(150)] public bool ChoseOrderRandomly;
        [VerticalGroup("Flowers")]
        [ShowIf("ChoseOrderRandomly")] public int MaxOrderCount;
        [VerticalGroup("Flowers")]
        public List<Order> Orders;

        [VerticalGroup("Conversations")]
        [LabelWidth(150)] public bool CustomInitConvo;
        [VerticalGroup("Conversations")]
        [LabelWidth(150)] public bool CustomGoodbyeConvo;
        [VerticalGroup("Conversations")]
        [ShowIf("CustomInitConvo")]
        public Conversation InitialConversation = null;
        [VerticalGroup("Conversations")]
        [ShowIf("CustomGoodbyeConvo")]
        public SerializedDictionary<HappinessState, Conversation> GoodbyeConversations;

        [VerticalGroup("Happiness")]
        public OrderHappinessType OrderHappinessType = OrderHappinessType.Default;
        [VerticalGroup("Happiness")]
        [ShowIf("OrderHappinessType", OrderHappinessType.Custom)] public OrderHappiness OrderHappiness;
    }

    [Serializable]
    public class OrderHappiness
    {
        [VerticalGroup("Happiness")]
        public SerializedDictionary<HappinessState, int> HappinessChange;
        [VerticalGroup("Happiness")]
        public int HappinessTipLimit;
        [VerticalGroup("Happiness")]
        public Vector2 TipPercentage = new(10, 50);
        [VerticalGroup("Happiness")]
        public int AcceptableWaitTime = 60;
    }

    [CreateAssetMenu(fileName = "CustomerConfig", menuName = "Paw/Configs/Customer")]
    public class CustomerConfig : SerializedScriptableObject
    {
        [TableList(ShowIndexLabels = true)]
        public List<CustomerInfo> Customers;
        public Dictionary<HappinessState, Conversation> GoodbyeConversations;
        public Conversation DefaultGoodbyeConversation;
        public List<Conversation> InitialConversations;
        public OrderHappiness DefaultOrderHappiness;

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

        public CustomerInfo GetCustomerByName(string name)
        {
            foreach (var customer in Customers)
            {
                if (customer.Name == name)
                {
                    return customer;
                }
            }
            return null;
        }

        public Conversation GetInitialConvo(CustomerInfo customerInfo)
        {
            foreach (var customer in Customers)
            {
                if (customer.CustomerType == customerInfo.CustomerType && customer.Name == customerInfo.Name && customer.CustomInitConvo)
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
                if (customer.CustomerType == customerInfo.CustomerType && customer.Name == customerInfo.Name && customer.CustomGoodbyeConvo)
                {
                    if (customer.GoodbyeConversations == null || customer.GoodbyeConversations.Count == 0)
                        return null;

                    if (customer.GoodbyeConversations.ContainsKey(happinessState))
                        return customer.GoodbyeConversations[happinessState];
                }
            }

            // If no specific conversation for this happiness state, return a default one
            if (GoodbyeConversations.ContainsKey(happinessState))
            {
                return GoodbyeConversations[happinessState];
            }

            return DefaultGoodbyeConversation;
        }

        public Sprite GetCustomerSprite(string name)
        {
            foreach (var customer in Customers)
            {
                if (customer.Name == name)
                {
                    return customer.Sprite;
                }
            }
            return null;
        }

    }
}
