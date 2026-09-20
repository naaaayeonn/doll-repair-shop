using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DollShop.View
{
    /// <summary>
    /// SEAM — 봉제선 드래그 미니게임.
    /// Catmull-Rom 스플라인, 허용 오차 40px, 단조 진행도.
    /// </summary>
    public class SeamMinigame : MinigameBase
    {
        [Header("스플라인")]
        [SerializeField] private List<Transform> _controlPoints = new List<Transform>();
        [SerializeField] private float           _tolerancePx   = 40f; // DESK_LAMP 시 +15%
        [SerializeField] private int             _segments      = 60;

        [Header("시각 피드백")]
        [SerializeField] private LineRenderer _threadLine;
        [SerializeField] private Color        _lineNormal = Color.white;
        [SerializeField] private Color        _lineError  = Color.red;

        // 내부 상태
        private float   _progress     = 0f; // 0~1 단조 증가
        private int     _totalFrames  = 0;
        private int     _errorFrames  = 0;
        private bool    _dragging     = false;
        private Vector2 _prevTouch    = Vector2.zero;

        protected override void OnBegin()
        {
            _progress    = 0f;
            _totalFrames = 0;
            _errorFrames = 0;
            _dragging    = false;

            // TODO: W2 — 컨트롤 포인트 기반 스플라인 생성
            // TODO: W2 — DESK_LAMP 아이템 보유 시 _tolerancePx *= 1.15f
        }

        protected override void OnAbort() { }

        protected override void Update()
        {
            base.Update();
            if (!IsActive) return;

            // TODO: W2 — 터치 드래그 추적, 진행도 업데이트, 정확도 계산
            // TODO: W2 — 이탈 누적 30% 초과 시 실패 → 재시작
        }

        // ── Catmull-Rom 스플라인 ─────────────────────────────
        private Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            return 0.5f * (
                2f * p1 +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
            );
        }
    }
}
