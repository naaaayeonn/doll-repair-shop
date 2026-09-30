using System;
using UnityEngine;

namespace DollShop.Core
{
    /// <summary>
    /// 정신력 관리. W3 완성 + W4 SOFT_RUG 효과 추가.
    /// 수치는 SanityConfigSO에서 읽는다. 절대 하드코딩 없음.
    /// </summary>
    public static class SanitySystem
    {
        public static float Current    { get; private set; } = 100f;
        public static float Max        { get; private set; } = 100f;
        public static int   FaintCount { get; private set; } = 0;

        public static event Action<float, float> OnChanged;
        public static event Action<SanityCause>  OnDrained;
        public static event Action<int>          OnFaint;
        public static event Action               OnDayRestart;

        // 설정값 (SanityConfigSO에서 로드)
        private static float _drainPerTick        = 1f;
        private static float _anomalyMissPenalty  = 15f;
        private static float _doorStagePenalty    = 8f;
        private static float _counterFailPenalty  = 20f;
        private static float _repairBonus         = 3f;
        private static int   _faintJumpTick       = 4;
        private static float _faintRestore        = 50f;
        private static int   _faintLimit          = 3;

        // SOFT_RUG 효과: 틱당 감소 -1 → -0.8
        private static bool _softRugActive = false;
        public static void SetSoftRugActive(bool active) => _softRugActive = active;

        public static void Initialize(SanityConfigSO config)
        {
            if (config == null)
            {
                Debug.LogWarning("[SanitySystem] config 없음. 기본값 사용.");
                return;
            }
            Max                 = config.maxSanity;
            _drainPerTick       = config.drainPerTick;
            _anomalyMissPenalty = config.anomalyMissPenalty;
            _doorStagePenalty   = config.doorStagePenalty;
            _counterFailPenalty = config.counterFailPenalty;
            _repairBonus        = config.repairBonus;
            _faintJumpTick      = config.faintJumpTick;
            _faintRestore       = config.faintRestore;
            _faintLimit         = config.faintLimit;
        }

        public static void BeginDay()
        {
            Current    = Max;
            FaintCount = 0;
            GameState.Sanity     = Current;
            GameState.FaintCount = 0;
            NotifyChanged();

            // 틱 이벤트 구독 (중복 방지)
            DayTimer.OnTick -= OnTick;
            DayTimer.OnTick += OnTick;
        }

        // ── 틱 드레인 ─────────────────────────────────────────
        private static void OnTick(int tick)
        {
            float drain = _drainPerTick;
            // SOFT_RUG: 틱당 -1 → -0.8
            if (_softRugActive) drain *= 0.8f;
            Apply(-drain, SanityCause.TICK);
        }

        // ── 외부 감소 API ─────────────────────────────────────
        public static void PenalizeAnomalyMiss()   => Apply(-_anomalyMissPenalty, SanityCause.ANOMALY_MISS);
        public static void PenalizeDoorStage()      => Apply(-_doorStagePenalty,   SanityCause.DOOR_STAGE);
        public static void PenalizeCounterFail()    => Apply(-_counterFailPenalty, SanityCause.COUNTER_FAIL);
        public static void BonusRepair()            => Apply(_repairBonus,         SanityCause.ITEM);
        public static void RestoreByItem(float amt) => Apply(amt,                  SanityCause.ITEM);

        // ── 내부 ─────────────────────────────────────────────
        private static void Apply(float delta, SanityCause cause)
        {
            float prev = Current;
            Current = Mathf.Clamp(Current + delta, 0f, Max);
            GameState.Sanity = Current;

            // 급감 연출 트리거 기준: -5 초과 감소
            if (delta < -5f)
                OnDrained?.Invoke(cause);

            NotifyChanged();

            if (Current <= 0f && prev > 0f)
                Faint();
        }

        private static void Faint()
        {
            FaintCount++;
            GameState.FaintCount = FaintCount;
            OnFaint?.Invoke(FaintCount);

            if (FaintCount >= _faintLimit)
            {
                OnDayRestart?.Invoke();
                DayTimer.ForceFaintLimit();
                return;
            }

            // 기절 중에도 인형 이동 정상 진행 (시간 점프 후 상황 악화 반영)
            DayTimer.JumpTicks(_faintJumpTick);
            Current          = _faintRestore;
            GameState.Sanity = Current;
            NotifyChanged();
        }

        private static void NotifyChanged() => OnChanged?.Invoke(Current, Max);
    }
}
