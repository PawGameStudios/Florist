using System;
using System.Collections.Generic;
using UnityEngine;

namespace Florist.Merge
{
    [Serializable]
    public class MergeOrderStage
    {
        public string Name;
        public string LocalizationKey;
        [Min(0)] public int CompletedOrders;
        [Range(1, 2)] public int MaxProductTypes = 1;
        [Range(1, 3)] public int MaxCountPerProduct = 1;
        [Range(1, 4)] public int MaxTotalProducts = 1;
    }

    [CreateAssetMenu(fileName = "MergeProgressionConfig", menuName = "MergeGame/Progression")]
    public sealed class MergeProgressionConfig : ScriptableObject
    {
        public List<MergeOrderStage> Stages = new List<MergeOrderStage>();
        [Range(0, 1)] public float LatestProductChance = 0.6f;

        public MergeOrderStage GetStage(int completed)
        {
            MergeOrderStage result = null;
            foreach (var stage in Stages)
                if (stage != null && stage.CompletedOrders <= completed &&
                    (result == null || stage.CompletedOrders > result.CompletedOrders)) result = stage;
            return result ?? new MergeOrderStage { Name = "Başlangıç", LocalizationKey = "merge_stage_0" };
        }

        public int GetNextThreshold(int completed)
        {
            int next = int.MaxValue;
            foreach (var stage in Stages)
                if (stage != null && stage.CompletedOrders > completed) next = Math.Min(next, stage.CompletedOrders);
            return next == int.MaxValue ? -1 : next;
        }
    }
}
