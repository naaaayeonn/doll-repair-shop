using System;
using System.Collections;
using UnityEngine;

namespace DollShop.View
{
    /// <summary>경량 트위너. DoTween 없이 코루틴 기반.</summary>
    public static class Tweener
    {
        // 팝업 스케일 인 (OutBack 이징, 0.18초)
        public static IEnumerator ScaleIn(Transform target, float duration = 0.18f, Action onComplete = null)
        {
            target.localScale = Vector3.zero;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                target.localScale = Vector3.one * EaseOutBack(t);
                yield return null;
            }
            target.localScale = Vector3.one;
            onComplete?.Invoke();
        }

        // 페이드 (0.25초)
        public static IEnumerator Fade(CanvasGroup cg, float from, float to, float duration = 0.25f, Action onComplete = null)
        {
            float elapsed = 0f;
            cg.alpha = from;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            cg.alpha = to;
            onComplete?.Invoke();
        }

        // 버튼 탭 스케일 (0.94 → 1.0, 0.08초)
        public static IEnumerator ButtonTap(Transform target)
        {
            target.localScale = Vector3.one * 0.94f;
            float elapsed = 0f;
            while (elapsed < 0.08f)
            {
                elapsed += Time.unscaledDeltaTime;
                target.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, elapsed / 0.08f);
                yield return null;
            }
            target.localScale = Vector3.one;
        }

        // 좌우 흔들림 (비활성 버튼)
        public static IEnumerator Shake(Transform target, float duration = 0.3f, float magnitude = 8f)
        {
            Vector3 origin = target.localPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float x = Mathf.Sin(elapsed * 50f) * magnitude * (1f - elapsed / duration);
                target.localPosition = origin + new Vector3(x, 0f, 0f);
                yield return null;
            }
            target.localPosition = origin;
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
