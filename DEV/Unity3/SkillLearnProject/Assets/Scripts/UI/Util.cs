using UnityEngine;

public static class Util
{
    // 특정 컴포넌트를 가진 자식을 이름으로 탐색
    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null) return null;

        if (recursive == false)
        {
            Transform transform = go.transform.Find(name);
            if (transform != null) return transform.GetComponent<T>();
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>(true))
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }
        return null;
    }

    // GameObject 자체를 이름으로 탐색
    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        return transform != null ? transform.gameObject : null;
    }
}