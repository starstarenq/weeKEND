using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenuScene : SceneUI
{
    enum Buttons
    {
        StartButton,
        ExitButton
    }

    enum Images
    {
        SelectionArrowImage
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
        ShowMainMenu();
    }

    // 메인 메뉴 화면으로 진입할 때 호출
    public void ShowMainMenu()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f; // 시간 정지

        // 인게임 UI 숨기기
        if (inGameSceneUI != null)
        {
            inGameSceneUI.gameObject.SetActive(false);
        }

        _selectedIndex = 0;
        UpdateMenuVisual();
    }

    private void Update()
    {
        // ⚠️ 메인 메뉴가 비활성화되어 있으면 키 입력을 받지 않음
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

    private void UpdateMenuVisual()
    {
        Button startBtn = Get<Button>((int)Buttons.StartButton);
        Button exitBtn = Get<Button>((int)Buttons.ExitButton);
        Image arrowImg = Get<Image>((int)Images.SelectionArrowImage);

        if (startBtn == null || exitBtn == null) return;

        Text startText = startBtn.GetComponentInChildren<Text>();
        Text exitText = exitBtn.GetComponentInChildren<Text>();

        // 1. 색상 변경
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

        // 2. 화살표 Y축 연동
        Button targetButton = (_selectedIndex == 0) ? startBtn : exitBtn;
        if (arrowImg != null && targetButton != null)
        {
            RectTransform arrowRect = arrowImg.rectTransform;
            RectTransform targetRect = targetButton.GetComponent<RectTransform>();

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

    public void OnClickStart()
    {
        Debug.Log("게임 시작 클릭됨!");
        gameObject.SetActive(false);
        Time.timeScale = 1f; // 시간 다시 흐름

        if (inGameSceneUI != null)
        {
            inGameSceneUI.gameObject.SetActive(true);
            inGameSceneUI.Init();
        }
    }

    public void OnClickExit()
    {
        Debug.Log("게임 종료 클릭됨!");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}