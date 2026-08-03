using UnityEngine;
using UnityEngine.UI;

public class UI_PausePopup : PopUI
{
    enum Buttons
    {
        ResumeButton,
        ExitButton
    }

    private bool _initAlready = false;
    private int _selectedIndex = 0; // 0: 이어하기, 1: 나가기

    public override void Init()
    {
        if (_initAlready) return;

        // 1. 버튼 컴포넌트 자동 바인딩
        Bind<Button>(typeof(Buttons));

        _initAlready = true;
    }

    public override void ShowPopup()
    {
        Init();
        base.ShowPopup();

        // 팝업이 켜질 때 항상 첫 번째 메뉴(이어하기)가 선택되도록 초기화
        _selectedIndex = 0;
        UpdateMenuVisual();
    }

    private void Update()
    {
        // 팝업이 활성화되어 있고, 일시정지 상태(Time.timeScale = 0)에서도 키 입력을 받기 위해 UnscaledDeltaTime 계열 작동
        if (!gameObject.activeSelf) return;

        // 1. 위 화살표 입력 시
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _selectedIndex = 0; // 첫 번째 메뉴로 이동
            UpdateMenuVisual();
        }
        // 2. 아래 화살표 입력 시
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _selectedIndex = 1; // 두 번째 메뉴로 이동
            UpdateMenuVisual();
        }
        // 3. 엔터(Return) 키 입력 시 선택된 기능 실행
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            ExecuteSelectedMenu();
        }
    }

    /// <summary>
    /// 선택된 버튼의 글자 색상을 바꾸어 시각적으로 강조하는 함수
    /// </summary>
    private void UpdateMenuVisual()
    {
        Button resumeBtn = Get<Button>((int)Buttons.ResumeButton);
        Button exitBtn = Get<Button>((int)Buttons.ExitButton);

        // 버튼 하위의 Text 컴포넌트를 가져옴
        Text resumeText = resumeBtn.GetComponentInChildren<Text>();
        Text exitText = exitBtn.GetComponentInChildren<Text>();

        // 0번(이어하기)이 선택된 경우
        if (_selectedIndex == 0)
        {
            if (resumeText != null) resumeText.color = Color.yellow; // 선택됨 (노란색)
            if (exitText != null) exitText.color = Color.white;     // 선택 안 됨 (흰색)
        }
        // 1번(나가기)이 selected된 경우
        else
        {
            if (resumeText != null) resumeText.color = Color.white;  // 선택 안 됨 (흰색)
            if (exitText != null) exitText.color = Color.yellow;    // 선택됨 (노란색)
        }
    }

    /// <summary>
    /// 엔터를 쳤을 때 실제 로직을 실행하는 브릿지 함수
    /// </summary>
    private void ExecuteSelectedMenu()
    {
        if (_selectedIndex == 0)
        {
            Debug.Log("키보드 엔터 선택: 게임을 재개합니다.");
            ClosePopup(); // 부모(PopUI)의 ClosePopup 호출
        }
        else if (_selectedIndex == 1)
        {
            Debug.Log("키보드 엔터 선택: 게임을 종료합니다.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }
    }
}
