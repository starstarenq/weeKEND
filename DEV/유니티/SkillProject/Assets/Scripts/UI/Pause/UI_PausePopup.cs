using UnityEngine;
using UnityEngine.UI;

public class UI_PausePopup : PopUI
{
    enum Buttons
    {
        ResumeButton,
        ExitButton
    }

    enum Images
    {
        SelectionArrowImage
    }

    private bool _initAlready = false;
    private int _selectedIndex = 0;

    public override void Init()
    {
        if (_initAlready) return;

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));

        _initAlready = true;
    }

    public override void ShowPopup()
    {
        Init();
        base.ShowPopup();

        _selectedIndex = 0;
        UpdateMenuVisual();
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        // 키보드 방향키 입력 처리
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _selectedIndex = 0;
            UpdateMenuVisual();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _selectedIndex = 1;
            UpdateMenuVisual();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            ExecuteSelectedMenu();
        }

        // ⚠️ 흔들림 연출(AnimateArrow) 함수 호출을 삭제하여 움직이지 않게 합니다.
    }

    /// <summary>
    /// 화살표 UI의 위치와 글자 색상을 선택된 메뉴에 맞게 고정 연결하는 함수
    /// </summary>
    private void UpdateMenuVisual()
    {
        Button resumeBtn = Get<Button>((int)Buttons.ResumeButton);
        Button exitBtn = Get<Button>((int)Buttons.ExitButton);
        Image arrowImg = Get<Image>((int)Images.SelectionArrowImage);

        Text resumeText = resumeBtn.GetComponentInChildren<Text>();
        Text exitText = exitBtn.GetComponentInChildren<Text>();

        Button targetButton = (_selectedIndex == 0) ? resumeBtn : exitBtn;

        // 1. 글자 색상 실시간 연동
        if (_selectedIndex == 0)
        {
            if (resumeText != null) resumeText.color = Color.yellow;
            if (exitText != null) exitText.color = Color.white;
        }
        else
        {
            if (resumeText != null) resumeText.color = Color.white;
            if (exitText != null) exitText.color = Color.yellow;
        }

        // 2. ⚠️ 화살표 위치 고정 연동 (흔들리지 않고 지정된 자리에 정지)
        if (arrowImg != null && targetButton != null)
        {
            // 화살표를 선택된 버튼의 자식으로 등록
            arrowImg.transform.SetParent(targetButton.transform);

            // 버튼의 왼쪽 앞에 딱 멈춰 서 있도록 로컬 좌표계 정렬
            // (화살표가 글자와 너무 가깝거나 멀다면 -120f 숫자를 조절해 보세요)
            arrowImg.transform.localPosition = new Vector3(-300f, 0f, 0f);
        }
    }

    private void ExecuteSelectedMenu()
    {
        if (_selectedIndex == 0)
        {
            Debug.Log("키보드 엔터 선택: 게임을 재개합니다.");
            ClosePopup();
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
