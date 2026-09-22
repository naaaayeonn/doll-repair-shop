using System;
using UnityEngine;

namespace DollShop.Core
{
    /// <summary>
    /// 정신력 관리. 감소·급감·기절 3회 규칙.
    /// 수치는 SanityConfigSO에서 읽는다.
    /// </summary>
    public static class SanitySystem
    {
        // ── 읽기 전용 프로퍼티 ─────────────────────────────
        public static float Current    { get; private set; } = 100f;
        public static float Max        { get; private set; } = 100f;
        public static int   FaintCount { get; private set; } = 0;

        // ── 이벤트 ─────────────────────────────────────────
        public static event Action<float, float> OnChanged;     // (current, max)
        public static event Action<SanityCause>  OnDrained;     // 급감 연출 트리거용
        public static event Action<int>          OnFaint;       // (누적 기절 횟수)
        public static event Action               OnDayRestart;  // 기절 3회

        // ── 설정 (SanityConfigSO에서 로드) ─────────────────
        private static float _drainPerTick        = 1f;
        private static float _anomalyMissPenalty  = 15f;
        private static float _doorStagePenalty    = 8f;
        private static float _counterFailPenalty  = 20f;
        private static float _repairBonus         = 3f;
        private static int   _faintJumpTick       = 4;
        private static float _faintRestore        = 50f;
        private static int   _faintLimit          = 3;

        // ── 초기화 ──────────────────────────────────────────
        public static void Initialize(SanityConfigSO config)
        {
            if (config == null) { Debug.LogWarning("[SanitySystem] config가 없습니다. 기본값 사용."); return; }
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
        }

        // ── 틱 드레인 (DayTimer.OnTick에서 호출) ──────────
        public static void OnTick(int tick)
        {
            // TODO: W3 — SOFT_RUG 아이템 보유 시 드레인 감소
            Apply(-_drainPerTick, SanityCause.TICK);
        }

        // ── 외부 감소 API ────────────────────────────────────
        public static void PenalizeAnomalyMiss()  => Apply(-_anomalyMissPenalty, SanityCause.ANOMALY_MISS);
        public static void PenalizeDoorStage()     => Apply(-_doorStagePenalty,   SanityCause.DOOR_STAGE);
        public static void PenalizeCounterFail()   => Apply(-_counterFailPenalty, SanityCause.COUNTER_FAIL);
        public static void BonusRepair()           => Apply(_repairBonus,         SanityCause.ITEM);
        public static void RestoreByItem(float amt) => Apply(amt,                 SanityCause.ITEM);

        // ── 내부 ────────────────────────────────────────────
        private static void Apply(float delta, SanityCause cause)
        {
            float prev = Current;
            Current = Mathf.Clamp(Current + delta, 0f, Max);
            GameState.Sanity = Current;

            if (delta < -5f) // 급감(연출 트리거 기준)
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

            // 기절 중에도 인형은 정상 이동 → 시간 점프 후 상황이 나빠져 있어야 한다.
            DayTimer.JumpTicks(_faintJumpTick);
            Current = _faintRestore;
            GameState.Sanity = Current;
            NotifyChanged();
        }

        private static void NotifyChanged() => OnChanged?.Invoke(Current, Max);
    }
}
