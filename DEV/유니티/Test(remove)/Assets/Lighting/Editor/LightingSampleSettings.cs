using UnityEngine;

namespace LightingSample.EditorTools
{
    /// <summary>
    /// 광원 실습 샘플 생성기 파라미터.
    /// </summary>
    [System.Serializable]
    public class LightingSampleSettings
    {
        // ── 출력 ──
        public string rootFolder = "Assets/Lighting";
        public string sampleName = "LightingLab";
        [Tooltip("새 씬을 만들어 저장한다. 끄면 현재 씬에 생성.")]
        public bool createNewScene = true;

        // ── 어떤 요소를 만들지 ──
        [Tooltip("① 외부 광원 샘플(야외 지면 + 태양광).")]
        public bool buildOutdoor = true;
        [Tooltip("② 차폐된 내부 공간(지붕/벽으로 태양광 차단 + 내부 조명).")]
        public bool buildRoom = true;
        [Tooltip("③ Light Probe 배치(야외→출입구→실내 그리드).")]
        public bool buildLightProbes = true;
        [Tooltip("④ Light Probe On/Off 비교 구체 + 토글 컨트롤러.")]
        public bool buildComparison = true;
        [Tooltip("베이크용 LightingSettings 를 생성/적용.")]
        public bool setupLightingSettings = true;

        // ── 공간 크기 ──
        public float groundSize = 40f;
        public Vector3 roomInteriorSize = new Vector3(10f, 6f, 10f);
        public Vector3 roomCenter = new Vector3(10f, 0f, 0f);
        public float wallThickness = 0.3f;
        public float doorWidth = 3f;
        public float doorHeight = 4f;

        // ── 조명 ──
        public Color sunColor = new Color(1f, 0.96f, 0.84f);
        public float sunIntensity = 1.2f;
        public Vector3 sunEuler = new Vector3(50f, -60f, 0f);
        public Color interiorLightColor = new Color(1f, 0.7f, 0.4f);
        public float interiorLightIntensity = 3f;
        public float interiorLightRange = 14f;

        // ── Light Probe ──
        public float probeSpacing = 2.5f;
        public float probeHeightLow = 1f;
        public float probeHeightHigh = 3.5f;

        // ── 비교 구체 ──
        [Tooltip("한 줄당 구체 수(야외→실내로 배치).")]
        public int spherePerRow = 5;
        public float sphereRadius = 0.6f;
        [Tooltip("On/Off 자동 전환 간격(초).")]
        public float autoToggleInterval = 2f;

        // ── 베이크 품질(빠른 실습용) ──
        public float lightmapResolution = 12f;
        public int lightmapMaxSize = 512;
    }
}
