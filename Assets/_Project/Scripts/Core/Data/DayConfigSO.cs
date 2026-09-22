using System;
using UnityEngine;

namespace DollShop.Core
{
    [CreateAssetMenu(fileName = "DayConfigSO", menuName = "DollShop/Data/DayConfig")]
    public class DayConfigSO : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public int    day;
            public int    orderCount;
            public int    maxTick;
            public string activeDolls;     // "RABBIT|OCTOPUS"
            public string anomalyPoolIds;  // "ANO_RABBIT_1|ANO_OCTO_1"
            public string introEventId;
            public string outroEventId;

            public string[] GetAnomalyIds()
            {
                if (string.IsNullOrEmpty(anomalyPoolIds)) return Array.Empty<string>();
                return anomalyPoolIds.Split('|');
            }
        }
        public Entry[] entries;

        public Entry GetDay(int day)
        {
            if (entries == null) return null;
            foreach (var e in entries)
                if (e.day == day) return e;
            return null;
        }
    }
}
