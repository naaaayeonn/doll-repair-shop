using System;
using UnityEngine;

namespace DollShop.View
{
    /// <summary>
    /// Core의 문자열 키 → 에셋(프리팹·스프라이트 등) 매핑.
    /// W6 아트 교체 주간에 이 레지스트리만 갈아끼우면 전체 아트 교체.
    /// W2~W5에는 임시 회색 박스 프리팹을 꽂아 둔다.
    /// </summary>
    [CreateAssetMenu(fileName = "AssetRegistry", menuName = "DollShop/AssetRegistry")]
    public class AssetRegistry : ScriptableObject
    {
        [Serializable] public class DollVariantEntry
        {
            public string     key;        // "VAR_BEAR_01"
            public GameObject prefab;
            public Sprite     thumbnail;
        }

        [Serializable] public class RoomEntry
        {
            public string  roomId;         // "WORKSHOP"
            public Sprite  bgCalm;
            public Sprite  bgTense;
            public Vector2 interactionAnchor;
        }

        [Serializable] public class AnomalyEntry
        {
            public string     anomalyId;
            public GameObject fxPrefab;
            public Sprite     icon;
            public AudioClip  sfx;
        }

        [Serializable] public class ItemEntry
        {
            public string     itemId;
            public Sprite     icon;
            public GameObject placedPrefab;
        }

        [Serializable] public class CutsceneEntry
        {
            public string     dollId;
            public GameObject timelinePrefab;
        }

        [Serializable] public class ClueEntry
        {
            public string clueId;
            public Sprite noteIllustration;
        }

        public DollVariantEntry[] dollVariants;
        public RoomEntry[]        rooms;
        public AnomalyEntry[]     anomalies;
        public ItemEntry[]        shopItems;
        public CutsceneEntry[]    cutscenes;
        public ClueEntry[]        clues;

        public DollVariantEntry GetDollVariant(string key)
        {
            foreach (var e in dollVariants)
                if (e.key == key) return e;
            return null;
        }

        public RoomEntry GetRoom(string roomId)
        {
            foreach (var e in rooms)
                if (e.roomId == roomId) return e;
            return null;
        }

        public AnomalyEntry GetAnomaly(string anomalyId)
        {
            foreach (var e in anomalies)
                if (e.anomalyId == anomalyId) return e;
            return null;
        }
    }
}
