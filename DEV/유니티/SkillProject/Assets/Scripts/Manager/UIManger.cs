using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("@UIManager");
                _instance = obj.AddComponent<UIManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 팝업 UI 스택 관리
    private Stack<PopUI> _popupStack = new Stack<PopUI>();

    // 현재 씬 UI 참조 (UI_InGameScene 등의 접근을 위해 제공)
    private SceneUI _sceneUI = null;
    public SceneUI CurrentSceneUI => _sceneUI;

    /// <summary>
    /// 씬 UI를 생성하거나 등록합니다.
    /// </summary>
    public T ShowSceneUI<T>(string path = null) where T : SceneUI
    {
        if (_sceneUI != null)
        {
            Destroy(_sceneUI.gameObject);
            _sceneUI = null;
        }

        string prefabPath = string.IsNullOrEmpty(path) ? $"UI/Scene/{typeof(T).Name}" : path;
        GameObject prefab = Resources.Load<GameObject>(prefabPath);

        if (prefab == null)
        {
            // 리소스를 못 찾았을 경우 씬 내에 이미 존재하는지 탐색
            T existingUI = FindAnyObjectByType<T>(FindObjectsInactive.Include);
            if (existingUI != null)
            {
                _sceneUI = existingUI;
                _sceneUI.gameObject.SetActive(true);
                return existingUI;
            }

            Debug.LogError($"[UIManager] SceneUI 프리팹 및 오브젝트를 찾을 수 없습니다: {typeof(T).Name}");
            return null;
        }

        GameObject obj = Instantiate(prefab);
        T sceneUI = obj.GetComponent<T>();

        if (sceneUI == null)
        {
            Debug.LogError($"[UIManager] 프리팹에 {typeof(T).Name} 컴포넌트가 없습니다.");
            return null;
        }

        _sceneUI = sceneUI;
        return sceneUI;
    }

    /// <summary>
    /// 서브 UI를 생성합니다.
    /// </summary>
    public T ShowSubUI<T>(Transform parent = null, string path = null) where T : SubUI
    {
        string prefabPath = string.IsNullOrEmpty(path) ? $"UI/Sub/{typeof(T).Name}" : path;
        GameObject prefab = Resources.Load<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"[UIManager] SubUI 프리팹을 찾을 수 없습니다: {prefabPath}");
            return null;
        }

        GameObject obj = Instantiate(prefab, parent);
        T subUI = obj.GetComponent<T>();
        return subUI;
    }

    /// <summary>
    /// 팝업 UI를 프리팹으로부터 생성하여 스택에 쌓고 엽니다.
    /// </summary>
    public T ShowPopupUI<T>(string path = null) where T : PopUI
    {
        string prefabPath = string.IsNullOrEmpty(path) ? $"UI/Popup/{typeof(T).Name}" : path;
        GameObject prefab = Resources.Load<GameObject>(prefabPath);

        GameObject obj = null;
        if (prefab != null)
        {
            obj = Instantiate(prefab);
        }
        else
        {
            // 씬 내에 미리 배치되어 있는 팝업인 경우 탐색 시도 (예: UI_TraitBook 등)
            T existingPopup = FindAnyObjectByType<T>(FindObjectsInactive.Include);
            if (existingPopup != null)
            {
                obj = existingPopup.gameObject;
            }
        }

        if (obj == null)
        {
            Debug.LogError($"[UIManager] PopupUI를 찾거나 생성할 수 없습니다: {typeof(T).Name}");
            return null;
        }

        T popup = obj.GetComponent<T>();
        if (popup != null)
        {
            _popupStack.Push(popup);
            popup.ShowPopup();
        }

        return popup;
    }

    /// <summary>
    /// 이미 존재하는 팝업 인스턴스를 직접 스택에 등록하고 열 때 사용합니다.
    /// </summary>
    public void ShowPopupUI(PopUI popup)
    {
        if (popup == null) return;
        _popupStack.Push(popup);
        popup.ShowPopup();
    }

    /// <summary>
    /// 최상단 팝업 UI를 닫습니다. (파괴 대신 비활성화 지원 옵션 추가 가능)
    /// </summary>
    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
        {
            Debug.LogWarning("[UIManager] 닫을 팝업이 없습니다.");
            return;
        }

        PopUI popup = _popupStack.Pop();
        if (popup != null)
        {
            popup.ClosePopup();

            // 만약 씬 내에 미리 배치된 오브젝트라면 Destroy 대신 SetActive(false) 처리가 안전할 수 있음
            // 동적 생성된 프리팹인 경우에만 Destroy 하도록 분기 처리
            if (popup.gameObject.scene.IsValid() && !popup.gameObject.name.Contains("(Clone)"))
            {
                popup.gameObject.SetActive(false);
            }
            else
            {
                Destroy(popup.gameObject);
            }
        }
    }

    /// <summary>
    /// 모든 팝업 UI를 일괄 닫습니다.
    /// </summary>
    public void ClearAllPopups()
    {
        while (_popupStack.Count > 0)
        {
            PopUI popup = _popupStack.Pop();
            if (popup != null)
            {
                popup.ClosePopup();
                if (!popup.gameObject.name.Contains("(Clone)"))
                {
                    popup.gameObject.SetActive(false);
                }
                else
                {
                    Destroy(popup.gameObject);
                }
            }
        }
    }
}