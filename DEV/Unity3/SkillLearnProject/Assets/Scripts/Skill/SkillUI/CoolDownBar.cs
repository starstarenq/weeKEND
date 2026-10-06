using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.Events;


// 구독자. 내가 언제 방송을 볼지 몰라.

public class CoolDownBar : MonoBehaviour
{
    // 회색 쿨타임 이미지 (Image Type: Filled, Fill Method: Radial 360 설정 필요)
    [SerializeField] private Image cooltimeImage;

    UnityEvent unityevent;

    private bool isCooldown = false;

    public FireballLauncher launcher;

    // GetComponent<T>


    private void Update()
    {
 
    }

    void Start()
    {
        if (cooltimeImage != null)
        {
            // 시작할 때는 쿨타임이 없으므로 Fill Amount를 0으로 설정
            cooltimeImage.fillAmount = 0f;

            launcher.CoolDownAction += HandleCoolDownAction;
        }
    }

    private void HandleCoolDownAction(float cooltime)
    {
        if (isCooldown) return;

        StartCoroutine(CoCooldown(cooltime));
    }

    /// <summary>
    /// 외부(예: 스킬 매니저나 입력 스크립트)에서 스킬을 실행할 때 호출하는 함수
    /// </summary>
    /// <param name="cooldownTime">쿨타임 시간(초)</param>
    public void UseSkill(float cooldownTime)
    {
        // 이미 쿨타임 중이면 중복 실행 방지
        if (isCooldown) return;

        StartCoroutine(CoCooldown(cooldownTime));
    }

    private IEnumerator CoCooldown(float cooldownTime)
    {
        isCooldown = true;
        float elapsedTime = 0f;

        // 처음에는 이미지를 가득 채움 (1에서 시작)
        cooltimeImage.fillAmount = 1f;

        while (elapsedTime < cooldownTime)
        {
            elapsedTime += Time.deltaTime;

            // 시간이 흐를수록 1에서 0으로 감소
            cooltimeImage.fillAmount = 1f - (elapsedTime / cooldownTime);

            yield return null;
        }

        // 확실하게 0으로 맞추고 쿨타임 종료
        cooltimeImage.fillAmount = 0f;
        isCooldown = false;
    }
}
