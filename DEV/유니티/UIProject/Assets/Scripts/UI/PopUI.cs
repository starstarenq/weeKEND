using UnityEngine;

public abstract class PopUI : UIBase
{
    public virtual void ShowPopup()
    {
        Init();
        gameObject.SetActive(true);
        Time.timeScale = 0f; // 팝업이 열리면 게임 시간 정지
    }

    public virtual void ClosePopup()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f; // 팝업이 닫히면 게임 시간 재생
    }
}
