using UnityEngine;

namespace DollShop.View
{
    /// <summary>Android/iOS 햅틱 피드백 래퍼.</summary>
    public static class Haptics
    {
        public enum Impact { Light, Medium, Heavy }

        public static bool Enabled { get; set; } = true;

        public static void Play(Impact impact)
        {
            if (!Enabled) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            long duration = impact switch {
                Impact.Light  => 10L,
                Impact.Medium => 25L,
                Impact.Heavy  => 50L,
                _             => 20L
            };
            Handheld.Vibrate();
            // TODO: W4 — Android VibrationEffect API (API 26+) 직접 호출
#elif UNITY_IOS && !UNITY_EDITOR
            // TODO: W4 — iOS UIImpactFeedbackGenerator 네이티브 플러그인
#endif
        }

        public static void Light()  => Play(Impact.Light);
        public static void Medium() => Play(Impact.Medium);
        public static void Heavy()  => Play(Impact.Heavy);
    }
}
