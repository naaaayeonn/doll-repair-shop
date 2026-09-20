using NUnit.Framework;
using DollShop.Core;

namespace DollShop.Tests
{
    /// <summary>W1 DoD: 더미 테스트 1개 통과 확인용.</summary>
    public class SmokeTest
    {
        [Test]
        public void Core_Namespace_Accessible()
        {
            // Core 네임스페이스의 공용 타입에 접근 가능한지 확인
            var stage = DollStage.Idle;
            Assert.AreEqual(DollStage.Idle, stage);
        }

        [Test]
        public void GameState_InitialValues()
        {
            // GameState 초기값 확인
            GameState.StartNewRun();
            Assert.AreEqual(1,     GameState.CurrentDay);
            Assert.AreEqual(0,     GameState.Gold);
            Assert.AreEqual(100f,  GameState.Sanity);
        }
    }
}
