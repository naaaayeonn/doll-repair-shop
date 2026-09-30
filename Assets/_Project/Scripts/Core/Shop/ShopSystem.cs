using System;
using System.Collections.Generic;
using UnityEngine;

namespace DollShop.Core
{
    /// <summary>
    /// 아이템 6종 구매·배치·효과 적용. W4 완성.
    /// 배치되지 않은 아이템은 효과 없음.
    /// SANITY_TEA: 소모형, 1일 1회.
    /// </summary>
    public static class ShopSystem
    {
        public static event Action<ItemId>             OnPurchased;
        public static event Action<ItemId, Vector2Int> OnPlaced;
        public static event Action<ItemId>             OnRemoved;

        private static readonly HashSet<ItemId>                _owned     = new HashSet<ItemId>();
        private static readonly Dictionary<ItemId, Vector2Int> _placed    = new Dictionary<ItemId, Vector2Int>();
        private static readonly HashSet<ItemId>                _usedToday = new HashSet<ItemId>();
        private static ShopItemsSO _itemsSO;

        // ── 1일 1회 제한 리셋 ────────────────────────────────
        public static void BeginDay()
        {
            _usedToday.Clear();
        }

        public static void Initialize(ShopItemsSO itemsSO)
        {
            _itemsSO = itemsSO;
        }

        public static void ResetForLoop()
        {
            _owned.Clear();
            _placed.Clear();
            _usedToday.Clear();
            ApplyAllEffects();
        }

        // ── 구매 ─────────────────────────────────────────────
        public static BuyResult TryBuy(ItemId id)
        {
            if (_itemsSO == null) return BuyResult.NO_GOLD;
            if (_owned.Contains(id)) return BuyResult.OWNED;

            var entry = _itemsSO.GetEntry(id.ToString());
            if (entry == null) return BuyResult.NO_GOLD;
            if (GameState.Gold < entry.price) return BuyResult.NO_GOLD;

            // 골드 차감
            EconomySystem.AddGold(-entry.price, 0);
            _owned.Add(id);
            OnPurchased?.Invoke(id);
            return BuyResult.OK;
        }

        // ── 배치 ─────────────────────────────────────────────
        public static bool TryPlace(ItemId id, Vector2Int cell)
        {
            if (!_owned.Contains(id)) return false;
            _placed[id] = cell;
            OnPlaced?.Invoke(id, cell);
            ApplyAllEffects();
            return true;
        }

        public static bool TryRemove(ItemId id)
        {
            if (!_placed.ContainsKey(id)) return false;
            _placed.Remove(id);
            OnRemoved?.Invoke(id);
            ApplyAllEffects();
            return true;
        }

        // ── 사용 (소모형) ─────────────────────────────────────
        public static bool TryUse(ItemId id)
        {
            if (!_owned.Contains(id)) return false;
            if (!_placed.ContainsKey(id)) return false;
            if (_usedToday.Contains(id)) return false;

            var entry = _itemsSO?.GetEntry(id.ToString());
            if (entry == null) return false;

            if (entry.effectType == "SANITY_RESTORE")
            {
                SanitySystem.RestoreByItem(entry.effectValue);
                _usedToday.Add(id);
                return true;
            }
            return false;
        }

        public static bool IsOwned(ItemId id)  => _owned.Contains(id);
        public static bool IsPlaced(ItemId id) => _placed.ContainsKey(id);

        // ── 효과 적용 파이프라인 ─────────────────────────────
        private static void ApplyAllEffects()
        {
            // 효과 초기화
            TimeService.SetToolBeltActive(false);
            AnomalySystem.SetSturdyRackActive(false);
            OrderSystem.SetDeskLampActive(false);
            DollRuleSystem.SetWallClockActive(false);
            SanitySystem.SetSoftRugActive(false);

            if (_itemsSO == null) return;

            foreach (var kvp in _placed)
            {
                var entry = _itemsSO.GetEntry(kvp.Key.ToString());
                if (entry == null) continue;

                switch (entry.effectType)
                {
                    case "TOOL_MOVE_FREE":
                        TimeService.SetToolBeltActive(true);
                        break;
                    case "ANOMALY_CHANCE":
                        AnomalySystem.SetSturdyRackActive(true);
                        break;
                    case "REPAIR_TOLERANCE":
                        OrderSystem.SetDeskLampActive(true);
                        break;
                    case "STAGE_HINT":
                        DollRuleSystem.SetWallClockActive(true);
                        break;
                    case "SANITY_DRAIN":
                        SanitySystem.SetSoftRugActive(true);
                        break;
                }
            }
        }
    }
}
