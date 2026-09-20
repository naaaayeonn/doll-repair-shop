using System;

namespace DollShop.Core
{
    /// <summary>Day 1~6 진행, 날짜별 이벤트 스크립팅. W5에 완성.</summary>
    public static class DayFlow
    {
        public static int CurrentDay { get; private set; } = 1;

        public static event Action<int>    OnDayStarted;
        public static event Action<string> OnScriptedEvent; // "EV_D3_LOCK" 등

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
            if (config == null) return;

            // 하위 시스템 초기화
            ToolState.ResetForDay();
            SanitySystem.BeginDay();
            EconomySystem.BeginDay();
            DollRuleSystem.BeginDay(day);
            OrderSystem.BeginDay(config.orderCount, day * 100);
            AnomalySystem.BeginDay(config.GetAnomalyIds());

            DayTimer.StartDay(config.maxTick);

            OnDayStarted?.Invoke(day);

            // 인트로 이벤트
            if (!string.IsNullOrEmpty(config.introEventId))
                OnScriptedEvent?.Invoke(config.introEventId);
        }

        public static void RequestNextDay()
        {
            int nextDay = CurrentDay + 1;
            if (nextDay > 6)
            {
                // Day 6 이후: 탈출 조건 체크
                EndingSystem.CheckEscape();
                return;
            }
            StartDay(nextDay);
        }

        private static void HandleDayEnd(DayEndReason reason)
        {
            switch (reason)
            {
                case DayEndReason.COMPLETE:
                    var config = _dayConfigSO?.GetDay(CurrentDay);
                    if (config != null && !string.IsNullOrEmpty(config.outroEventId))
                        OnScriptedEvent?.Invoke(config.outroEventId);
                    EconomySystem.SettleDay(CurrentDay, GameState.FaintCount == 0);
                    SceneFlow.GoToSettlement();
                    break;
                case DayEndReason.TIMEOUT:
                case DayEndReason.DEATH:
                    LoopSystem.EndRun(reason == DayEndReason.DEATH ? (DollId?)null : null);
                    break;
                case DayEndReason.FAINT_LIMIT:
                    // 그날 재시작 (Day 유지)
                    StartDay(CurrentDay);
                    break;
            }
        }
    }
}
