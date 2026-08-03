using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Header("Scene UI")]
    [SerializeField] private UI_InGameScene inGameSceneUI;

    [Header("Pop UI")]
    // 부모 클래스가 PopUI 구조이므로 확장성을 위해 UI_PausePopup 컴포넌트로 연결합니다.
    [SerializeField] private UI_PausePopup pausePopupUI;

    [Header("Game Objects")]
    [SerializeField] private GameObject enemyObject;

    private float _currentEmotion = 50f; // 초기 감정 값

    private void Update()
    {
        // 1. Q 입력 시 적 파괴 및 감정 게이지 30 상승
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (enemyObject != null)
            {
                Destroy(enemyObject);
                Debug.Log("적이 처치되었습니다! 오브젝트가 삭제됩니다.");

                _currentEmotion = Mathf.Clamp(_currentEmotion + 30f, 0f, 100f);

                if (inGameSceneUI != null)
                {
                    inGameSceneUI.UpdateEmotion(_currentEmotion);
                }
            }
            else
            {
                Debug.LogWarning("처치할 적이 이미 존재하지 않습니다.");
            }
        }

        // 2. ESC 입력 시 일시정지 팝업(PopUI 상속 구조) 토글 기능
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausePopupUI != null)
            {
                // 팝업이 이미 켜져 있다면 부모(PopUI)의 ClosePopup 호출
                if (pausePopupUI.gameObject.activeSelf)
                {
                    pausePopupUI.ClosePopup();
                }
                // 팝업이 꺼져 있다면 부모(PopUI)의 ShowPopup 호출
                else
                {
                    pausePopupUI.ShowPopup();
                }
            }
        }
    }
}
