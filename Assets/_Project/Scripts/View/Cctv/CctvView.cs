using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DollShop.Core;

namespace DollShop.View
{
    /// <summary>CCTV 화면. 방 탭, 상태 스프라이트, 노이즈 셰이더.</summary>
    public class CctvView : MonoBehaviour
    {
        [SerializeField] private Image    _displayImage;
        [SerializeField] private TMP_Text _timestampText;
        [SerializeField] private Material _noiseMaterial;        // 노이즈 셰이더
        [SerializeField] private Image    _showroomTabWarning;
        [SerializeField] private Image    _frontTabWarning;

        private RoomId _currentRoom = RoomId.SHOWROOM;
        private bool   _isOpen      = false;

        private void OnEnable()
        {
            DollRuleSystem.OnClueEmitted += ShowWarning;
        }

        private void OnDisable()
        {
            DollRuleSystem.OnClueEmitted -= ShowWarning;
        }

        public void Open()
        {
            _isOpen = true;
            gameObject.SetActive(true);
            TimeService.BeginCctv(_currentRoom);
            // TODO: W3 — 노이즈 셰이더 시작, 스프라이트 갱신
        }

        public void Close()
        {
            _isOpen = false;
            gameObject.SetActive(false);
            TimeService.EndCctv();
        }

        public void SwitchTab(string roomId)
        {
            // TODO: W3 — 0.15초 노이즈 전환, 방 스프라이트 교체
        }

        private void ShowWarning(ClueInfo info)
        {
            // TODO: W3 — 해당 인형의 방 탭에 붉은 점 + 소리
        }
    }
}
