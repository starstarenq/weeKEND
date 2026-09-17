using UnityEngine;

public class PopupUI : UIBase
{
    public override void Init()
    {
        base.Init();

        // 팝업 UI 특성에 맞는 초기화 로직 (예: 캔버스 정렬 순서 상향)
        // Managers.UI.SetCanvas(gameObject, true);
    }

    // 팝업 닫기 기본 메서드
    public virtual void ClosePopupUI()
    {
        // Managers.UI.ClosePopup(this);
        Destroy(gameObject);
    }
}
