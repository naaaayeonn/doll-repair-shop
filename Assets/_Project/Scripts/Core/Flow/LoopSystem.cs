using System;

namespace DollShop.Core
{
    /// <summary>사망→Day 1 복귀, 유지/초기화 분리.</summary>
    public static class LoopSystem
    {
        public static int DeathCount { get; private set; } = 0;

        public static event Action<DollId?> OnRunEnded;    // null = 시간초과/기절
        public static event Action          OnRunRestarted;

        public static void EndRun(DollId? killerDoll)
        {
            DeathCount++;
            GameState.Persistent.DeathCount = DeathCount;

            // 루프 퍼시스턴트 데이터 저장
            SaveSystem.Save();

            OnRunEnded?.Invoke(killerDoll);
        }

        public static void RestartRun()
        {
            // 유지: GameState.Persistent (단서, 사망 횟수 등)
            // 초기화: Gold, Day, 상점, 인형 상태 등
            GameState.StartNewRun();
            ShopSystem.ResetForLoop();

            OnRunRestarted?.Invoke();
            DayFlow.StartDay(1);
        }
    }
}
