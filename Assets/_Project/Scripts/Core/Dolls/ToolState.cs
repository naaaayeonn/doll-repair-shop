using System;
using System.Collections.Generic;

namespace DollShop.Core
{
    /// <summary>가위·실·재단칼 보유·수납 상태 관리. W4 완성.</summary>
    public static class ToolState
    {
        private static readonly HashSet<ToolId> _held = new HashSet<ToolId>();

        /// <summary>재단칼이 도구함에 수납되어 있는가. false면 BEAR 가속.</summary>
        public static bool IsKnifeStored { get; private set; } = false;

        public static event Action<ToolId, bool> OnToolChanged;   // (tool, held)
        public static event Action               OnKnifeStored;   // 재단칼 치웠을 때
        public static event Action               OnKnifeRestored; // 재단칼 다시 꺼냈을 때

        public static bool HasTool(ToolId tool) => _held.Contains(tool);

        /// <summary>도구 집기. 이미 보유 중이면 false.</summary>
        public static bool TryTake(ToolId tool)
        {
            if (_held.Contains(tool)) return false;
            _held.Add(tool);
            OnToolChanged?.Invoke(tool, true);
            return true;
        }

        /// <summary>도구 내려놓기 (사용 소비 등).</summary>
        public static bool TryDrop(ToolId tool)
        {
            if (!_held.Remove(tool)) return false;
            OnToolChanged?.Invoke(tool, false);
            return true;
        }

        /// <summary>
        /// 재단칼을 도구함에 치운다. BEAR 예방 행동.
        /// 이미 치워져 있으면 false 반환.
        /// </summary>
        public static bool TryStoreKnife()
        {
            if (IsKnifeStored) return false;
            IsKnifeStored = true;
            OnToolChanged?.Invoke(ToolId.KNIFE, false);
            OnKnifeStored?.Invoke();
            return true;
        }

        /// <summary>재단칼을 다시 꺼낸다 (실수 취소 등). BEAR 속도 즉시 복구.</summary>
        public static bool TryRestoreKnife()
        {
            if (!IsKnifeStored) return false;
            IsKnifeStored = false;
            OnToolChanged?.Invoke(ToolId.KNIFE, true);
            OnKnifeRestored?.Invoke();
            return true;
        }

        /// <summary>다음 Day 초기화 시 호출.</summary>
        public static void ResetForDay()
        {
            _held.Clear();
            IsKnifeStored = false;
        }
    }
}
