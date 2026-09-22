using System;
using UnityEngine;

namespace DollShop.Core
{
    [CreateAssetMenu(fileName = "AnomalyTableSO", menuName = "DollShop/Data/AnomalyTable")]
    public class AnomalyTableSO : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string anomalyId;
            public string ownerDoll;
            public string room;
            public float  triggerChancePerTick;
            public int    limitTick;
            public string counterAction;
            public int    sanityPenalty;
            public int    intervalPenalty;
        }
        public Entry[] entries;

        public Entry GetEntry(string id)
        {
            foreach (var e in entries)
                if (e.anomalyId == id) return e;
            return null;
        }
    }
}
