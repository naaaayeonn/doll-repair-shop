using UnityEngine;

namespace DollShop.Core
{
    /// <summary>
    /// 플레이어의 시간 소모 행동을 Core에 알리는 API. W3 완성.
    /// CCTV 감시 중: 해당 방 인형 이동 정지 (DollRuleSystem과 연동).
    /// TOOL_BELT 효과: 이동 비용 1틱 → 0틱.
    /// </summary>
    public static class TimeService
    {
        private static RoomId? _cctvRoom      = null;
        private static DollId? _inspectingDoll = null;
        private static float   _inspectElapsed = 0f;

        // TOOL_BELT 아이템 효과
        private static bool _toolBeltActive = false;
        public static void SetToolBeltActive(bool active) => _toolBeltActive = active;

        /// <summary>현재 CCTV 감시 중인 방. DollRuleSystem이 참조해 이동 정지 여부 판단.</summary>
        public static RoomId? CctvRoom => _cctvRoom;

        /// <summary>방 이동. 기본 1틱 소비. TOOL_BELT 보유 시 0틱.</summary>
        public static bool SpendMove(RoomId to)
        {
            if (!_toolBeltActive)
                DayTimer.SpendTicks(1);
            return true;
        }

        /// <summary>CCTV 감시 시작. 해당 방 인형 이동 정지.</summary>
        public static void BeginCctv(RoomId room)
        {
            _cctvRoom = room;
        }

        /// <summary>CCTV 감시 종료. 0.5~1틱 소비 (중간값 1틱 사용).</summary>
        public static void EndCctv()
        {
            _cctvRoom = null;
            DayTimer.SpendTicks(1);
        }

        /// <summary>3D 인형 살펴보기 시작.</summary>
        public static void BeginInspect(DollId doll)
        {
            _inspectingDoll  = doll;
            _inspectElapsed  = 0f;
        }

        /// <summary>살펴보기 종료. 2~3틱 소비. SIGHT 단서 획득 시도 후 ClueId 반환.</summary>
        public static ClueId EndInspect()
        {
            if (_inspectingDoll == null) return ClueId.Empty;

            DollId doll     = _inspectingDoll.Value;
            _inspectingDoll = null;
            DayTimer.SpendTicks(2); // 2틱 소비 (사양 2~3틱)

            // SIGHT 단서 획득 시도
            string clueId = $"CLUE_{doll}_SIGHT";
            var    id     = new ClueId(clueId);

            bool added = NoteSystem.TryAddClue(id);
            return added ? id : ClueId.Empty;
        }

        /// <summary>미니게임 소요 시간을 틱으로 환산해 DayTimer에 전달.</summary>
        public static void SpendRepairTicks(float seconds)
        {
            // 10초 = 1틱. 소수점 이하 누적은 DayTimer 실시간 흐름이 처리.
            int ticks = Mathf.FloorToInt(seconds / DayTimer.TICK_DURATION);
            if (ticks > 0)
                DayTimer.SpendTicks(ticks);
        }

        public static void SetPaused(bool paused)
        {
            DayTimer.SetPaused(paused);
        }
    }
}
