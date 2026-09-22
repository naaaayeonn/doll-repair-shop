using System;
using System.Collections.Generic;
using UnityEngine;

namespace DollShop.Core
{
    /// <summary>
    /// 의뢰 생성·수선 판정·완료 집계.
    /// 수치는 OrderTemplatesSO에서 읽는다.
    /// </summary>
    public static class OrderSystem
    {
        // ── 읽기 전용 프로퍼티 ─────────────────────────────
        public static Order? Current    { get; private set; }
        public static int    Remaining  { get; private set; }
        public static int    TotalToday { get; private set; }

        // ── 이벤트 ─────────────────────────────────────────
        public static event Action<Order>       OnOrderIssued;   // 새 주문서 발행
        public static event Action<OrderResult> OnOrderJudged;   // 수선 판정 완료
        public static event Action              OnAllOrdersDone; // 그날 의뢰 전부 완료

        // ── 내부 상태 ───────────────────────────────────────
        private static readonly List<OrderRecord> _records = new List<OrderRecord>();
        private static int _nextOrderId = 1;
        private static OrderTemplatesSO _templatesSO;
        private static EconomySystem.Config _economyConfig;

        // ── 초기화 ──────────────────────────────────────────
        public static void Initialize(OrderTemplatesSO templatesSO)
        {
            _templatesSO = templatesSO;
        }

        /// <summary>하루 시작 시 호출. 그날 의뢰 큐를 준비한다.</summary>
        public static void BeginDay(int orderCount, int seed)
        {
            _records.Clear();
            Current   = null;
            TotalToday = orderCount;
            Remaining  = orderCount;
            GameState.OrdersDone = 0;
            // TODO: W2 — seed 기반으로 templatesSO에서 의뢰 N건 생성
            IssueNext();
        }

        /// <summary>다음 의뢰를 발행한다.</summary>
        public static void IssueNext()
        {
            if (_templatesSO == null) { Debug.LogWarning("[OrderSystem] templatesSO가 없습니다."); return; }
            if (Remaining <= 0) return;

            // TODO: W2 — 실제 템플릿 풀에서 생성
            var dummyOrder = new Order(
                new OrderId(_nextOrderId++),
                "VAR_BEAR_01",
                MinigameType.SEAM,
                new[] { new RepairRequirement("seam_left", "stitch") },
                25f
            );

            Current = dummyOrder;
            OnOrderIssued?.Invoke(dummyOrder);
        }

        /// <summary>수선 결과 제출. View가 미니게임 완료 후 호출한다.</summary>
        public static OrderResult SubmitRepair(OrderId id, RepairInput input)
        {
            var result = JudgeRepair(input);

            // 골드 처리
            EconomySystem.AddGold(result.Reward, result.Complaint ? -30 : 0);

            Current = null;
            Remaining--;
            GameState.OrdersDone++;

            OnOrderJudged?.Invoke(result);

            if (Remaining <= 0)
            {
                OnAllOrdersDone?.Invoke();
                DayTimer.ForceComplete();
            }
            else
            {
                IssueNext();
            }

            return result;
        }

        // ── 판정 로직 (§2.8 계산식) ─────────────────────────
        private static OrderResult JudgeRepair(RepairInput input)
        {
            if (input.Aborted)
                return new OrderResult(false, 0, false);

            const int BASE_PRICE = 40;
            float accuracyFactor = GetAccuracyFactor(input.Accuracy);
            float speedFactor    = GetSpeedFactor(input.Seconds);
            int   reward         = Mathf.RoundToInt(BASE_PRICE * accuracyFactor * speedFactor);
            bool  complaint      = input.Accuracy < 60f;

            return new OrderResult(!complaint, reward, complaint);
        }

        private static float GetAccuracyFactor(float acc)
        {
            // §2.8 정확도계수
            if (acc >= 95f) return 1.5f;
            if (acc >= 80f) return 1.2f;
            if (acc >= 60f) return 1.0f;
            return 0.6f;
        }

        private static float GetSpeedFactor(float seconds)
        {
            // §2.8 속도계수
            if (seconds <= 25f) return 1.2f;
            if (seconds <= 40f) return 1.0f;
            return 0.8f;
        }
    }
}
