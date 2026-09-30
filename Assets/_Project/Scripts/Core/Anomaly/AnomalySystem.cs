using System;
using System.Collections.Generic;
using UnityEngine;

namespace DollShop.Core
{
    /// <summary>
    /// 이상현상 발생·타이머·미대응 페널티. W3(3종)+W4(6종) 완성.
    /// - 틱당 확률 기반 발생 (seed 고정 가중 풀 방식으로 학습 가능성 보장).
    /// - 미대응 시 정신력 -15 + 해당 인형 moveInterval -1.
    /// </summary>
    public static class AnomalySystem
    {
        public static event Action<AnomalyInfo>     OnRaised;
        public static event Action<AnomalyId, bool> OnResolved;
        public static event Action<AnomalyId, int>  OnTicking;

        private class AnomalyRecord
        {
            public AnomalyId Id;
            public RoomId    Room;
            public int       RemainingTick;
            public int       IntervalPenalty;
            public string    OwnerDoll;
        }

        private static readonly List<AnomalyRecord>        _active  = new List<AnomalyRecord>();
        private static readonly List<AnomalyTableSO.Entry> _dayPool = new List<AnomalyTableSO.Entry>();
        private static AnomalyTableSO _tableSO;

        // STURDY_RACK 효과: SHOWROOM 이상현상 발생 확률 -30%
        private static bool _sturdyRackActive = false;
        public static void SetSturdyRackActive(bool active) => _sturdyRackActive = active;

        // 결정성을 위한 Day별 seed 기반 RNG
        private static System.Random _rng;

        public static void Initialize(AnomalyTableSO tableSO)
        {
            _tableSO = tableSO;
            DayTimer.OnTick += ProcessTick;
        }

        public static void BeginDay(string[] anomalyIds)
        {
            _active.Clear();
            _dayPool.Clear();

            if (_tableSO == null || anomalyIds == null) return;

            foreach (var id in anomalyIds)
            {
                if (id == "ALL")
                {
                    _dayPool.AddRange(_tableSO.entries);
                    break;
                }
                var entry = _tableSO.GetEntry(id);
                if (entry != null) _dayPool.Add(entry);
            }

            // Day 기반 seed (같은 Day는 항상 유사한 이상현상 — 학습 가능성)
            _rng = new System.Random(GameState.CurrentDay * 1000 + 42);
        }

        private static void ProcessTick(int tick)
        {
            // 활성 이상현상 타이머 처리
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var rec = _active[i];
                rec.RemainingTick--;
                OnTicking?.Invoke(rec.Id, rec.RemainingTick);

                if (rec.RemainingTick <= 0)
                {
                    // 미대응 패널티
                    SanitySystem.PenalizeAnomalyMiss();
                    OnResolved?.Invoke(rec.Id, false);
                    _active.RemoveAt(i);
                }
            }

            // 새 이상현상 발생 판정 (활성 이상현상이 2개 미만일 때)
            if (_active.Count >= 2 || _dayPool.Count == 0) return;

            foreach (var entry in _dayPool)
            {
                // 이미 활성 중인 이상현상은 스킵
                bool alreadyActive = false;
                var  entryId       = new AnomalyId(entry.anomalyId);
                foreach (var a in _active)
                    if (a.Id == entryId) { alreadyActive = true; break; }
                if (alreadyActive) continue;

                float chance = entry.triggerChancePerTick;

                // STURDY_RACK: SHOWROOM 확률 -30%
                if (_sturdyRackActive && entry.room == "SHOWROOM")
                    chance *= 0.7f;

                if ((float)_rng.NextDouble() < chance)
                {
                    RoomId room = entry.room switch
                    {
                        "SHOWROOM" => RoomId.SHOWROOM,
                        "WORKSHOP" => RoomId.WORKSHOP,
                        "FRONT"    => RoomId.FRONT,
                        _          => RoomId.SHOWROOM
                    };

                    var rec = new AnomalyRecord
                    {
                        Id              = entryId,
                        Room            = room,
                        RemainingTick   = entry.limitTick,
                        IntervalPenalty = entry.intervalPenalty,
                        OwnerDoll       = entry.ownerDoll
                    };
                    _active.Add(rec);

                    var info = new AnomalyInfo(entryId, room, entry.limitTick);
                    OnRaised?.Invoke(info);
                    break; // 틱당 최대 1개 발생
                }
            }
        }

        public static bool TryResolve(AnomalyId id)
        {
            for (int i = 0; i < _active.Count; i++)
            {
                if (_active[i].Id == id)
                {
                    _active.RemoveAt(i);
                    OnResolved?.Invoke(id, true);

                    // TRACE 단서 획득 시도
                    string idStr = id.ToString();
                    if (idStr.Contains("RABBIT"))
                        NoteSystem.TryAddClue(new ClueId("CLUE_RABBIT_TRACE"));
                    else if (idStr.Contains("OCTO"))
                        NoteSystem.TryAddClue(new ClueId("CLUE_OCTO_TRACE"));
                    else if (idStr.Contains("BEAR"))
                        NoteSystem.TryAddClue(new ClueId("CLUE_BEAR_TRACE"));

                    return true;
                }
            }
            return false;
        }

        /// <summary>현재 방에 활성 이상현상이 있는지 확인.</summary>
        public static bool HasActiveAnomalyInRoom(RoomId room)
        {
            foreach (var a in _active)
                if (a.Room == room) return true;
            return false;
        }
    }
}
