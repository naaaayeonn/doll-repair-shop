using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DollShop.Core;

namespace DollShop.View
{
    /// <summary>HUD 전체 관리자. Core 이벤트를 구독해 각 요소를 갱신한다.</summary>
    public class HudController : MonoBehaviour
    {
        [Header("시간")]
        [SerializeField] private TMP_Text _timeText;
        [Header("정신력")]
        [SerializeField] private Slider   _sanitySlider;
        [SerializeField] private Image    _sanityFill;
        [Header("골드")]
        [SerializeField] private TMP_Text _goldText;
        [Header("의뢰")]
        [SerializeField] private TMP_Text _orderText;

        // 정신력 색상 구간
        [SerializeField] private Color _colorNormal  = Color.green;
        [SerializeField] private Color _colorWarning = Color.yellow;
        [SerializeField] private Color _colorDanger  = Color.red;

        private void OnEnable()
        {
            DayTimer.OnTick             += UpdateTime;
            SanitySystem.OnChanged      += UpdateSanity;
            EconomySystem.OnGoldChanged += UpdateGold;
            OrderSystem.OnOrderIssued   += _ => UpdateOrders();
            OrderSystem.OnOrderJudged   += _ => UpdateOrders();
        }

        private void OnDisable()
        {
            DayTimer.OnTick             -= UpdateTime;
            SanitySystem.OnChanged      -= UpdateSanity;
            EconomySystem.OnGoldChanged -= UpdateGold;
            OrderSystem.OnOrderIssued   -= _ => UpdateOrders();
            OrderSystem.OnOrderJudged   -= _ => UpdateOrders();
        }

        private void UpdateTime(int tick)
        {
            // 22:00 기준. 1틱 = 10분 게임 내 시간
            int minutesTotal = 22 * 60 + tick * 10;
            int hour   = (minutesTotal / 60) % 24;
            int minute = minutesTotal % 60;
            if (_timeText) _timeText.text = $"{hour:D2}:{minute:D2}";
        }

        private void UpdateSanity(float current, float max)
        {
            if (_sanitySlider) _sanitySlider.value = current / max;
            if (_sanityFill)
            {
                float ratio = current / max;
                _sanityFill.color = ratio > 0.7f ? _colorNormal :
                                    ratio > 0.4f ? _colorWarning : _colorDanger;
            }
        }

        private void UpdateGold(int newGold, int delta)
        {
            if (_goldText) _goldText.text = $"{newGold:N0}G";
        }

        private void UpdateOrders()
        {
            if (_orderText)
                _orderText.text = $"{OrderSystem.TotalToday - OrderSystem.Remaining}/{OrderSystem.TotalToday}";
        }
    }
}
