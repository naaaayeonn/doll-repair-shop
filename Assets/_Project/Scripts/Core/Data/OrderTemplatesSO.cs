using System;
using UnityEngine;

namespace DollShop.Core
{
    [CreateAssetMenu(fileName = "OrderTemplatesSO", menuName = "DollShop/Data/OrderTemplates")]
    public class OrderTemplatesSO : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string templateId;
            public string dollVariant;
            public string minigameType;  // "SEAM"
            public int    requirementCount;
            public float  targetSeconds;
            public int    basePrice;
        }
        public Entry[] entries;

        public Entry GetEntry(string id)
        {
            foreach (var e in entries)
                if (e.templateId == id) return e;
            return null;
        }
    }
}
