using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_GameOver : PopUI
{
    enum Buttons
    {
        RestartButton,
        MainMenuButton
    }

    enum Images
    {
        SelectionArrowImage // 화살표 이미지 오브젝트
    }

    private bool _initAlready = false;
    private int _selectedIndex = 0; // 0: 재시작, 1: 메인메뉴/종료

    public override void Init()
    {
        if (_initAlready) return;

        // UI 요소 바인딩
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));

        // 마우스 클릭 이벤트 추가
        Button restartBtn = Get<Button>((int)Buttons.RestartButton);
        if (restartBtn != null)
        {
            restartBtn.onClick.RemoveAllListeners();
            restartBtn.onClick.AddListener(OnClickRestart);
        }

        Button mainBtn = Get<Button>((int)Buttons.MainMenuButton);
        if (mainBtn != null)
        {
            mainBtn.onClick.RemoveAllListeners();
            mainBtn.onClick.AddListener(OnClickMainMenu);
        }

        _initAlready = true;
    }

    public override void ShowPopup()
    {
        Init();
        base.ShowPopup();

        Time.timeScale = 0f; // 게임 일시 정지
        _selectedIndex = 0;  // 기본 선택: 재시작 버튼
        UpdateMenuVisual();
    }

    public override void ClosePopup()
    {
        base.ClosePopup();
        Time.timeScale = 1f;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
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
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ExecuteSelectedMenu();
        }
    }

    /// <summary>
    /// 선택된 메뉴의 글자 색상 및 화살표 Y축 위치를 업데이트하는 함수
    /// </summary>
    private void UpdateMenuVisual()
    {
        Button restartBtn = Get<Button>((int)Buttons.RestartButton);
        Button mainBtn = Get<Button>((int)Buttons.MainMenuButton);
        Image arrowImg = Get<Image>((int)Images.SelectionArrowImage);

        if (restartBtn == null || mainBtn == null) return;

        Text restartText = restartBtn.GetComponentInChildren<Text>();
        Text mainText = mainBtn.GetComponentInChildren<Text>();

        // 1. 선택된 메뉴의 글자 색상 연동 (노란색/흰색)
        if (_selectedIndex == 0)
        {
            if (restartText != null) restartText.color = Color.yellow;
            if (mainText != null) mainText.color = Color.white;
        }
        else
        {
            if (restartText != null) restartText.color = Color.white;
            if (mainText != null) mainText.color = Color.yellow;
        }

        // 2. 화살표 X축 위치 고정 / Y축 위치 연동
        Button targetButton = (_selectedIndex == 0) ? restartBtn : mainBtn;
        if (arrowImg != null && targetButton != null)
        {
            RectTransform arrowRect = arrowImg.rectTransform;
            RectTransform targetRect = targetButton.GetComponent<RectTransform>();

            // 현재 화살표의 X 좌표는 유지하고 Y 좌표만 선택된 버튼의 Y 좌표로 세팅
            Vector2 arrowPos = arrowRect.anchoredPosition;
            arrowPos.y = targetRect.anchoredPosition.y;
            arrowRect.anchoredPosition = arrowPos;
        }
    }

    private void ExecuteSelectedMenu()
    {
        if (_selectedIndex == 0)
        {
            Debug.Log("키보드 엔터 선택: 게임을 재시작합니다.");
            OnClickRestart();
        }
        else if (_selectedIndex == 1)
        {
            Debug.Log("키보드 엔터 선택: 메인메뉴로 이동하거나 게임을 종료합니다.");
            OnClickMainMenu();
        }
    }

    private void OnClickRestart()
    {
        Time.timeScale = 1f;
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    [Header("메인 메뉴 UI 참조")]
    [SerializeField] private UI_MainMenuScene mainMenuUI;

    private void OnClickMainMenu()
    {
        Time.timeScale = 0f; // 메인 메뉴 화면으로 돌아가므로 시간 일시정지 유지

        // 게임 오버 팝업 닫기
        gameObject.SetActive(false);

        // 메인 메뉴 화면 활성화
        if (mainMenuUI != null)
        {
            mainMenuUI.gameObject.SetActive(true);
            mainMenuUI.Init();
        }
    }
}