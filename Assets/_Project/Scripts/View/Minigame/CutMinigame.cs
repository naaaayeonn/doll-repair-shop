using UnityEngine;

namespace DollShop.View
{
    /// <summary>
    /// CUT — 가위 자르기 미니게임.
    /// 경로 추종 + 속도 판정 (400~900 px/s).
    /// </summary>
    public class CutMinigame : MinigameBase
    {
        [SerializeField] private float _minSpeedPxSec = 400f;
        [SerializeField] private float _maxSpeedPxSec = 900f;

        // TODO: W4 구현
        protected override void OnBegin()  { /* TODO: W4 */ }
        protected override void OnAbort() { }
    }
}
