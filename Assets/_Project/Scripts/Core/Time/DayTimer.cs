using System;
using UnityEngine;

namespace DollShop.Core
{
    /// <summary>
    /// 1틱 = 10초. 36틱 상한.
    /// 시간은 두 경로로 간다:
    ///   [흐르는 시간] 실시간 누적 → 10초마다 TickCount++ → OnTick 발행
    ///   [소비하는 시간] 행동 발생 시 즉시 가산 (이동 1틱 등)
    /// </summary>
    public static class DayTimer
    {
        // 설정값 — SanityConfigSO에서 읽어온다
        public static int MaxTick { get; private set; } = 36;

        public static int  CurrentTick { get; private set; } = 0;
        public static bool IsPaused    { get; private set; } = false;
        public static bool IsRunning   { get; private set; } = false;

        private static float _accumulatedSeconds = 0f;
        private const  float TICK_DURATION = 10f; // 1틱 = 10초 (확정 사양)

        // ── 이벤트 ─────────────────────────────────────────
        public static event Action<int>          OnTick;    // 매 틱 (인자: 현재 틱)
        public static event Action<DayEndReason> OnDayEnd;  // COMPLETE / TIMEOUT / DEATH / FAINT_LIMIT

        // ── 외부 API ────────────────────────────────────────
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

        /// <summary>행동에 의한 즉시 틱 소비. (이동, 대응 등)</summary>
        public static void SpendTicks(int count)
        {
            if (!IsRunning) return;
            for (int i = 0; i < count; i++)
                AdvanceTick();
        }

        /// <summary>기절 시 시간 점프.</summary>
        public static void JumpTicks(int count)
        {
            SpendTicks(count);
        }

        /// <summary>의뢰 완료 시 호출. COMPLETE 종료.</summary>
        public static void ForceComplete()
        {
            if (!IsRunning) return;
            IsRunning = false;
            OnDayEnd?.Invoke(DayEndReason.COMPLETE);
        }

        /// <summary>사망 시 호출.</summary>
        public static void ForceDeath()
        {
            if (!IsRunning) return;
            IsRunning = false;
            OnDayEnd?.Invoke(DayEndReason.DEATH);
        }

        /// <summary>기절 3회 시 호출.</summary>
        public static void ForceFaintLimit()
        {
            if (!IsRunning) return;
            IsRunning = false;
            OnDayEnd?.Invoke(DayEndReason.FAINT_LIMIT);
        }

        /// <summary>MonoBehaviour의 Update()에서 매 프레임 호출한다(View 쪽에서).</summary>
        public static void Tick(float deltaTime)
        {
            if (!IsRunning || IsPaused) return;

            _accumulatedSeconds += deltaTime;
            while (_accumulatedSeconds >= TICK_DURATION)
            {
                _accumulatedSeconds -= TICK_DURATION;
                AdvanceTick();
                if (!IsRunning) break; // 틱 중 종료됐을 수 있음
            }
        }

        // ── 내부 ────────────────────────────────────────────
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
