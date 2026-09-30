using System;

namespace DollShop.Core
{
    /// <summary>Day 1~6 진행. W3+W4 완성. DayConfig.csv 기반 하위 시스템 초기화.</summary>
    public static class DayFlow
    {
        public static int CurrentDay { get; private set; } = 1;

        public static event Action<int>    OnDayStarted;
        public static event Action<string> OnScriptedEvent;

        private static DayConfigSO _dayConfigSO;

        public static void Initialize(DayConfigSO dayConfigSO)
        {
            _dayConfigSO = dayConfigSO;
            DayTimer.OnDayEnd += HandleDayEnd;
        }

        public static void StartDay(int day)
        {
            CurrentDay = day;
            GameState.CurrentDay = day;

            var config = _dayConfigSO?.GetDay(day);
            if (config == null)
            {
                UnityEngine.Debug.LogError($"[DayFlow] Day {day} 설정이 없습니다.");
                return;
            }

            // 하위 시스템 초기화 순서
            ToolState.ResetForDay();
            SanitySystem.BeginDay();
            EconomySystem.BeginDay();
            ShopSystem.BeginDay();
            DollRuleSystem.BeginDay(day);
            OrderSystem.BeginDay(config.orderCount, day * 100 + 7);
            AnomalySystem.BeginDay(config.GetAnomalyIds());

            // Persistent에서 최고 도달 Day 갱신
            if (day > GameState.Persistent.MaxDayReached)
                GameState.Persistent.MaxDayReached = day;

            DayTimer.StartDay(config.maxTick);
            OnDayStarted?.Invoke(day);

            // 인트로 이벤트
            if (!string.IsNullOrEmpty(config.introEventId))
                OnScriptedEvent?.Invoke(config.introEventId);
        }

        public static void RequestNextDay()
        {
            int next = CurrentDay + 1;
            if (next > 6)
            {
                EndingSystem.CheckEscape();
                return;
            }
            StartDay(next);
        }

        private static void HandleDayEnd(DayEndReason reason)
        {
            var config = _dayConfigSO?.GetDay(CurrentDay);

            switch (reason)
            {
                case DayEndReason.COMPLETE:
                    bool noAccident = GameState.FaintCount == 0;
                    EconomySystem.SettleDay(CurrentDay, noAccident);
                    if (config != null && !string.IsNullOrEmpty(config.outroEventId))
                        OnScriptedEvent?.Invoke(config.outroEventId);
                    SceneFlow.GoToSettlement();
                    break;

                case DayEndReason.TIMEOUT:
                case DayEndReason.DEATH:
                    LoopSystem.EndRun(reason == DayEndReason.DEATH
                        ? FindAttackingDoll()
                        : (DollId?)null);
                    break;

                case DayEndReason.FAINT_LIMIT:
                    // 그날 재시작 (Day 유지, 골드 유지, 진행도 초기화)
                    StartDay(CurrentDay);
                    break;
            }
        }

        private static DollId? FindAttackingDoll()
        {
            foreach (DollId d in System.Enum.GetValues(typeof(DollId)))
                if (DollRuleSystem.StageOf(d) == DollStage.Attack) return d;
            return null;
        }
    }
}
