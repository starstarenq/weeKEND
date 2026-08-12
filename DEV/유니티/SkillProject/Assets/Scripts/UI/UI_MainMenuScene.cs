using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenuScene : SceneUI
{
    // UIBase의 Bind 기능을 위해 hierarchy의 오브젝트 이름과 똑같이 맞춥니다.
    enum Buttons
    {
        StartButton,
        ExitButton
    }

    enum Images
    {
        SelectionArrowImage // 화살표 이미지가 없을 경우 생성하지 않아도 동작합니다.
    }

    [Header("인게임 UI 참조")]
    [SerializeField] private UI_InGameScene inGameSceneUI;

    private int _selectedIndex = 0; // 0: 게임 시작, 1: 게임 종료
    private bool _initAlready = false;

    public override void Init()
    {
        if (_initAlready) return;

        // UI 요소 자동 바인딩
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));

        // 마우스 클릭 이벤트 연결
        Button startBtn = Get<Button>((int)Buttons.StartButton);
        if (startBtn != null)
        {
            startBtn.onClick.RemoveAllListeners();
            startBtn.onClick.AddListener(OnClickStart);
        }

        Button exitBtn = Get<Button>((int)Buttons.ExitButton);
        if (exitBtn != null)
        {
            exitBtn.onClick.RemoveAllListeners();
            exitBtn.onClick.AddListener(OnClickExit);
        }

        _initAlready = true;
    }

    private void Start()
    {
        Init();

        // 1. 메인 메뉴가 켜진 동안 게임 시간 일시정지
        Time.timeScale = 0f;

        // 2. 게임 시작 전까지 인게임 UI 비활성화 (나타나지 않도록 처리)
        if (inGameSceneUI != null)
        {
            inGameSceneUI.gameObject.SetActive(false);
        }

        UpdateMenuVisual();
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        // 키보드 방향키 및 엔터 입력 지원
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
    /// 선택된 메뉴 글자 색상 변경 및 화살표 위치 고정 연동
    /// </summary>
    /// <summary>
    /// 선택된 메뉴 글자 색상 변경 및 화살표 Y축 위치 연동
    /// </summary>
    private void UpdateMenuVisual()
    {
        Button startBtn = Get<Button>((int)Buttons.StartButton);
        Button exitBtn = Get<Button>((int)Buttons.ExitButton);
        Image arrowImg = Get<Image>((int)Images.SelectionArrowImage);

        if (startBtn == null || exitBtn == null) return;

        Text startText = startBtn.GetComponentInChildren<Text>();
        Text exitText = exitBtn.GetComponentInChildren<Text>();

        // 1. 선택 메뉴 글자 색상 (노란색 / 흰색)
        if (_selectedIndex == 0)
        {
            if (startText != null) startText.color = Color.yellow;
            if (exitText != null) exitText.color = Color.white;
        }
        else
        {
            if (startText != null) startText.color = Color.white;
            if (exitText != null) exitText.color = Color.yellow;
        }

        // 2. 화살표 X축 고정 / Y축 위치 연동
        Button targetButton = (_selectedIndex == 0) ? startBtn : exitBtn;
        if (arrowImg != null && targetButton != null)
        {
            RectTransform arrowRect = arrowImg.rectTransform;
            RectTransform targetRect = targetButton.GetComponent<RectTransform>();

            // 화살표의 현재 X 좌표는 그대로 유지하고, Y 좌표만 선택된 버튼의 Y 좌표로 변경
            Vector2 arrowPos = arrowRect.anchoredPosition;
            arrowPos.y = targetRect.anchoredPosition.y;
            arrowRect.anchoredPosition = arrowPos;
        }
    }

    private void ExecuteSelectedMenu()
    {
        if (_selectedIndex == 0)
        {
            OnClickStart();
        }
        else if (_selectedIndex == 1)
        {
            OnClickExit();
        }
    }

    /// <summary>
    /// [시작] 버튼 클릭 시 동작
    /// </summary>
    public void OnClickStart()
    {
        // 1. 메인 메뉴 UI 비활성화
        gameObject.SetActive(false);

        // 2. 게임 시간을 정상 상태로 전환
        Time.timeScale = 1f;

        // 3. 게임 시작 시점에 인게임 UI 활성화 및 초기화 진행
        if (inGameSceneUI != null)
        {
            inGameSceneUI.gameObject.SetActive(true);
            inGameSceneUI.Init();
        }
    }

    /// <summary>
    /// [게임 종료] 버튼 클릭 시 동작
    /// </summary>
    public void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}