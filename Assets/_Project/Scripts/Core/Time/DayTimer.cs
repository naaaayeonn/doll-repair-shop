using System;
using UnityEngine;

namespace DollShop.Core
{
    /// <summary>
    /// 1틱 = 10초. 36틱 상한. W3 완성.
    /// [흐르는 시간] 실시간 누적 → 10초마다 AdvanceTick → OnTick
    /// [소비하는 시간] 행동 발생 시 SpendTicks로 즉시 가산
    /// 일시정지: IsPaused = true 동안 흐르는 시간 정지 (행동 소비는 그대로)
    /// </summary>
    public static class DayTimer
    {
        public static int  MaxTick     { get; private set; } = 36;
        public static int  CurrentTick { get; private set; } = 0;
        public static bool IsPaused    { get; private set; } = false;
        public static bool IsRunning   { get; private set; } = false;

        private static float _accumulatedSeconds = 0f;
        public  const  float TICK_DURATION       = 10f; // 확정 사양

        public static event Action<int>          OnTick;
        public static event Action<DayEndReason> OnDayEnd;

        // ── 외부 API ─────────────────────────────────────────
        public static void StartDay(int maxTick = 36)
        {
            MaxTick              = maxTick;
            CurrentTick          = 0;
            _accumulatedSeconds  = 0f;
            IsPaused             = false;
            IsRunning            = true;
            GameState.CurrentTick = 0;
        }

        public static void SetPaused(bool paused)
        {
            IsPaused = paused;
        }

        /// <summary>흐르는 시간 업데이트. View의 MonoBehaviour.Update()에서 매 프레임 호출.</summary>
        public static void Tick(float deltaTime)
        {
            if (!IsRunning || IsPaused) return;
            _accumulatedSeconds += deltaTime;
            while (_accumulatedSeconds >= TICK_DURATION && IsRunning)
            {
                _accumulatedSeconds -= TICK_DURATION;
                AdvanceTick();
            }
        }

        /// <summary>행동에 의한 즉시 틱 소비. (이동 1틱 등)</summary>
        public static void SpendTicks(int count)
        {
            if (!IsRunning) return;
            for (int i = 0; i < count && IsRunning; i++)
                AdvanceTick();
        }

        /// <summary>기절 시 시간 점프.</summary>
        public static void JumpTicks(int count)
        {
            // 점프는 누적 초도 리셋 (점프 후 즉시 다음 틱이 오는 것 방지)
            _accumulatedSeconds = 0f;
            SpendTicks(count);
        }

        public static void ForceComplete()
        {
            if (!IsRunning) return;
            IsRunning = false;
            OnDayEnd?.Invoke(DayEndReason.COMPLETE);
        }

        public static void ForceDeath()
        {
            if (!IsRunning) return;
            IsRunning = false;
            OnDayEnd?.Invoke(DayEndReason.DEATH);
        }

        public static void ForceFaintLimit()
        {
            if (!IsRunning) return;
            IsRunning = false;
            OnDayEnd?.Invoke(DayEndReason.FAINT_LIMIT);
        }

        // ── 내부 ─────────────────────────────────────────────
        private static void AdvanceTick()
        {
            CurrentTick++;
            GameState.CurrentTick = CurrentTick;
            OnTick?.Invoke(CurrentTick);

            if (CurrentTick >= MaxTick)
            {
                IsRunning = false;
                OnDayEnd?.Invoke(DayEndReason.TIMEOUT);
            }
        }
    }
}
