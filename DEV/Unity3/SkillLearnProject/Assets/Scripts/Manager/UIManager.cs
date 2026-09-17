using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    // 현재 켜져 있는 씬 UI 관리
    private SceneUI _sceneUI;

    // UI가 생성될 기준 루트 오브젝트
    private GameObject _uiRoot;
    public GameObject Root
    {
        get
        {
            if (_uiRoot == null)
            {
                _uiRoot = GameObject.Find("@UI_Root");
                if (_uiRoot == null)
                    _uiRoot = new GameObject { name = "@UI_Root" };
            }
            return _uiRoot;
        }
    }

    // Generic을 활용해 어떤 SceneUI든 동적으로 생성하는 메서드
    public T ShowSceneUI<T>(string name = null) where T : SceneUI
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name; // 이름을 안 주면 클래스 이름과 동일한 프리팹을 찾음

        // Resources/Prefabs/UI/ 폴더 내에 프리팹이 있다고 가정
        GameObject prefab = Resources.Load<GameObject>($"Prefabs/UI/{name}");
        if (prefab == null)
        {
            Debug.LogError($"[UIManager] 프리팹을 찾을 수 없습니다: Prefabs/UI/{name}");
            return null;
        }

        GameObject go = Instantiate(prefab);
        T sceneUI = go.GetComponent<T>();
        if (sceneUI == null)
            sceneUI = go.AddComponent<T>();

        _sceneUI = sceneUI;
        go.transform.SetParent(Root.transform);

        return sceneUI;
    }
}
