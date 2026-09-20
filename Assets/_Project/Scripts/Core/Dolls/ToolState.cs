using System;
using System.Collections.Generic;

namespace DollShop.Core
{
    /// <summary>가위·실·재단칼 보유·배치 상태 관리.</summary>
    public static class ToolState
    {
        private static readonly HashSet<ToolId> _held = new HashSet<ToolId>();

        /// <summary>재단칼이 도구함에 수납되어 있는가. false면 BEAR 가속.</summary>
        public static bool IsKnifeStored { get; private set; } = false;

        public static event Action<ToolId, bool> OnToolChanged; // (tool, held)

        public static bool HasTool(ToolId tool) => _held.Contains(tool);

        public static bool TryTake(ToolId tool)
        {
            if (_held.Contains(tool)) return false; // 이미 보유
            _held.Add(tool);
            OnToolChanged?.Invoke(tool, true);
            return true;
        }

        public static bool TryDrop(ToolId tool)
        {
            if (!_held.Remove(tool)) return false;
            OnToolChanged?.Invoke(tool, false);
            return true;
        }

        /// <summary>재단칼을 도구함에 치운다. BEAR 예방 행동.</summary>
        public static bool TryStoreKnife()
        {
            if (IsKnifeStored) return false;
            IsKnifeStored = true;
            OnToolChanged?.Invoke(ToolId.KNIFE, false);
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
