using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LightingSample
{
    /// <summary>
    /// Light Probe On/Off 실습용 컨트롤러.
    /// 지정한 렌더러들의 LightProbeUsage 를 BlendProbes(On) ↔ Off 로 전환한다.
    /// - autoToggle 이 켜져 있으면 일정 간격으로 자동 전환하여 차이를 시연.
    /// - 인스펙터 우클릭 ContextMenu "Toggle Now" 또는 SetLightProbes(bool) 로 수동 전환.
    /// (신규 Input System 충돌을 피하려고 키 입력은 사용하지 않는다.)
    /// </summary>
    public class LightProbeDemoController : MonoBehaviour
    {
        [Tooltip("Light Probe 사용을 On/Off 할 렌더러 목록 (동적 오브젝트).")]
        public List<Renderer> probeRenderers = new List<Renderer>();

        [Tooltip("현재 Light Probe 사용 여부.")]
        public bool lightProbesEnabled = true;

        [Header("Auto Toggle")]
        public bool autoToggle = true;
        [Tooltip("자동 전환 간격(초).")]
        public float toggleInterval = 2f;

        private float _timer;

        private void Start() => Apply();

        private void Update()
        {
            if (!autoToggle) return;
            _timer += Time.deltaTime;
            if (_timer >= toggleInterval)
            {
                _timer = 0f;
                lightProbesEnabled = !lightProbesEnabled;
                Apply();
            }
        }

        public void SetLightProbes(bool on)
        {
            lightProbesEnabled = on;
            Apply();
        }

        [ContextMenu("Toggle Now")]
        public void ToggleNow()
        {
            lightProbesEnabled = !lightProbesEnabled;
            Apply();
        }

        /// <summary>현재 상태를 렌더러에 반영.</summary>
        public void Apply()
        {
            var usage = lightProbesEnabled ? LightProbeUsage.BlendProbes : LightProbeUsage.Off;
            for (int i = 0; i < probeRenderers.Count; i++)
                if (probeRenderers[i] != null)
                    probeRenderers[i].lightProbeUsage = usage;
        }
    }
}
