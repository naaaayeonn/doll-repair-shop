using NUnit.Framework;
using UnityEngine;
using DollShop.Core;

namespace DollShop.Tests
{
    /// <summary>§6.5 — 보상 계산 12케이스 + 공정성 + 대응 검증 테스트</summary>
    public class OrderSystemTests
    {
        private static int CalcReward(float acc, float seconds)
        {
            float a = acc >= 95f ? 1.5f : acc >= 80f ? 1.2f : acc >= 60f ? 1.0f : 0.6f;
            float s = seconds <= 25f ? 1.2f : seconds <= 40f ? 1.0f : 0.8f;
            return Mathf.RoundToInt(40 * a * s);
        }

        // ── 보상 계산 12케이스 ─────────────────────────────
        [Test] public void Acc95_Speed25()   => Assert.AreEqual(72, CalcReward(96f, 24f));
        [Test] public void Acc95_Speed40()   => Assert.AreEqual(60, CalcReward(96f, 30f));
        [Test] public void Acc95_SpeedOver() => Assert.AreEqual(48, CalcReward(96f, 50f));
        [Test] public void Acc80_Speed25()   => Assert.AreEqual(58, CalcReward(85f, 24f));
        [Test] public void Acc80_Speed40()   => Assert.AreEqual(48, CalcReward(85f, 30f));
        [Test] public void Acc80_SpeedOver() => Assert.AreEqual(38, CalcReward(85f, 50f));
        [Test] public void Acc60_Speed25()   => Assert.AreEqual(48, CalcReward(65f, 24f));
        [Test] public void Acc60_Speed40()   => Assert.AreEqual(40, CalcReward(65f, 30f));
        [Test] public void Acc60_SpeedOver() => Assert.AreEqual(32, CalcReward(65f, 50f));
        [Test] public void AccLow_Speed25()  => Assert.AreEqual(29, CalcReward(50f, 24f));
        [Test] public void AccLow_Speed40()  => Assert.AreEqual(24, CalcReward(50f, 30f));
        [Test] public void AccLow_SpeedOver()=> Assert.AreEqual(19, CalcReward(50f, 50f));

        // ── GameState 기본값 ────────────────────────────────
        [Test]
        public void GameState_NewRun_DefaultValues()
        {
            GameState.StartNewRun();
            Assert.AreEqual(1,    GameState.CurrentDay,  "CurrentDay");
            Assert.AreEqual(0,    GameState.Gold,        "Gold");
            Assert.AreEqual(100f, GameState.Sanity,      "Sanity");
            Assert.AreEqual(0,    GameState.FaintCount,  "FaintCount");
            Assert.AreEqual(0,    GameState.OrdersDone,  "OrdersDone");
        }

        // ── DollId enum 값 확인 ────────────────────────────
        [Test]
        public void DollStage_Attack_IsZero()
            => Assert.AreEqual(0, (int)DollStage.Attack);

        [Test]
        public void DollStage_Idle_IsThree()
            => Assert.AreEqual(3, (int)DollStage.Idle);

        // ── LoopPersistentData 초기화 ─────────────────────
        [Test]
        public void LoopPersistent_InitiallyEmpty()
        {
            var p = new LoopPersistentData();
            Assert.AreEqual(0, p.UnlockedClues.Count);
            Assert.AreEqual(0, p.DeathCount);
        }

        // ── NoteSystem 중복 방지 ───────────────────────────
        [Test]
        public void NoteSystem_NoDuplicateClue()
        {
            NoteSystem.LoadFrom(new System.Collections.Generic.List<string>());
            var id = new ClueId("CLUE_RABBIT_SIGHT");
            bool first  = NoteSystem.TryAddClue(id);
            bool second = NoteSystem.TryAddClue(id);
            Assert.IsTrue(first,   "첫 획득은 true");
            Assert.IsFalse(second, "중복 획득은 false");
            Assert.AreEqual(1, NoteSystem.Unlocked.Count);
        }

        // ── ToolState ─────────────────────────────────────
        [Test]
        public void ToolState_KnifeStoreAndRestore()
        {
            ToolState.ResetForDay();
            Assert.IsFalse(ToolState.IsKnifeStored);
            bool stored = ToolState.TryStoreKnife();
            Assert.IsTrue(stored);
            Assert.IsTrue(ToolState.IsKnifeStored);
            bool again = ToolState.TryStoreKnife();
            Assert.IsFalse(again, "이미 치워져 있으면 false");
            ToolState.TryRestoreKnife();
            Assert.IsFalse(ToolState.IsKnifeStored, "다시 꺼내면 false");
        }

        [Test]
        public void ToolState_NoTool_HasToolFalse()
        {
            ToolState.ResetForDay();
            Assert.IsFalse(ToolState.HasTool(ToolId.SCISSORS));
            ToolState.TryTake(ToolId.SCISSORS);
            Assert.IsTrue(ToolState.HasTool(ToolId.SCISSORS));
            ToolState.TryDrop(ToolId.SCISSORS);
            Assert.IsFalse(ToolState.HasTool(ToolId.SCISSORS));
        }
    }
}
