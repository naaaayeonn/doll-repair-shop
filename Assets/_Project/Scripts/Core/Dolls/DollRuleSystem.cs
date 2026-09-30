using System;
using System.Collections.Generic;
using UnityEngine;

namespace DollShop.Core
{
    /// <summary>
    /// 인형 3종 상태머신 + 공정성 규칙 강제. W3(RABBIT)+W4(OCTOPUS,BEAR) 완성.
    /// - 공정성: Stage 1 진입 시 OnClueEmitted 반드시 발행, Stage 1→0 최소 2틱 보장.
    /// - BEAR 특수: 재단칼 미치움 시 moveInterval 절반 (급가속).
    /// - TOOL 대응: 가위 미보유 시 FailReason = "NO_TOOL".
    /// - PREVENT 대응: Stage 2 이전에 knife를 치워야 성공.
    /// </summary>
    public static class DollRuleSystem
    {
        // ── 이벤트 ─────────────────────────────────────────
        public static event Action<DollId, DollStage> OnStageChanged;
        public static event Action<ClueInfo>          OnClueEmitted;
        public static event Action<DollId, bool>      OnCounterResult;
        public static event Action<DollId>            OnAttack;

        // ── 읽기 전용 API ───────────────────────────────────
        public static DollStage StageOf(DollId id)       => GetState(id).Stage;
        public static bool      IsNeutralized(DollId id) => GetState(id).Neutralized;
        public static bool      IsActive(DollId id)      => GetState(id).Active;

        // ── 내부 상태 ───────────────────────────────────────
        private class DollState
        {
            public DollId      Id;
            public DollStage   Stage           = DollStage.Idle;
            public bool        Active          = false;
            public bool        Neutralized     = false;
            public int         MoveInterval    = 8;
            public int         IntervalCounter = 8;
            public CounterType CounterType     = CounterType.IMMEDIATE;
            public int         ClueStage       = 1;
            public int         AttackDelayTick = 2;
            public int         AttackGuard     = 0;
        }

        private static readonly Dictionary<DollId, DollState> _states =
            new Dictionary<DollId, DollState>
            {
                { DollId.RABBIT,  new DollState { Id = DollId.RABBIT  } },
                { DollId.OCTOPUS, new DollState { Id = DollId.OCTOPUS } },
                { DollId.BEAR,    new DollState { Id = DollId.BEAR    } },
            };

        private static DollRulesSO _rulesSO;
        // WALL_CLOCK 아이템: Stage 변화 시 HUD 방향 힌트 표시 플래그
        private static bool _wallClockActive = false;
        public static void SetWallClockActive(bool active) => _wallClockActive = active;

        // ── 초기화 ──────────────────────────────────────────
        public static void Initialize(DollRulesSO rulesSO)
        {
            _rulesSO = rulesSO;
            DayTimer.OnTick += ProcessTick;
        }

        public static void BeginDay(int day)
        {
            foreach (var state in _states.Values)
            {
                state.Stage       = DollStage.Idle;
                state.Active      = false;
                state.Neutralized = false;
                state.AttackGuard = 0;
            }

            if (_rulesSO == null || _rulesSO.entries == null)
            {
                Debug.LogWarning("[DollRuleSystem] rulesSO가 없습니다.");
                return;
            }

            // rulesSO에서 해당 day 설정 읽어 각 인형 초기화 (W3 구현)
            foreach (DollId dollId in System.Enum.GetValues(typeof(DollId)))
            {
                var entry = _rulesSO.GetRule(dollId.ToString(), day);
                if (entry == null || !entry.active) continue;

                var state = GetState(dollId);
                state.Active          = true;
                state.Stage           = (DollStage)entry.startStage;
                state.MoveInterval    = entry.moveInterval;
                state.IntervalCounter = entry.moveInterval;
                state.AttackDelayTick = entry.attackDelayTick;
                state.ClueStage       = entry.clueStage;
                state.CounterType     = entry.counterType switch
                {
                    "IMMEDIATE" => CounterType.IMMEDIATE,
                    "TOOL"      => CounterType.TOOL,
                    "PREVENT"   => CounterType.PREVENT,
                    _           => CounterType.IMMEDIATE
                };
            }
        }

