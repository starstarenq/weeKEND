using UnityEngine;

namespace LightingSample
{
    /// <summary>
    /// 두 지점 사이를 왕복(PingPong) 이동. Light Probe 실습에서 오브젝트가
    /// 야외 → 출입구 → 실내를 오갈 때 조명 변화를 관찰하기 위한 용도.
    /// </summary>
    public class SimplePingPongMover : MonoBehaviour
    {
        public Vector3 pointA;
        public Vector3 pointB;
        [Tooltip("이동 속도(m/s).")]
        public float speed = 2f;

        private void Update()
        {
            float dist = Vector3.Distance(pointA, pointB);
            if (dist < 0.001f) return;
            float t = Mathf.PingPong(Time.time * speed / dist, 1f);
            transform.position = Vector3.Lerp(pointA, pointB, t);
        }
    }
}
