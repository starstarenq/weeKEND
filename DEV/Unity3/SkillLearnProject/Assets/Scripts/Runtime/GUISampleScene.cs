using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GUISampleScene : SceneUI
{
    
    enum Texts
    {
        TitleText,
        PatronText
    }

    enum Buttons
    {
        ContinueButton,
        StartButton,
        SettingButton,
        ExitButton
    }

    private void Start()
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
        GetButton((int)Buttons.ContinueButton).onClick.AddListener(OnContinueButton);
        GetButton((int)Buttons.StartButton).onClick.AddListener(OnClickedStart);
        GetButton((int)Buttons.SettingButton).onClick.AddListener(OnSettingButton);
        GetButton((int)Buttons.ExitButton).onClick.AddListener(OnExitButton);
        GetText((int)Texts.TitleText).text = "GUI SAMPLE SCENE";
    }

    private void OnClickedStart()
    {
        Debug.Log("게임 시작 버튼 클릭됨!");
    }

    private void OnContinueButton()
    {
        Debug.Log("게임 계속 버튼 클릭됨!");
    }

    private void OnSettingButton()
    {
        Debug.Log("게임 설정 버튼 클릭됨!");
    }

    private void OnExitButton()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif

        Application.Quit();
    }
}
