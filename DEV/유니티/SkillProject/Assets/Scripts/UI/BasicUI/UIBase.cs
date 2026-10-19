using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIBase : MonoBehaviour
{
    // UI 컴포넌트들을 타입별로 보관할 딕셔너리
    private Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();

    // 상속받은 자식들이 반드시 구현해야 하는 초기화 함수
    public abstract void Init();

    private void Awake()
    {
        Init();
    }

    /// <summary>
    /// 자식 UI에서 지정한 컴포넌트 타입들을 자동으로 찾아 자식 오브젝트 배열에 등록합니다.
    /// </summary>
    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        if (_objects.ContainsKey(typeof(T)))
        {
            Debug.LogWarning($"[UIBase] {typeof(T).Name} 타입이 이미 바인딩되어 있어 건너뜁니다.");
            return;
        }

        string[] names = Enum.GetNames(type);
        UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
        _objects.Add(typeof(T), objects);

        for (int i = 0; i < names.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
                objects[i] = FindChildGameObject(names[i]);
            else
                objects[i] = FindChildComponent<T>(names[i]);
        }
    }

    /// <summary>
    /// 등록된 UI 컴포넌트를 ID(인덱스)로 안전하게 꺼내옵니다.
    /// </summary>
    protected T Get<T>(int idx) where T : UnityEngine.Object
    {
        UnityEngine.Object[] objects = null;
        if (_objects.TryGetValue(typeof(T), out objects) == false)
            return null;

        return objects[idx] as T;
    }

    // 이름 기반 하위 오브젝트/컴포넌트 탐색용 헬퍼 함수들
    private GameObject FindChildGameObject(string name)
    {
        Transform[] transforms = GetComponentsInChildren<Transform>(true);
        foreach (Transform t in transforms)
        {
            if (t.name == name) return t.gameObject;
        }
        return null;
    }

    private T FindChildComponent<T>(string name) where T : UnityEngine.Object
    {
        T[] components = GetComponentsInChildren<T>(true);
        foreach (T component in components)
        {
            if (component.name == name) return component;
        }
        return null;
    }
}
