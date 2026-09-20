using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DollShop.View
{
    /// <summary>
    /// 터치 입력 래퍼. 탭/드래그/홀드/핀치 4종 제스처.
    /// 드래그와 탭 구분: 이동 12px 이상 OR 0.15초 이상 → 드래그.
    /// </summary>
    public class TouchInput : MonoBehaviour
    {
        // 임계값
        [SerializeField] private float _dragThresholdPx  = 12f;
        [SerializeField] private float _holdThresholdSec = 0.15f;
        [SerializeField] private float _holdMinSec       = 0.5f;

        // 이벤트
        public static event Action<Vector2>          OnTap;
        public static event Action<Vector2>          OnDragBegin;
        public static event Action<Vector2, Vector2> OnDragMove;  // (pos, delta)
        public static event Action<Vector2>          OnDragEnd;
        public static event Action<Vector2>          OnHoldBegin;
        public static event Action<Vector2>          OnHoldEnd;
        public static event Action<float>            OnPinch;     // 핀치 스케일 배율

        // 내부 상태
        private bool    _touching      = false;
        private bool    _isDragging    = false;
        private bool    _isHolding     = false;
        private Vector2 _startPos      = Vector2.zero;
        private Vector2 _prevPos       = Vector2.zero;
        private float   _touchTime     = 0f;
        private float   _holdTimer     = 0f;
        private float   _prevPinchDist = 0f;

        private void Update()
        {
            if (Input.touchCount == 2)
            {
                HandlePinch();
                return;
            }

            if (Input.touchCount == 0)
            {
                if (_touching) EndTouch();
                return;
            }

            var t = Input.GetTouch(0);
            switch (t.phase)
            {
                case TouchPhase.Began:      BeginTouch(t.position);   break;
                case TouchPhase.Moved:      MoveTouch(t.position);    break;
                case TouchPhase.Stationary: UpdateHold(t.position);   break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:   EndTouch();               break;
            }
        }

        private void BeginTouch(Vector2 pos)
        {
            _touching   = true;
            _isDragging = false;
            _isHolding  = false;
            _startPos   = pos;
            _prevPos    = pos;
            _touchTime  = 0f;
            _holdTimer  = 0f;
        }

        private void MoveTouch(Vector2 pos)
        {
            if (!_touching) return;
            _touchTime += Time.deltaTime;
            float dist = Vector2.Distance(_startPos, pos);

            if (!_isDragging && (dist >= _dragThresholdPx || _touchTime >= _holdThresholdSec))
            {
                _isDragging = true;
                _isHolding  = false;
                OnDragBegin?.Invoke(_startPos);
            }

            if (_isDragging)
            {
                Vector2 delta = pos - _prevPos;
                OnDragMove?.Invoke(pos, delta);
            }
            _prevPos = pos;
        }

        private void UpdateHold(Vector2 pos)
        {
            if (!_touching || _isDragging) return;
            _touchTime += Time.deltaTime;
            _holdTimer += Time.deltaTime;

            if (!_isHolding && _holdTimer >= _holdMinSec)
            {
                _isHolding = true;
                OnHoldBegin?.Invoke(pos);
            }
        }

        private void EndTouch()
        {
            if (!_touching) return;

            if (_isHolding)
                OnHoldEnd?.Invoke(_prevPos);
            else if (_isDragging)
                OnDragEnd?.Invoke(_prevPos);
            else
                OnTap?.Invoke(_prevPos);

            _touching   = false;
            _isDragging = false;
            _isHolding  = false;
        }

        private void HandlePinch()
        {
            var t0 = Input.GetTouch(0);
            var t1 = Input.GetTouch(1);
            float dist = Vector2.Distance(t0.position, t1.position);

            if (t1.phase == TouchPhase.Began)
            {
                _prevPinchDist = dist;
                return;
            }

            if (_prevPinchDist > 0f)
            {
                float ratio = dist / _prevPinchDist;
                OnPinch?.Invoke(ratio);
            }
            _prevPinchDist = dist;
        }
    }
}
