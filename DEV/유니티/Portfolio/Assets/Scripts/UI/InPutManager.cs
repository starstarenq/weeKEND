using UnityEngine;

public class TestInputManager : MonoBehaviour
{
    [Header("Scene UI")]
    [SerializeField] private UI_InGameScene inGameSceneUI;

    [Header("Pop UI")]
    [SerializeField] private UI_PausePopup pausePopupUI;
    [SerializeField] private UI_TraitBook traitBookUI;

    [Header("Game Objects")]
    [SerializeField] private GameObject enemyObject;

    private float _currentEmotion = 50f;

    private void Start()
    {
        // 처음 게임을 시작할 때는 특성 사전(도감) 화면이 확실하게 꺼져 있도록 비활성화합니다.
        if (traitBookUI != null)
        {
            traitBookUI.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // 1. Q 키 입력: 적 오브젝트를 파괴하고 감정 게이지 수치를 30 증가시킵니다.
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

        // 2. ESC 키 입력: 팝업 UI 창 닫기 및 일시정지 메뉴 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 특성 사전(도감)이 열려있다면 ESC 키로 도감부터 먼저 닫습니다.
            if (traitBookUI != null && traitBookUI.gameObject.activeSelf)
            {
                traitBookUI.ClosePopup();
            }
            // 켜진 팝업이 없다면 일반 일시정지 창을 켜거나 끕니다.
            else if (pausePopupUI != null)
            {
                if (pausePopupUI.gameObject.activeSelf)
                    pausePopupUI.ClosePopup();
                else
                    pausePopupUI.ShowPopup();
            }
        }

        // 3. O 키 입력: 특성 사전(도감) 열고 닫기 토글
        if (Input.GetKeyDown(KeyCode.O))
        {
            // 일시정지 메뉴가 이미 켜져 있다면 도감이 열리지 않도록 방어합니다.
            if (pausePopupUI != null && pausePopupUI.gameObject.activeSelf) return;

            if (traitBookUI != null)
            {
                // 이미 켜져 있다면 닫고, 꺼져 있다면 엽니다.
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

}