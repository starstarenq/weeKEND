using UnityEngine;
using UnityEngine.UI;

public class UI_TraitItem : SubUI
{
    enum Texts
    {
        TraitNameText,  // 유니티 오브젝트명과 일치해야 함
        TraitDescText   // 유니티 오브젝트명과 일치해야 함
    }

    private bool _initAlready = false;

    public override void Init()
    {
        if (_initAlready) return;

        // UIBase의 기능을 응용해 자식 텍스트 컴포넌트들을 자동 탐색
        Bind<Text>(typeof(Texts));

        _initAlready = true;
    }

    /// <summary>
    /// 메인 도감 시스템에서 데이터를 넘겨받을 때 호출되는 브릿지 함수
    /// </summary>
    public void SetTraitInfo(string traitName, string traitDesc)
    {
        Init(); // 초기화 보장

        Text nameText = Get<Text>((int)Texts.TraitNameText);
        Text descText = Get<Text>((int)Texts.TraitDescText);

        if (nameText != null) nameText.text = traitName;
        if (descText != null) descText.text = traitDesc;
    }
}
