using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VolumeSetup.EditorTools
{
    /// <summary>
    /// Global Volume + 그래픽 품질 세팅 파라미터.
    /// </summary>
    [System.Serializable]
    public class VolumeQualitySettings
    {
        // ── 출력 / 대상 ──
        public string rootFolder = "Assets/Volume";
        public string profileName = "GlobalVolumeProfile";
        public string volumeName = "Global Volume";
        public int volumePriority = 0;
        [Tooltip("현재 씬에 Global Volume 오브젝트를 추가한다.")]
        public bool addToCurrentScene = true;

        // ── Tonemapping ──
        public bool enableTonemapping = true;
        public TonemappingMode tonemappingMode = TonemappingMode.ACES;

        // ── Bloom ──
        public bool enableBloom = true;
        public float bloomThreshold = 0.9f;
        public float bloomIntensity = 0.9f;
        [Range(0f, 1f)] public float bloomScatter = 0.7f;
        public Color bloomTint = Color.white;
        public bool bloomHighQuality = true;

        // ── Color Adjustments ──
        public bool enableColorAdjustments = true;
        public float postExposure = 0.0f;
        [Range(-100f, 100f)] public float contrast = 10f;
        [Range(-100f, 100f)] public float saturation = 8f;
        public Color colorFilter = Color.white;

        // ── Vignette ──
        public bool enableVignette = true;
        [Range(0f, 1f)] public float vignetteIntensity = 0.3f;
        [Range(0.01f, 1f)] public float vignetteSmoothness = 0.4f;
        public Color vignetteColor = Color.black;

        // ── SSAO (Renderer Feature) ──
        [Tooltip("활성 Universal Renderer 에 Screen Space Ambient Occlusion 을 추가/설정.")]
        public bool enableSSAO = true;
        [Range(0f, 10f)] public float ssaoIntensity = 3.0f;
        [Range(0f, 1f)] public float ssaoRadius = 0.25f;
        [Range(0f, 1f)] public float ssaoDirectLightingStrength = 0.25f;
    }
}
