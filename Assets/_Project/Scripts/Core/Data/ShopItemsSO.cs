using System;
using UnityEngine;

namespace DollShop.Core
{
    [CreateAssetMenu(fileName = "ShopItemsSO", menuName = "DollShop/Data/ShopItems")]
    public class ShopItemsSO : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string itemId;
            public string nameKey;
            public int    price;
            public string effectType;
            public float  effectValue;
            public int    gridW;
            public int    gridH;
            public string room;
        }
        public Entry[] entries;

        public Entry GetEntry(string id)
        {
            foreach (var e in entries)
                if (e.itemId == id) return e;
            return null;
        }
    }
}
