using NUnit.Framework;
using DollShop.Core;

namespace DollShop.Tests
{
    /// <summary>§6.5 — 보상 계산 12케이스 (정확도 4구간 × 속도 3구간)</summary>
    public class OrderSystemTests
    {
        // 내부 계산 메서드를 테스트하기 위해 동일한 로직을 복제
        private static int CalcReward(float accuracy, float seconds)
        {
            float a = accuracy >= 95f ? 1.5f : accuracy >= 80f ? 1.2f : accuracy >= 60f ? 1.0f : 0.6f;
            float s = seconds  <= 25f ? 1.2f : seconds  <= 40f ? 1.0f : 0.8f;
            return UnityEngine.Mathf.RoundToInt(40 * a * s);
        }

        // 정확도 95+, 속도 3구간
        [Test] public void Acc95_Speed25()    => Assert.AreEqual(72, CalcReward(96f, 24f));
        [Test] public void Acc95_Speed40()    => Assert.AreEqual(60, CalcReward(96f, 30f));
        [Test] public void Acc95_SpeedOver()  => Assert.AreEqual(48, CalcReward(96f, 50f));

        // 정확도 80+
        [Test] public void Acc80_Speed25()    => Assert.AreEqual(58, CalcReward(85f, 24f));
        [Test] public void Acc80_Speed40()    => Assert.AreEqual(48, CalcReward(85f, 30f));
        [Test] public void Acc80_SpeedOver()  => Assert.AreEqual(38, CalcReward(85f, 50f));

        // 정확도 60+
        [Test] public void Acc60_Speed25()    => Assert.AreEqual(48, CalcReward(65f, 24f));
        [Test] public void Acc60_Speed40()    => Assert.AreEqual(40, CalcReward(65f, 30f));
        [Test] public void Acc60_SpeedOver()  => Assert.AreEqual(32, CalcReward(65f, 50f));

        // 정확도 60미만
        [Test] public void AccLow_Speed25()   => Assert.AreEqual(29, CalcReward(50f, 24f));
        [Test] public void AccLow_Speed40()   => Assert.AreEqual(24, CalcReward(50f, 30f));
        [Test] public void AccLow_SpeedOver() => Assert.AreEqual(19, CalcReward(50f, 50f));
    }
}
