using System;
using System.Collections.Generic;
using UnityEngine;

namespace DollShop.Core
{
    /// <summary>
    /// 인형 3종 상태머신 + 공정성 규칙 강제.
    /// 공정성: Stage 1 진입 시 OnClueEmitted 강제 발행, Stage 1→0 최소 2틱 보장.
    /// </summary>
    public static class DollRuleSystem
    {
        // ── 이벤트 ─────────────────────────────────────────
        public static event Action<DollId, DollStage> OnStageChanged;   // 스테이지 변화
        public static event Action<ClueInfo>          OnClueEmitted;    // 공정성 단서 노출
        public static event Action<DollId, bool>      OnCounterResult;  // (인형, 성공여부)
        public static event Action<DollId>            OnAttack;         // 습격 = 사망

        // ── 읽기 전용 API ───────────────────────────────────
        public static DollStage StageOf(DollId id)       => GetState(id).Stage;
        public static bool      IsNeutralized(DollId id) => GetState(id).Neutralized;

        // ── 내부 상태 ───────────────────────────────────────
        private class DollState
        {
            public DollId      Id;
            public DollStage   Stage           = DollStage.Idle;
            public bool        Active          = false;
            public bool        Neutralized     = false;
            public int         MoveInterval    = 8;    // 틱 단위
            public int         IntervalCounter = 8;
            public CounterType CounterType     = CounterType.IMMEDIATE;
            public int         ClueStage       = 1;   // 단서 노출 시작 Stage
            public int         AttackGuard     = 0;   // 습격 방지 카운터 (최소 2틱)
        }

        private static readonly Dictionary<DollId, DollState> _states =
            new Dictionary<DollId, DollState>
            {
                { DollId.RABBIT,  new DollState { Id = DollId.RABBIT  } },
                { DollId.OCTOPUS, new DollState { Id = DollId.OCTOPUS } },
                { DollId.BEAR,    new DollState { Id = DollId.BEAR    } },
            };

        private static DollRulesSO _rulesSO;

        // ── 초기화 ──────────────────────────────────────────
        public static void Initialize(DollRulesSO rulesSO)
        {
            _rulesSO = rulesSO;
            DayTimer.OnTick += ProcessTick;
        }

        public static void BeginDay(int day)
        {
            if (_rulesSO == null) { Debug.LogWarning("[DollRuleSystem] rulesSO가 없습니다."); return; }

            foreach (var state in _states.Values)
            {
                state.Stage           = DollStage.Idle;
                state.Active          = false;
                state.Neutralized     = false;
                state.AttackGuard     = 0;
            }

            // TODO: W3 — rulesSO에서 해당 day의 설정을 읽어 각 인형 초기화
        }

        // ── 매 틱 처리 (§6.2 의사코드 구현) ────────────────
        private static void ProcessTick(int tick)
        {
            foreach (var state in _states.Values)
            {
                if (!state.Active || state.Neutralized) continue;

                int moveInterval = state.MoveInterval;

                // BEAR 특수 규칙: 재단칼이 작업대에 있으면 moveInterval 절반
                if (state.Id == DollId.BEAR && !ToolState.IsKnifeStored)
                    moveInterval = Mathf.Max(1, moveInterval / 2);

                state.IntervalCounter--;

                if (state.IntervalCounter <= 0)
                {
                    state.IntervalCounter = moveInterval;

                    DollStage prevStage = state.Stage;
                    int newStageVal = Mathf.Max(0, (int)state.Stage - 1);
                    DollStage newStage = (DollStage)newStageVal;

                    // Stage 1 진입 공정성 규칙
                    if (newStage == DollStage.AtDoor && prevStage != DollStage.AtDoor)
                    {
                        state.Stage = DollStage.AtDoor;
                        state.AttackGuard = 2; // 최소 2틱 보장

                        var clueInfo = new ClueInfo(state.Id, ClueChannel.SOUND, $"hint.{state.Id}.door");
                        OnClueEmitted?.Invoke(clueInfo); // 반드시 발행
                        OnStageChanged?.Invoke(state.Id, DollStage.AtDoor);

                        // 정신력 패널티
                        SanitySystem.PenalizeDoorStage();
                    }
                    else if (newStage == DollStage.Attack)
                    {
                        if (state.AttackGuard > 0)
                        {
                            // 2틱 미만이면 습격 금지
                            state.AttackGuard--;
                            // Stage 유지
                        }
                        else
                        {
                            state.Stage = DollStage.Attack;
                            OnStageChanged?.Invoke(state.Id, DollStage.Attack);
                            OnAttack?.Invoke(state.Id);
                            DayTimer.ForceDeath();
                        }
                    }
                    else
                    {
                        if (state.AttackGuard > 0) state.AttackGuard--;
                        state.Stage = newStage;
                        OnStageChanged?.Invoke(state.Id, newStage);
                    }
                }
            }
        }

        // ── 대응 시도 (View → Core) ──────────────────────────
        public static CounterResult TryCounter(DollId doll, CounterType type)
        {
            var state = GetState(doll);

            if (!state.Active)
                return new CounterResult(false, "NOT_ACTIVE");

            if (state.Neutralized)
                return new CounterResult(false, "ALREADY_NEUTRALIZED");

            // 대응 유형 검증
            if (type != state.CounterType)
            {
                SanitySystem.PenalizeCounterFail();
                OnCounterResult?.Invoke(doll, false);
                return new CounterResult(false, "WRONG_TYPE");
            }

            // TOOL 대응: 가위를 미리 챙겨야 함
            if (type == CounterType.TOOL && !ToolState.HasTool(ToolId.SCISSORS))
            {
                OnCounterResult?.Invoke(doll, false);
                return new CounterResult(false, "NO_TOOL");
            }

            // IMMEDIATE: Stage 1(AtDoor)이어야 함
            if (type == CounterType.IMMEDIATE && state.Stage != DollStage.AtDoor)
            {
                OnCounterResult?.Invoke(doll, false);
                return new CounterResult(false, "WRONG_TIMING");
            }

            // 성공 → 그날 무력화, Stage 3 복귀
            state.Neutralized     = true;
            state.Stage           = DollStage.Idle;
            state.IntervalCounter = state.MoveInterval;
            OnCounterResult?.Invoke(doll, true);
            OnStageChanged?.Invoke(doll, DollStage.Idle);
            return new CounterResult(true);
        }

        private static DollState GetState(DollId id) => _states[id];
    }
}
