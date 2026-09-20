using System;
using UnityEngine;

namespace DollShop.Core
{
    [CreateAssetMenu(fileName = "DollRulesSO", menuName = "DollShop/Data/DollRules")]
    public class DollRulesSO : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string     dollId;           // "RABBIT"
            public int        day;
            public bool       active;
            public int        startStage;       // 3
            public int        moveInterval;     // 틱
            public string     counterType;      // "IMMEDIATE"
            public int        clueStage;        // 1
            public int        attackDelayTick;  // 2
        }
        public Entry[] entries;

        public Entry GetRule(string dollId, int day)
        {
            if (entries == null) return null;
            foreach (var e in entries)
                if (e.dollId == dollId && e.day == day)
                    return e;
            return null;
        }
    }
}
