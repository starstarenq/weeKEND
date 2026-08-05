using UnityEngine;

public class UI_TraitBook : PopUI
{
    // ⚠️ 위에서 만든 UI_TraitItem 컴포넌트 배열을 선언하여 에디터와 연결고리를 만듭니다.
    [Header("Trait Items (SubUI)")]
    [SerializeField] private UI_TraitItem[] traitSubUIItems;

    private bool _initAlready = false;

    // 기획서 스펙 가상 데이터 (이름, 설명)
    private readonly string[,] _traitData = new string[,]
    {
        { "강인함", "최대 체력이 영구적으로 증가합니다." },
        { "민첩함", "이동 속도와 회피율이 소폭 상승합니다." },
        { "냉철함", "감정 게이지의 불행 방향 변동성이 감소합니다." },
        { "낙천주의", "행복 감정 수치 획득량이 20% 증가합니다." },
        { "과몰입", "스킬 재사용 대기시간이 감소하지만 피격 데미지가 늘어납니다." }
    };

    public override void Init()
    {
        if (_initAlready) return;
        _initAlready = true;
    }

    public override void ShowPopup()
    {
        Init();
        base.ShowPopup();
        RefreshTraitBook();
    }

    private void RefreshTraitBook()
    {
        if (traitSubUIItems == null || traitSubUIItems.Length == 0) return;

        int loopCount = Mathf.Min(traitSubUIItems.Length, _traitData.GetLength(0));

        for (int i = 0; i < loopCount; i++)
        {
            if (traitSubUIItems[i] != null)
            {
                traitSubUIItems[i].Init();

                string name = _traitData[i, 0];
                string desc = _traitData[i, 1];

                // 개별 하위 SubUI에 데이터를 전달하여 화면을 갱신합니다.
                traitSubUIItems[i].SetTraitInfo(name, desc);
            }
        }
    }
}
