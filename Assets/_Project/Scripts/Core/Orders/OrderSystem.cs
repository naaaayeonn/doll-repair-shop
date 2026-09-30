using System;
using System.Collections.Generic;
using UnityEngine;

namespace DollShop.Core
{
    /// <summary>
    /// 의뢰 생성·수선 판정·완료 집계. W2 완성.
    /// seed 기반 의뢰 풀 생성, 의뢰 N건 큐잉, 판정 계산식(§2.8).
    /// </summary>
    public static class OrderSystem
    {
        // ── 읽기 전용 프로퍼티 ─────────────────────────────
        public static Order? Current    { get; private set; }
        public static int    Remaining  { get; private set; }
        public static int    TotalToday { get; private set; }

        // ── 이벤트 ─────────────────────────────────────────
        public static event Action<Order>       OnOrderIssued;
        public static event Action<OrderResult> OnOrderJudged;
        public static event Action              OnAllOrdersDone;

        // ── 내부 상태 ───────────────────────────────────────
        private static readonly List<Order> _queue = new List<Order>();
        private static int _nextOrderId = 1;
        private static OrderTemplatesSO _templatesSO;

        // DESK_LAMP 효과: 속도 판정 관대화 (+15% 허용 오차 → 속도 계수 경계 완화)
        private static bool _deskLampActive = false;
        public static void SetDeskLampActive(bool active) => _deskLampActive = active;

        public static void Initialize(OrderTemplatesSO templatesSO)
        {
            _templatesSO = templatesSO;
        }

        /// <summary>하루 시작. seed 기반 의뢰 N건을 큐에 생성한다.</summary>
        public static void BeginDay(int orderCount, int seed)
        {
            _queue.Clear();
            Current    = null;
            TotalToday = orderCount;
            Remaining  = orderCount;
            GameState.OrdersDone = 0;

            if (_templatesSO == null || _templatesSO.entries == null || _templatesSO.entries.Length == 0)
            {
                Debug.LogWarning("[OrderSystem] templatesSO가 없거나 비어 있습니다. 더미 의뢰를 사용합니다.");
                for (int i = 0; i < orderCount; i++)
                    _queue.Add(MakeDummyOrder());
            }
            else
            {
                // seed 고정 셔플로 의뢰 풀 생성 (결정성 보장)
                var rng = new System.Random(seed);
                var pool = new List<OrderTemplatesSO.Entry>(_templatesSO.entries);

                for (int i = 0; i < orderCount; i++)
                {
                    int idx  = rng.Next(pool.Count);
                    var tmpl = pool[idx];
                    _queue.Add(BuildOrder(tmpl));
                    // 같은 템플릿 연속 방지를 위해 일시 제거 후 순환
                    pool.RemoveAt(idx);
                    if (pool.Count == 0)
                        pool.AddRange(_templatesSO.entries);
                }
            }

            IssueNext();
        }

        public static void IssueNext()
        {
            if (Remaining <= 0 || _queue.Count == 0) return;
            Current = _queue[0];
            _queue.RemoveAt(0);
            OnOrderIssued?.Invoke(Current.Value);
        }

        /// <summary>수선 결과 제출. §2.8 계산식 적용.</summary>
        public static OrderResult SubmitRepair(OrderId id, RepairInput input)
        {
            var result = JudgeRepair(input);

            int complainDeduct = result.Complaint ? -30 : 0;
            EconomySystem.AddGold(result.Reward, complainDeduct);

            // 수선 성공(정확도 90 이상) → 정신력 보너스
            if (!input.Aborted && input.Accuracy >= 90f)
                SanitySystem.BonusRepair();

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

        // ── 판정 로직 (§2.8) ────────────────────────────────
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
            if (acc >= 95f) return 1.5f;
            if (acc >= 80f) return 1.2f;
            if (acc >= 60f) return 1.0f;
            return 0.6f;
        }

        private static float GetSpeedFactor(float seconds)
        {
            // DESK_LAMP: 허용 오차 +15% → 경계값을 1.15배 늘림
            float mult = _deskLampActive ? 1.15f : 1.0f;
            if (seconds <= 25f * mult) return 1.2f;
            if (seconds <= 40f * mult) return 1.0f;
            return 0.8f;
        }

        // ── 의뢰 생성 헬퍼 ──────────────────────────────────
        private static Order BuildOrder(OrderTemplatesSO.Entry tmpl)
        {
            MinigameType mg = tmpl.minigameType switch
            {
                "SEAM"   => MinigameType.SEAM,
                "BUTTON" => MinigameType.BUTTON,
                "CUT"    => MinigameType.CUT,
                _        => MinigameType.SEAM
            };

            // 요구사항: requirementCount만큼 파트 키 생성
            var reqs = new RepairRequirement[tmpl.requirementCount];
            for (int i = 0; i < tmpl.requirementCount; i++)
                reqs[i] = new RepairRequirement($"part_{i}", mg.ToString().ToLower());

            return new Order(
                new OrderId(_nextOrderId++),
                tmpl.dollVariant,
                mg,
                reqs,
                tmpl.targetSeconds
            );
        }

        private static Order MakeDummyOrder() =>
            new Order(
                new OrderId(_nextOrderId++),
                "VAR_BEAR_01",
                MinigameType.SEAM,
                new[] { new RepairRequirement("seam_left", "stitch") },
                25f
            );
    }
}
