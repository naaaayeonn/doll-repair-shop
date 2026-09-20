using System;

namespace DollShop.Core
{
    /// <summary>탈출 조건 판정(§2.10).</summary>
    public static class EndingSystem
    {
        public static bool IsEscapeUnlocked { get; private set; } = false;

        public static event Action OnEscapeUnlocked;
        public static event Action OnEscaped;

        /// <summary>Day 종료 시 탈출 조건 체크.</summary>
        internal static void CheckEscape()
        {
            bool canEscape = NoteSystem.Completion >= 1f
                          && DayFlow.CurrentDay == 6
                          && GameState.OrdersDone >= 5; // Day 6 의뢰 5건

            if (canEscape && !IsEscapeUnlocked)
            {
                IsEscapeUnlocked = true;
                OnEscapeUnlocked?.Invoke();
            }
            else if (!canEscape)
            {
                // 노트 미완성 → 강제 사망
                LoopSystem.EndRun(null);
            }
        }

        /// <summary>현관 상호작용 시 호출.</summary>
        public static bool TryEscape()
        {
            if (!IsEscapeUnlocked) return false;
            OnEscaped?.Invoke();
            SceneFlow.GoToEnding();
            return true;
        }
    }
}
