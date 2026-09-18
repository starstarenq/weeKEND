using UnityEngine;

public class GUISampleScene : SceneUI
{
    enum Buttons
{
    StartButton,
}
    enum Texts
    {
        TitleText,
        PatronText,
    }
    private void Start()
    {
        Init();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created




    public override void Init()
    {
        base.Init();

        // 1. 자동 바인딩 수행 (Enum에 적힌 이름의 컴포넌트들을 알아서 수집)
      /*  Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));

        // 2. 바인딩된 컴포넌트 사용 및 이벤트 등록
        GetButton((int)Buttons.StartButton).onClick.AddListener(OnClickedStart);
        GetText((int)Texts.TitleText).text = "로비에 오신 것을 환영합니다!";
    */
        }

    private void OnClickedStart()
    {
        Debug.Log("게임 시작 버튼 클릭됨!");
    }
}
