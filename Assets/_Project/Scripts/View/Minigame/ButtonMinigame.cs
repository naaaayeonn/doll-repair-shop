using UnityEngine;
using UnityEngine.UI;

namespace DollShop.View
{
    /// <summary>
    /// BUTTON — 단추 눈 배치 미니게임.
    /// 드래그 스냅(60px) + 1.2초 홀드 + 눈 깜빡임 연출.
    /// </summary>
    public class ButtonMinigame : MinigameBase
    {
        [SerializeField] private float _snapRadiusPx = 60f;
        [SerializeField] private float _holdDuration = 1.2f;

        // TODO: W4 구현
        protected override void OnBegin()  { /* TODO: W4 */ }
        protected override void OnAbort() { }
    }
}
