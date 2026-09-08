using UnityEngine;

namespace Florist.Merge
{
    [CreateAssetMenu(fileName = "MergeEconomyConfig", menuName = "MergeGame/Economy")]
    public sealed class MergeEconomyConfig : ScriptableObject
    {
        [Header("Cell feedback colors")]
        [Tooltip("Background for unlocked items at their final level. Alpha 0 hides it.")]
        public Color MaxLevelCellColor = new Color(.2f, 1f, .2f, .3f);
        [Tooltip("Background of a compatible merge target while dragging. Alpha 0 disables this highlight.")]
        public Color MergeTargetCellColor = Color.clear;

        [Min(1)] public int MaxEnergy = 100;
        [Min(0)] public int StartingEnergy = 100;
        [Min(0)] public int ProducerEnergyCost = 1;
        [Min(1)] public float EnergyRechargeSeconds = 120;
        [Tooltip("Enable to use each ItemData.SpawnChances. Disabled preserves the existing equal level 1/2 distribution.")]
        public bool UseItemSpawnChances;
        [Tooltip("The current inventory panel has room for 24 slots.")]
        [Range(1, 24)] public int InventoryCapacity = 24;
        [Min(1)] public int MaxActiveOrders = 3;
        [Min(0)] public int InitialOrders = 2;
        [Min(1)] public float OrderGenerationSeconds = 30;
        [Min(0)] public int RegularRewardMultiplier = 1;
        [Min(0)] public int VipRewardMultiplier = 2;
        [Min(0)] public int RareRewardMultiplier = 3;

        [Header("Optional rewarded energy")]
        public bool EnableEnergyAds = true;
        [Min(1)] public int RewardedEnergy = 20;
        [Min(0)] public int EnergyAdCooldownSeconds = 60;
        [Min(1)] public int EnergyAdsPerUtcDay = 5;

        public int GetRewardMultiplier(CustomerData.CustomerType type)
        {
            return Mathf.Max(0, type == CustomerData.CustomerType.VIP ? VipRewardMultiplier :
                type == CustomerData.CustomerType.Rare ? RareRewardMultiplier : RegularRewardMultiplier);
        }

        public int ChooseSpawnLevel(ItemData item)
        {
            int levels = item.Sprites.Length;
            if (!UseItemSpawnChances || item.SpawnChances == null)
                return Random.Range(1, Mathf.Min(2, levels) + 1);
            int count = Mathf.Min(levels, item.SpawnChances.Length);
            float total = 0;
            for (int i = 0; i < count; i++) total += Mathf.Max(0, item.SpawnChances[i]);
            if (total <= 0) return 1;
            float roll = Random.value * total;
            int lastEligible = 1;
            for (int i = 0; i < count; i++)
            {
                float weight = Mathf.Max(0, item.SpawnChances[i]);
                if (weight <= 0) continue;
                lastEligible = i + 1;
                roll -= weight;
                if (roll < 0) return i + 1;
            }
            return lastEligible;
        }
    }
}
