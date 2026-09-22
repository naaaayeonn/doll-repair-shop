using UnityEngine;
using DollShop.Core;

namespace DollShop.View
{
    /// <summary>정신력·틱·인형 Stage를 종합해 BGM 긴장도를 자동 전환.</summary>
    public class TensionMixer : MonoBehaviour
    {
        [SerializeField] private AudioService _audioService;

        private void OnEnable()
        {
            DayTimer.OnTick               += UpdateTension;
            SanitySystem.OnChanged        += (_, __) => UpdateTension(DayTimer.CurrentTick);
            DollRuleSystem.OnStageChanged += (_, __) => UpdateTension(DayTimer.CurrentTick);
        }

        private void OnDisable()
        {
            DayTimer.OnTick -= UpdateTension;
        }

        private void UpdateTension(int tick)
        {
            if (_audioService == null) return;

            float tickRatio   = (float)tick / Mathf.Max(1, DayTimer.MaxTick);
            float sanityRatio = SanitySystem.Current / SanitySystem.Max;

            // 인형 최소 Stage (낮을수록 위험)
            int minStage = 3;
            foreach (DollId doll in System.Enum.GetValues(typeof(DollId)))
            {
                int s = (int)DollRuleSystem.StageOf(doll);
                if (s < minStage) minStage = s;
            }

            float tension = Mathf.Clamp01(
                tickRatio * 0.3f +
                (1f - sanityRatio) * 0.4f +
                (1f - (minStage / 3f)) * 0.3f
            );

            _audioService.SetTension(tension);
        }
    }
}