        // ── 매 틱 처리 (§6.2 의사코드 완전 구현) ─────────────
        private static void ProcessTick(int tick)
        {
            foreach (var state in _states.Values)
            {
                if (!state.Active || state.Neutralized) continue;

                // AttackGuard 틱 감소 (Stage 1에서 독립적으로 감소)
                if (state.Stage == DollStage.AtDoor && state.AttackGuard > 0)
                    state.AttackGuard--;

                int moveInterval = state.MoveInterval;

                // BEAR 특수 규칙: 재단칼 미치움 시 moveInterval 절반 (§2.4)
                if (state.Id == DollId.BEAR && !ToolState.IsKnifeStored)
                    moveInterval = Mathf.Max(1, moveInterval / 2);

                state.IntervalCounter--;

                if (state.IntervalCounter > 0) continue;

                // 카운터 리셋
                state.IntervalCounter = moveInterval;

                DollStage prevStage = state.Stage;
                DollStage newStage  = (DollStage)Mathf.Max(0, (int)state.Stage - 1);

                if (newStage == DollStage.Approaching && prevStage == DollStage.Idle)
                {
                    // Stage 3→2: 1차 단서 (clueStage가 2인 경우 — BEAR)
                    state.Stage = newStage;
                    OnStageChanged?.Invoke(state.Id, newStage);

                    if (state.ClueStage >= 2)
                    {
                        var clue = BuildClueInfo(state, ClueChannel.SOUND);
                        OnClueEmitted?.Invoke(clue);
                    }
                }
                else if (newStage == DollStage.AtDoor && prevStage != DollStage.AtDoor)
                {
                    // Stage 2→1: 공정성 핵심 — 반드시 OnClueEmitted 발행 + AttackGuard 설정
                    state.Stage       = DollStage.AtDoor;
                    state.AttackGuard = state.AttackDelayTick; // 기본 2틱

                    OnStageChanged?.Invoke(state.Id, DollStage.AtDoor);

                    // 공정성 단서 발행 (반드시!)
                    var clue = BuildClueInfo(state, ClueChannel.SOUND);
                    OnClueEmitted?.Invoke(clue);

                    // 정신력 패널티
                    SanitySystem.PenalizeDoorStage();
                }
                else if (newStage == DollStage.Attack)
                {
                    // AttackGuard가 남아 있으면 습격 금지 (2틱 보장)
                    if (state.AttackGuard > 0)
                    {
                        // Stage 유지, 다음 틱에 다시 시도
                        state.IntervalCounter = 1; // 1틱 후 재시도
                    }
                    else
                    {
                        state.Stage = DollStage.Attack;
                        OnStageChanged?.Invoke(state.Id, DollStage.Attack);
                        OnAttack?.Invoke(state.Id);
                        DayTimer.ForceDeath();
                    }
                }
                else if (newStage != prevStage)
                {
                    state.Stage = newStage;
                    OnStageChanged?.Invoke(state.Id, newStage);
                }
            }
        }

        private static ClueInfo BuildClueInfo(DollState state, ClueChannel channel)
            => new ClueInfo(state.Id, channel, $"hint.{state.Id}.{channel}");

        // ── 대응 시도 (View → Core) — W4 3종 완성 ───────────
        public static CounterResult TryCounter(DollId doll, CounterType type)
        {
            var state = GetState(doll);

            if (!state.Active)
                return new CounterResult(false, "NOT_ACTIVE");
            if (state.Neutralized)
                return new CounterResult(false, "ALREADY_NEUTRALIZED");

            // 잘못된 대응 유형 → 정신력 패널티
            if (type != state.CounterType)
            {
                SanitySystem.PenalizeCounterFail();
                OnCounterResult?.Invoke(doll, false);
                return new CounterResult(false, "WRONG_TYPE");
            }

            switch (type)
            {
                case CounterType.IMMEDIATE:
                    // RABBIT: Stage 1(AtDoor)에서만 실로 묶기 성공
                    if (state.Stage != DollStage.AtDoor)
                    {
                        OnCounterResult?.Invoke(doll, false);
                        return new CounterResult(false, "WRONG_TIMING");
                    }
                    break;

                case CounterType.TOOL:
                    // OCTOPUS: 가위를 미리 챙겨야 성공
                    if (!ToolState.HasTool(ToolId.SCISSORS))
                    {
                        OnCounterResult?.Invoke(doll, false);
                        return new CounterResult(false, "NO_TOOL");
                    }
                    // OCTOPUS는 AtDoor 또는 Approaching에서 대응 가능
                    if (state.Stage == DollStage.Idle)
                    {
                        OnCounterResult?.Invoke(doll, false);
                        return new CounterResult(false, "WRONG_TIMING");
                    }
                    // 가위 소비
                    ToolState.TryDrop(ToolId.SCISSORS);
                    break;

                case CounterType.PREVENT:
                    // BEAR: Stage 2(Approaching) 이전에 재단칼을 치워야 성공
                    // TryCounter는 "지금 당장 치우기" 시도로 처리
                    if (!ToolState.TryStoreKnife())
                    {
                        // 이미 치워져 있으면 성공 (예방 완료 상태)
                        if (ToolState.IsKnifeStored)
                            break;
                        OnCounterResult?.Invoke(doll, false);
                        return new CounterResult(false, "ALREADY_STORED");
                    }
                    break;
            }

            // 성공 → 그날 무력화, Stage 3(Idle) 복귀
            state.Neutralized     = true;
            state.Stage           = DollStage.Idle;
            state.IntervalCounter = state.MoveInterval;
            state.AttackGuard     = 0;
            OnCounterResult?.Invoke(doll, true);
            OnStageChanged?.Invoke(doll, DollStage.Idle);
            return new CounterResult(true);
        }

        private static DollState GetState(DollId id) => _states[id];
    }
}
