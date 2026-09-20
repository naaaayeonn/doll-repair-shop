using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DollShop.Core;

namespace DollShop.View
{
    /// <summary>
    /// 3D 인형 뷰어. 전용 씬 Additive 로드 → RenderTexture → RawImage.
    /// 드래그 회전 + 핀치 확대 + 관성(0.6초 감속).
    /// </summary>
    public class DollInspector3D : MonoBehaviour
    {
        [SerializeField] private RawImage     _displayImage;
        [SerializeField] private RenderTexture _renderTexture;
        [SerializeField] private string        _inspectSceneName = "Inspect";

        [Header("조작 설정")]
        [SerializeField] private float _rotateSpeed  = 0.3f;
        [SerializeField] private float _minZoom      = 1.0f;
        [SerializeField] private float _maxZoom      = 2.5f;
        [SerializeField] private float _inertiaDecay = 0.6f; // 0.6초 감속

        private Transform  _dollTransform;
        private float      _currentZoom     = 1f;
        private Vector2    _inertiaVelocity = Vector2.zero;
        private bool       _isOpen          = false;
        private DollId     _currentDoll;

        public void Open(DollId doll, AssetRegistry registry)
        {
            if (_isOpen) return;
            _currentDoll = doll;
            _isOpen      = true;
            gameObject.SetActive(true);

            TimeService.BeginInspect(doll);
            // TODO: W2 — Additive 씬 로드, 해당 인형 모델 스폰, RenderTexture 바인딩
        }

        public void Close()
        {
            if (!_isOpen) return;
            _isOpen = false;

            ClueId clue = TimeService.EndInspect();
            if (!clue.IsEmpty)
            {
                NoteSystem.TryAddClue(clue);
                // TODO: W5 — 단서 획득 연출
            }

            // TODO: W2 — 씬 Unload + RenderTexture Release
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!_isOpen || _dollTransform == null) return;

            // 관성 적용
            if (_inertiaVelocity.sqrMagnitude > 0.001f)
            {
                _dollTransform.Rotate(Vector3.up, -_inertiaVelocity.x, Space.World);
                _inertiaVelocity = Vector2.Lerp(_inertiaVelocity, Vector2.zero, Time.deltaTime / _inertiaDecay);
            }
        }
    }
}
