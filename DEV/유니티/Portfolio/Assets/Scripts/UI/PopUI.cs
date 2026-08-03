using UnityEngine;

// 추상 클래스로 선언하여 다른 팝업들이 상속받을 수 있도록 합니다.
public abstract class PopUI : UIBase
{
    // 모든 팝업 UI가 공통으로 사용할 열기/닫기 가상 함수
    public virtual void ShowPopup()
    {
        Init(); // 안전 장치 초기화
        gameObject.SetActive(true);

        // 팝업이 열릴 때 게임을 일시정지하는 공통 규칙 적용
        Time.timeScale = 0f;
    }

    public virtual void ClosePopup()
    {
        gameObject.SetActive(false);

        // 팝업이 닫힐 때 게임을 다시 재생하는 공통 규칙 적용
        Time.timeScale = 1f;
    }
}
