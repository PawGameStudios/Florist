using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Paw/Configs/Level")]
public class LevelConfig : ScriptableObject
{
    public enum CustomerType
    {
        Regular, Random, Opponent
    }

    public enum FlowerType
    {
        Daisy, Rose
    }

    public enum ConversationType
    {
        FirstCustomer, Type1
    }

    public enum Gender
    {
        None, Boy, Girl
    }

    public enum SpecialEvents
    {
        None, InroduceRose, IntroduceDaisy, IntroduceBoy, IntroduceGirl, GiveReward, OpenMezat
    }

    [Serializable]
    public class Conversation
    {
        public string[] Sentences;
    }

    [Serializable]
    public class Customer
    {
        public string Name;
        public Sprite Sprite;
        public Gender Gender;
        public CustomerType Type;
        public List<FlowerType> FlowerTypes;
        public List<ConversationType> InitialConversationTypes;
        public List<ConversationType> GoodbyeConversationTypes;

        [MinMaxSlider("DynamicRange", true)]
        public Vector2Int FlowerCount = new(1, 5);

        [MinMaxSlider("DynamicRange", true)]
        public Vector2 PaymentAmount = new(10, 50);

        public float TipGiveRatio;
        [MinMaxSlider("DynamicRange", true)]
        public Vector2 TipAmount = new(10, 50);
    }

    [Serializable]
    public class DayInfo
    {
        public List<CustomerType> Customers;
        [Tooltip("In minutes")]
        public float DayDuration;
        public SpecialEvents SpecialEvent;
        [HideIf(nameof(SpecialEvent), SpecialEvents.None)]
        public bool IsSpecialEventOnDayStart;
    }

    public SerializedDictionary<CustomerType, Customer> Customers;
    public SerializedDictionary<ConversationType, Conversation> Conversations;
    public List<DayInfo> Days;

}

