using UnityEngine;

public class TestInputManager : MonoBehaviour
{
    public static TestInputManager Instance { get; private set; }

    [Header("Scene UI")]
    [SerializeField] private UI_InGameScene inGameSceneUI;

    [Header("Pop UI")]
    [SerializeField] private UI_PausePopup pausePopupUI;
    [SerializeField] private UI_TraitBook traitBookUI;

    [Header("Player Reference")]
    [SerializeField] private PlayerAttack playerAttack;

    [Header("Game Objects")]
    [SerializeField] private GameObject enemyObject;

    private float _currentEmotion = 50f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 처음 게임을 시작할 때는 특성 사전(도감) 화면이 확실하게 꺼져 있도록 비활성화합니다[cite: 2].
        if (traitBookUI != null)
        {
            traitBookUI.gameObject.SetActive(false);
        }

        if (playerAttack == null)
        {
            playerAttack = FindFirstObjectByType<PlayerAttack>();
        }
    }

    private void Update()
    {
        // 1. Q 키 입력: 적 오브젝트를 파괴하고 감정 게이지 수치를 30 증가시킵니다[cite: 2].
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (enemyObject != null)
            {
                Destroy(enemyObject);
                _currentEmotion = Mathf.Clamp(_currentEmotion + 30f, 0f, 100f);
                if (inGameSceneUI != null)
                {
                    inGameSceneUI.UpdateEmotion(_currentEmotion);
                }
            }
        }

        // 2. ESC 키 입력: 팝업 UI 창 닫기 및 일시정지 메뉴 토글[cite: 2]
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 특성 사전(도감)이 열려있다면 ESC 키로 도감부터 먼저 닫습니다[cite: 2].
            if (traitBookUI != null && traitBookUI.gameObject.activeSelf)
            {
                traitBookUI.ClosePopup();
            }
            // 켜진 팝업이 없다면 일반 일시정지 창을 켜거나 끕니다[cite: 2].
            else if (pausePopupUI != null)
            {
                if (pausePopupUI.gameObject.activeSelf)
                    pausePopupUI.ClosePopup();
                else
                    pausePopupUI.ShowPopup();
            }
        }

        // 3. O 키 입력: 특성 사전(도감) 열고 닫기 토글[cite: 2]
        if (Input.GetKeyDown(KeyCode.O))
        {
            // 일시정지 메뉴가 이미 켜져 있다면 도감이 열리지 않도록 방어합니다[cite: 2].
            if (pausePopupUI != null && pausePopupUI.gameObject.activeSelf) return;

            if (traitBookUI != null)
            {
                // 이미 켜져 있다면 닫고, 꺼져 있다면 엽니다[cite: 2].
                if (traitBookUI.gameObject.activeSelf)
                {
                    traitBookUI.ClosePopup();
                }
                else
                {
                    traitBookUI.ShowPopup();
                }
            }
        }
    }

    /// <summary>
    /// 팝업 UI(일시정지, 도감)가 활성화되어 있어 플레이어 조작 및 스킬 사용을 차단해야 하는지 확인합니다[cite: 2].
    /// </summary>
    public bool IsUIBlockingInput()
    {
        bool isPauseOpen = pausePopupUI != null && pausePopupUI != null && pausePopupUI.gameObject.activeSelf;
        bool isTraitOpen = traitBookUI != null && traitBookUI.gameObject.activeSelf;
        return isPauseOpen || isTraitOpen;
    }
}