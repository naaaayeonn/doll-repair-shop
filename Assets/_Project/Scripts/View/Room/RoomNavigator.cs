using System;
using System.Collections;
using UnityEngine;
using DollShop.Core;

namespace DollShop.View
{
    /// <summary>하단 바 방 이동. 전환 연출 0.3초 이내.</summary>
    public class RoomNavigator : MonoBehaviour
    {
        public static RoomId CurrentRoom { get; private set; } = RoomId.WORKSHOP;

        [SerializeField] private CanvasGroup _roomCanvas;
        [SerializeField] private float       _fadeDuration = 0.15f;

        private bool _transitioning = false;

        public static event Action<RoomId> OnRoomChanged;

        public void NavigateTo(RoomId room)
        {
            if (_transitioning || CurrentRoom == room) return;
            StartCoroutine(DoTransition(room));
        }

        // 편의 메서드 (버튼에서 직접 연결)
        public void GoWorkshop() => NavigateTo(RoomId.WORKSHOP);
        public void GoShowroom() => NavigateTo(RoomId.SHOWROOM);
        public void GoFront()    => NavigateTo(RoomId.FRONT);
        public void GoMyRoom()   => NavigateTo(RoomId.MYROOM);

        private IEnumerator DoTransition(RoomId target)
        {
            _transitioning = true;

            // 페이드 아웃
            yield return StartCoroutine(Tweener.Fade(_roomCanvas, 1f, 0f, _fadeDuration));

            // 방 전환
            bool moved = TimeService.SpendMove(target);
            CurrentRoom = target;
            OnRoomChanged?.Invoke(target);

            // 페이드 인
            yield return StartCoroutine(Tweener.Fade(_roomCanvas, 0f, 1f, _fadeDuration));

            _transitioning = false;
        }
    }
}
