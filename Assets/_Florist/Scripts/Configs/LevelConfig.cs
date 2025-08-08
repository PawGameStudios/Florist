using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Config
{
    public enum CustomerType
    {
        Regular, Random, Opponent, Specific
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

    public enum PaymentState
    {
        None, Normal, NotPaid, Overpaid, Underpaid
    }

    public enum Gender
    {
        None, Boy, Girl, Random
    }

    public enum SpecialEvents
    {
        None, InroduceFlower, IntroducePaper, IntroduceRibbon, IntroduceGarden
    }

    [Serializable]
    public class DayEvent
    {
        public bool IsEvent;
        [ShowIf("IsEvent")] public SpecialEvents EventType;
        [HideIf("IsEvent")] public CustomerType CustomerType;
        [ShowIf("CustomerType", CustomerType.Specific)] public string CustomerName;

        [ShowIf("@this.EventType == SpecialEvents.InroduceFlower && IsEvent")]
        public FlowerType IntroducedFlowerType;

        [ShowIf("@this.EventType == SpecialEvents.InroduceFlower && IsEvent")]
        public FlowerColor IntroducedFlowerColor;

        [ShowIf("@this.EventType == SpecialEvents.IntroducePaper && IsEvent")]
        public WrappingPaperType PaperType;

        [ShowIf("@this.EventType == SpecialEvents.IntroduceRibbon && IsEvent")]
        public RibbonType RibbonType;
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
        public DayTimeInfo DayTimeInfo;
        public List<DayInfo> Days;
        public DayInfo RandomDayInfo;
    }
}
