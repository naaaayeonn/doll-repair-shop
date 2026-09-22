using System;
using UnityEngine;

namespace DollShop.Core
{
    /// <summary>
    /// 플레이어의 시간 소모 행동을 Core에 알리는 API.
    /// View는 "했다"만 알린다. 틱 관리는 DayTimer가 한다.
    /// </summary>
    public static class TimeService
    {
        private const float TICK_DURATION = 10f;

        // CCTV 감시 중인 방 (null이면 미감시)
        private static RoomId? _cctvRoom = null;

        /// <summary>현재 CCTV로 감시 중인 방. DollRuleSystem이 이 값을 본다.</summary>
        public static RoomId? CctvRoom => _cctvRoom;

        // 살펴보기 중인 인형
        private static DollId? _inspectingDoll = null;

        /// <summary>방 이동. 1틱 소비.</summary>
        public static bool SpendMove(RoomId to)
        {
            // TODO: W3 — TOOL_BELT 아이템 보유 시 0틱으로 단축
            DayTimer.SpendTicks(1);
            return true;
        }

        /// <summary>CCTV 감시 시작. 그 방 인형 이동 정지.</summary>
        public static void BeginCctv(RoomId room)
        {
            _cctvRoom = room;
            // TODO: W3 — 실시간 0.5~1틱 누적 (현재는 종료 시 처리)
        }

        /// <summary>CCTV 감시 종료.</summary>
        public static void EndCctv()
        {
            // TODO: W3 — 감시 지속 시간 틱 환산 후 소비
            DayTimer.SpendTicks(1); // 임시: 1틱 고정
            _cctvRoom = null;
        }

        /// <summary>3D 인형 살펴보기 시작.</summary>
        public static void BeginInspect(DollId doll)
        {
            _inspectingDoll = doll;
        }

        /// <summary>3D 인형 살펴보기 종료. 단서 반환(없으면 Empty).</summary>
        public static ClueId EndInspect()
        {
            if (_inspectingDoll == null) return ClueId.Empty;

            DollId doll = _inspectingDoll.Value;
            _inspectingDoll = null;
            DayTimer.SpendTicks(2); // 2~3틱. TODO: W3 실제 소요 환산

            // TODO: W5 — NoteSystem에 SIGHT 단서 획득 시도
            return ClueId.Empty;
        }

        /// <summary>미니게임 소요 시간을 틱으로 환산해 소비.</summary>
        public static void SpendRepairTicks(float seconds)
        {
            // TODO: W3 — DayTimer와 연결. 현재 스텁.
        }

        /// <summary>설정/노트 열람용 일시정지.</summary>
        public static void SetPaused(bool paused)
        {
            DayTimer.SetPaused(paused);
        }
    }
}
