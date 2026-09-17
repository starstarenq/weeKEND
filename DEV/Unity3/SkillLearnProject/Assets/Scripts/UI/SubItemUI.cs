using UnityEngine;

public class SubItemUI : UIBase
{
    public override void Init()
    {
        base.Init();

        // 서브 아이템은 정렬이나 독립 실행이 아닌 내부 데이터 바인딩 위주로 작동합니다.
    }

    // 외부(부모 UI)에서 데이터를 넘겨받아 UI를 갱신할 때 사용
    public virtual void SetInfo()
    {
        Init();
        // 개별 슬롯 아이템 정보 세팅 로직
    }
}