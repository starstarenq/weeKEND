using UnityEngine;
using UnityEngine.UI;

public class EnemyHP : MonoBehaviour
{
    private Transform mainCameraTransform;
    [Header("체력 및 UI 설정")]
    public float maxHp = 100f;
    private float currentHp;
    [SerializeField] private Slider hpSlider; // [추가] 머리 위 UI Slider 연결용 변수

    void Start()
    {
        currentHp = maxHp;
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (mainCameraTransform != null)
        {
            // UI의 방향이 항상 카메라가 바라보는 정면 방향과 일치하도록 회전시킵니다.
            transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward,
                             mainCameraTransform.rotation * Vector3.up);
        }
    }

}
