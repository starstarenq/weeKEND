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
    }

    /// <summary>
    /// 화살표 UI의 X축 좌표는 그대로 두고 선택된 메뉴에 맞게 Y축 좌표 및 글자 색상만 업데이트하는 함수
    /// </summary>
    private void UpdateMenuVisual()
    {
        Button resumeBtn = Get<Button>((int)Buttons.ResumeButton);
        Button exitBtn = Get<Button>((int)Buttons.ExitButton);
        Image arrowImg = Get<Image>((int)Images.SelectionArrowImage);

        if (resumeBtn == null || exitBtn == null) return;

        Text resumeText = resumeBtn.GetComponentInChildren<Text>();
        Text exitText = exitBtn.GetComponentInChildren<Text>();

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

        // 2. 화살표 X축 위치 고정 / Y축 위치 연동
        Button targetButton = (_selectedIndex == 0) ? resumeBtn : exitBtn;
        if (arrowImg != null && targetButton != null)
        {
            RectTransform arrowRect = arrowImg.rectTransform;
            RectTransform targetRect = targetButton.GetComponent<RectTransform>();

            // 현재 화살표의 X 좌표는 그대로 유지하고, Y 좌표만 선택된 버튼의 Y 좌표로 변경
            Vector2 arrowPos = arrowRect.anchoredPosition;
            arrowPos.y = targetRect.anchoredPosition.y;
            arrowRect.anchoredPosition = arrowPos;
        }
    }

    private void ExecuteSelectedMenu()
    {
        if (_selectedIndex == 0)
        {
            Debug.Log("게임을 재개합니다.");
            ClosePopup();
        }
        else if (_selectedIndex == 1)
        {
            Debug.Log("게임을 종료합니다.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}