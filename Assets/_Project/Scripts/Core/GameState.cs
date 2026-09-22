using System.Collections.Generic;
using UnityEngine;

namespace DollShop.Core
{
    /// <summary>
    /// 게임의 단일 진실 공급원(Single Source of Truth).
    /// 현재 Day·골드·정신력·RunId를 한 곳에서 조회한다.
    /// 직접 수정은 각 System을 통해서만 한다.
    /// </summary>
    public static class GameState
    {
        // ── Run-Scoped (루프 시 초기화) ─────────────────────
        public static int   CurrentDay   { get; internal set; } = 1;
        public static int   Gold         { get; internal set; } = 0;
        public static float Sanity       { get; internal set; } = 100f;
        public static int   FaintCount   { get; internal set; } = 0;
        public static int   OrdersDone   { get; internal set; } = 0;
        public static int   CurrentTick  { get; internal set; } = 0;

        // ── Loop-Persistent (루프 후에도 유지) ──────────────
        public static LoopPersistentData Persistent { get; private set; } = new LoopPersistentData();

        // ── 식별자 ──────────────────────────────────────────
        public static System.Guid RunId { get; private set; } = System.Guid.NewGuid();

        internal static void StartNewRun()
        {
            CurrentDay  = 1;
            Gold        = 0;
            Sanity      = 100f;
            FaintCount  = 0;
            OrdersDone  = 0;
            CurrentTick = 0;
            RunId       = System.Guid.NewGuid();
        }

        internal static void SetPersistent(LoopPersistentData data)
        {
            Persistent = data;
        }
    }

    /// <summary>루프를 넘어 유지되는 데이터. 플레이어의 "지식"이다.</summary>
    [System.Serializable]
    public class LoopPersistentData
    {
        public List<string> UnlockedClues    = new List<string>();
        public List<string> SeenCutscenes    = new List<string>();
        public int          DeathCount       = 0;
        public int          MaxDayReached    = 0;
        // 인형별 대응법 해금 (UI 힌트용)
        public List<string> UnlockedCounters = new List<string>();
    }
}
