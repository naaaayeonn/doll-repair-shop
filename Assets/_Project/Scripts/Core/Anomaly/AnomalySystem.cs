using System;
using System.Collections.Generic;
using UnityEngine;

namespace DollShop.Core
{
    /// <summary>이상현상 발생·타이머·미대응 페널티.</summary>
    public static class AnomalySystem
    {
        // ── 이벤트 ─────────────────────────────────────────
        public static event Action<AnomalyInfo>     OnRaised;   // 발생
        public static event Action<AnomalyId, bool> OnResolved; // (id, 성공여부)
        public static event Action<AnomalyId, int>  OnTicking;  // (id, 남은 틱) 게이지용

        // ── 내부 상태 ───────────────────────────────────────
        private class AnomalyRecord
        {
            public AnomalyId Id;
            public RoomId    Room;
            public int       RemainingTick;
            public float     TriggerChancePerTick;
            public int       IntervalPenalty;
            public bool      Active;
        }

        private static readonly List<AnomalyRecord>         _activeAnomalies = new List<AnomalyRecord>();
        private static readonly List<AnomalyTableSO.Entry>  _dayPool         = new List<AnomalyTableSO.Entry>();
        private static AnomalyTableSO _tableSO;

        public static void Initialize(AnomalyTableSO tableSO)
        {
            _tableSO = tableSO;
            DayTimer.OnTick += ProcessTick;
        }

        public static void BeginDay(string[] anomalyIds)
        {
            _activeAnomalies.Clear();
            _dayPool.Clear();
            // TODO: W3 — anomalyIds 목록으로 _dayPool 구성
        }

        private static void ProcessTick(int tick)
        {
            // 기존 활성 이상현상 타이머 처리
            for (int i = _activeAnomalies.Count - 1; i >= 0; i--)
            {
                var rec = _activeAnomalies[i];
                rec.RemainingTick--;
                OnTicking?.Invoke(rec.Id, rec.RemainingTick);

                if (rec.RemainingTick <= 0)
                {
                    // 시간 초과 → 미대응 페널티
                    SanitySystem.PenalizeAnomalyMiss();
                    // TODO: W3 — 해당 인형 moveInterval -1
                    OnResolved?.Invoke(rec.Id, false);
                    _activeAnomalies.RemoveAt(i);
                }
            }

            // 새 이상현상 발생 판정
            // TODO: W3 — _dayPool에서 확률 기반 발생
        }

        /// <summary>이상현상 대응 시도.</summary>
        public static bool TryResolve(AnomalyId id)
        {
            for (int i = 0; i < _activeAnomalies.Count; i++)
            {
                if (_activeAnomalies[i].Id == id)
                {
                    _activeAnomalies.RemoveAt(i);
                    OnResolved?.Invoke(id, true);
                    return true;
                }
            }
            return false;
        }
    }
}
