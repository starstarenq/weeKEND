using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public abstract class UIBase : MonoBehaviour
{
    // 컴포넌트 타입별로 오브젝트들을 관리할 딕셔너리
    private Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();

    protected bool _init = false;

    public virtual void Init()
    {
        if (_init) return;
        _init = true;
    }

    private void Start()
    {
        Init();
    }

    // [핵심] Enum을 기반으로 UI 오브젝트들을 자동으로 찾아서 바인딩하는 함수
    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        string[] names = Enum.GetNames(type);
        UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
        _objects.Add(typeof(T), objects);

        for (int i = 0; i < names.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
                objects[i] = Util.FindChild(gameObject, names[i], true);
            else
                objects[i] = Util.FindChild<T>(gameObject, names[i], true);

            if (objects[i] == null)
                Debug.LogWarning($"[UIBase] 바인딩 실패: {names[i]} ({typeof(T).Name})");
        }
    }

    // 바인딩된 오브젝트를 가져오는 함수
    protected T Get<T>(int idx) where T : UnityEngine.Object
    {
        if (_objects.TryGetValue(typeof(T), out UnityEngine.Object[] objects) == false)
            return null;

        return objects[idx] as T;
    }

    // 자주 사용하는 컴포넌트들을 쉽게 꺼내 쓰기 위한 숏컷 함수들
    protected Button GetButton(int idx) => Get<Button>(idx);
    protected Image GetImage(int idx) => Get<Image>(idx);
    protected Text GetText(int idx) => Get<Text>(idx);
    protected GameObject GetObject(int idx) => Get<GameObject>(idx);
}
