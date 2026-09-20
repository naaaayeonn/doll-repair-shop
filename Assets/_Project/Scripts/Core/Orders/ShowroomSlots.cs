using System;
using System.Collections.Generic;

namespace DollShop.Core
{
    /// <summary>진열실 슬롯 6개 관리. 만석 시 진열 거부.</summary>
    public static class ShowroomSlots
    {
        public const int MAX_SLOTS = 6;
        private static readonly List<string> _dollVariantIds = new List<string>(MAX_SLOTS);

        public static int  OccupiedCount => _dollVariantIds.Count;
        public static bool IsFull        => _dollVariantIds.Count >= MAX_SLOTS;

        public static event Action<string, int> OnDollPlaced;   // (variantId, slotIndex)
        public static event Action<int>         OnDollRemoved;  // slotIndex

        public static bool TryPlace(string dollVariantId)
        {
            if (IsFull) return false;
            _dollVariantIds.Add(dollVariantId);
            OnDollPlaced?.Invoke(dollVariantId, _dollVariantIds.Count - 1);
            return true;
        }

        public static bool TryRemove(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _dollVariantIds.Count) return false;
            _dollVariantIds.RemoveAt(slotIndex);
            OnDollRemoved?.Invoke(slotIndex);
            return true;
        }

        public static void ClearAll()
        {
            _dollVariantIds.Clear();
        }
    }
}
