using System;
using System.Collections.Generic;
using UnityEngine;

namespace DollShop.Core
{
    /// <summary>아이템 6종 구매·배치·효과 적용.</summary>
    public static class ShopSystem
    {
        public static event Action<ItemId>             OnPurchased;
        public static event Action<ItemId, Vector2Int> OnPlaced;

        private static readonly HashSet<ItemId>               _owned  = new HashSet<ItemId>();
        private static readonly Dictionary<ItemId, Vector2Int> _placed = new Dictionary<ItemId, Vector2Int>();
        private static ShopItemsSO _itemsSO;

        public static void Initialize(ShopItemsSO itemsSO) => _itemsSO = itemsSO;

        public static void ResetForLoop()
        {
            _owned.Clear();
            _placed.Clear();
        }

        public static BuyResult TryBuy(ItemId id)
        {
            if (_itemsSO == null) return BuyResult.NO_GOLD;
            if (_owned.Contains(id)) return BuyResult.OWNED;

            var entry = _itemsSO.GetEntry(id.ToString());
            if (entry == null) return BuyResult.NO_GOLD;

            if (GameState.Gold < entry.price) return BuyResult.NO_GOLD;

            GameState.Gold -= entry.price;
            EconomySystem.AddGold(-entry.price, 0);
            _owned.Add(id);
            OnPurchased?.Invoke(id);
            ApplyPassiveEffect(id, entry);
            return BuyResult.OK;
        }

        public static bool TryPlace(ItemId id, Vector2Int cell)
        {
            if (!_owned.Contains(id)) return false;
            _placed[id] = cell;
            OnPlaced?.Invoke(id, cell);
            return true;
        }

        public static bool TryUse(ItemId id)
        {
            if (!_owned.Contains(id)) return false;
            if (!_placed.ContainsKey(id)) return false; // 배치되어야만 사용 가능

            var entry = _itemsSO?.GetEntry(id.ToString());
            if (entry == null) return false;

            // TODO: W4 — 소모형 아이템 효과 적용
            if (entry.effectType == "SANITY_RESTORE")
            {
                SanitySystem.RestoreByItem(entry.effectValue);
                _owned.Remove(id);
                _placed.Remove(id);
                return true;
            }
            return false;
        }

        public static bool IsOwned(ItemId id)  => _owned.Contains(id);
        public static bool IsPlaced(ItemId id) => _placed.ContainsKey(id);

        private static void ApplyPassiveEffect(ItemId id, ShopItemsSO.Entry entry)
        {
            // TODO: W4 — 패시브 효과 적용 (TOOL_BELT, SOFT_RUG, DESK_LAMP 등)
        }
    }
}
