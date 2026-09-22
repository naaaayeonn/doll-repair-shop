using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace DollShop.View
{
    /// <summary>
    /// 오디오 싱글톤 서비스.
    /// BGM·SFX·AMB 3그룹, SFX 풀 8개, 피치 랜덤 ±5%.
    /// </summary>
    public class AudioService : MonoBehaviour
    {
        public static AudioService Instance { get; private set; }

        [Header("Mixer")]
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private string     _bgmParam = "BGM";
        [SerializeField] private string     _sfxParam = "SFX";
        [SerializeField] private string     _ambParam = "AMB";

        [Header("Sources")]
        [SerializeField] private AudioSource   _bgmSource;
        [SerializeField] private AudioSource   _ambSource;
        [SerializeField] private AudioSource[] _sfxPool = new AudioSource[8];

        [Header("Clips — 추후 AssetRegistry로 이관")]
        // TODO: W6 — AssetRegistry에서 키 기반으로 로드
        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();

        private int _poolIndex = 0;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ── BGM ──────────────────────────────────────────────
        public void PlayBgm(string key, float fadeDuration = 0.5f)
        {
            // TODO: W3 — 크로스페이드 구현
            if (_clips.TryGetValue(key, out var clip) && _bgmSource != null)
            {
                _bgmSource.clip = clip;
                _bgmSource.Play();
            }
        }

        // ── SFX ──────────────────────────────────────────────
        public void PlaySfx(string key)
        {
            if (!_clips.TryGetValue(key, out var clip)) return;
            var src = _sfxPool[_poolIndex % _sfxPool.Length];
            _poolIndex++;
            src.pitch = 1f + Random.Range(-0.05f, 0.05f); // ±5% 피치 랜덤
            src.PlayOneShot(clip);
        }

        // ── AMB ──────────────────────────────────────────────
        public void SetAmbience(string key)
        {
            // TODO: W3
        }

        public void SetTension(float t)
        {
            // TODO: W3 — BGM 긴장 레이어 볼륨 크로스페이드 (0~1)
        }

        // ── 볼륨 설정 ────────────────────────────────────────
        public void SetVolume(string paramName, float vol)
        {
            if (_mixer == null) return;
            float db = vol > 0.001f ? Mathf.Log10(vol) * 20f : -80f;
            _mixer.SetFloat(paramName, db);
        }
    }
}
