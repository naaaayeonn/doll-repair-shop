using System;
using UnityEngine;

namespace DollShop.Core
{
    [CreateAssetMenu(fileName = "ClueTableSO", menuName = "DollShop/Data/ClueTable")]
    public class ClueTableSO : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string clueId;
            public string dollId;
            public string channel;          // "SIGHT"
            public string unlockCondition;  // "INSPECT"
            [TextArea]
            public string noteText;
        }
        public Entry[] entries;

        public Entry GetEntry(string clueId)
        {
            foreach (var e in entries)
                if (e.clueId == clueId) return e;
            return null;
        }
    }
}
