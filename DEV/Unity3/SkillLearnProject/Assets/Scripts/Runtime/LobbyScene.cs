using UnityEngine.UI;
using UnityEngine;

public class UI_LobbyScene : SceneUI
{
    // 하위 자식 게임 오브젝트들의 '이름'과 정확히 일치하는 Enum 선언
    enum Buttons
    {
        StartButton,
        SettingButton,
    }

    enum Texts
    {
        TitleText,
        StatusText,
    }

    public void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();

        // 1. 자동 바인딩 수행 (Enum에 적힌 이름의 컴포넌트들을 알아서 수집)
        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));

        // 2. 바인딩된 컴포넌트 사용 및 이벤트 등록
        GetButton((int)Buttons.StartButton).onClick.AddListener(OnClickedStart);
        GetText((int)Texts.TitleText).text = "로비에 오신 것을 환영합니다!";
    }

    private void OnClickedStart()
    {
        Debug.Log("게임 시작 버튼 클릭됨!");
    }
}
