using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_Skill : MonoBehaviour
{
    [SerializeField] Image CoolTime_Image;
    [SerializeField] float CoolTimeDuration = 3f;

    // Skill 발동(입력) -> <  ?? manager> 스킬이 지금 발동되었어요!! -> UI ok 연락받음 쿨타임 동작할게
    // 타이밍을 구현하는 법. 이벤트 / 싱글톤 / 직접 참조

    [SerializeField] Ease easeCurve;

    [SerializeField] SimpleController_UsingPlayerInput playerInput;

    // Callback 끝나면 나한테 연락해줘
    // event 기능의 본질
    // 람다식
    // Update is called once per frame

    private void Start()
    {
        playerInput.OnAltFireInputTriggered += OnAltFireInputTriggered;
    }

    private void OnDestroy()
    {
        playerInput.OnAltFireInputTriggered -= OnAltFireInputTriggered;
    }

    private void OnAltFireInputTriggered()
    {
        CoolTime_Image.DOFillAmount(0, CoolTimeDuration)
               .OnComplete(DoFillAmountCompleted);
    }

    //void Update()
    //{
    //    if(Keyboard.current.qKey.wasPressedThisFrame) 
    //    {
    //        CoolTime_Image.DOFillAmount(0, CoolTimeDuration)
    //            .OnComplete(DoFillAmountCompleted);
    //    }
    //}



    void DoFillAmountCompleted()
    {
        // 이 기능이 끝났을 때 해야할 작업들을 작업해라
        CoolTime_Image.fillAmount = 1f;
        //gameObject.SetActive(false);
    }

    // 자동 시전(입력) -> 스킬 시전! -> UI, Progress 실행! -> 끝났으면 해야할 거 해!
}
