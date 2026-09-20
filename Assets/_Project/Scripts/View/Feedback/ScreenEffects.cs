using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DollShop.View
{
    /// <summary>
    /// 정신력 구간별 화면 연출.
    /// URP Volume을 통해 비네팅·채도·렌즈 왜곡을 제어한다.
    /// </summary>
    public class ScreenEffects : MonoBehaviour
    {
        [SerializeField] private Volume _globalVolume;

        private Vignette         _vignette;
        private ColorAdjustments _colorAdj;
        private LensDistortion   _lensDistortion;

        private void Awake()
        {
            if (_globalVolume == null) return;
            _globalVolume.profile.TryGet(out _vignette);
            _globalVolume.profile.TryGet(out _colorAdj);
            _globalVolume.profile.TryGet(out _lensDistortion);
        }

        private void Start()
        {
            // Core 이벤트 구독
            DollShop.Core.SanitySystem.OnChanged += OnSanityChanged;
        }

        private void OnDestroy()
        {
            DollShop.Core.SanitySystem.OnChanged -= OnSanityChanged;
        }

        /// <summary>§2.4 정신력 구간별 화면 연출.</summary>
        private void OnSanityChanged(float current, float max)
        {
            float ratio = current / max; // 1.0 = 100%, 0.0 = 0%

            // 비네팅 강도
            if (_vignette != null)
            {
                float vigIntensity = ratio > 0.7f ? 0.2f :
                                     ratio > 0.4f ? Mathf.Lerp(0.35f, 0.2f, (ratio - 0.4f) / 0.3f) :
                                     ratio > 0.15f ? Mathf.Lerp(0.55f, 0.35f, (ratio - 0.15f) / 0.25f) :
                                                    Mathf.Lerp(0.7f, 0.55f, ratio / 0.15f);
                _vignette.intensity.value = vigIntensity;
            }

            // 채도 (15% 이하부터 저하)
            if (_colorAdj != null)
            {
                float saturation = ratio > 0.15f ? 0f : Mathf.Lerp(-60f, 0f, ratio / 0.15f);
                _colorAdj.saturation.value = saturation;
            }

            // 렌즈 왜곡 (15% 이하)
            if (_lensDistortion != null)
            {
                float distortion = ratio > 0.15f ? 0f : Mathf.Lerp(-0.3f, 0f, ratio / 0.15f);
                _lensDistortion.intensity.value = distortion;
            }
        }
    }
}
