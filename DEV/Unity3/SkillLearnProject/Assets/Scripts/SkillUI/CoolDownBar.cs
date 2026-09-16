using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트를 사용하기 위해 필요합니다.

public class CoolDownBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image coolTimeImage; // 회색 쿨타임 이미지 (Image Type: Filled 설정 필요)

    private float maxCoolTime = 5.0f; // 최대 쿨타임 (초 단위, 기본값 5초)
    private float currentCoolTime = 0.0f; // 현재 남은 쿨타임
    private bool isCoolingDown = false; // 현재 쿨타임 진행 중인지 여부

    void Update()
    {
        if (isCoolingDown)
        {
            UpdateCoolTime();
        }
    }

    /// <summary>
    /// 스킬을 사용할 때 호출하는 함수입니다.
    /// </summary>
    /// <param name="coolTime">적용할 쿨타임 시간</param>
    public void UseSkill(float coolTime)
    {
        // 이미 쿨타임 중이라면 중복 실행 방지
        if (isCoolingDown) return;

        maxCoolTime = coolTime;
        currentCoolTime = maxCoolTime;
        isCoolingDown = true;

        if (coolTimeImage != null)
        {
            coolTimeImage.fillAmount = 1f; // 쿨타임 시작 시 채우기를 1로 초기화
        }
    }

    /// <summary>
    /// 매 프레임 쿨타임 시간을 감소시키고 UI를 업데이트합니다.
    /// </summary>
    private void UpdateCoolTime()
    {
        // 프레임 시간을 빼서 남은 시간 계산
        currentCoolTime -= Time.deltaTime;

        if (coolTimeImage != null)
        {
            // 남은 시간에 비례하여 fillAmount를 1에서 0으로 줄임
            coolTimeImage.fillAmount = currentCoolTime / maxCoolTime;
        }

        // 쿨타임이 끝났을 때 처리
        if (currentCoolTime <= 00f)
        {
            EndCoolTime();
        }
    }

    /// <summary>
    /// 쿨타임이 종료되었을 때 초기화하는 함수입니다.
    /// </summary>
    private void EndCoolTime()
    {
        currentCoolTime = 0f;
        isCoolingDown = false;

        if (coolTimeImage != null)
        {
            coolTimeImage.fillAmount = 0f; // 확실하게 0으로 비워줌
        }
    }
}
