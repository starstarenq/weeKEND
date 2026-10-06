using UnityEngine;

[System.Serializable]
public struct UnityObjectReference : ISerializationCallbackReceiver
{
    // 실제 유니티가 강제로 직렬화 추적할 수 있도록 Object 형태로 저장합니다.
    [SerializeField] private Object targetUnityObject;

    // 런타임에 Property Drawer나 외부에서 접근하여 GameObject로 형변환해 쓸 변수입니다.
    private GameObject cachedGameObject;

    public GameObject Value
    {
        get
        {
            if (cachedGameObject == null && targetUnityObject != null)
            {
                cachedGameObject = targetUnityObject as GameObject;
            }
            return cachedGameObject;
        }
        set
        {
            cachedGameObject = value;
            targetUnityObject = value;
        }
    }

    // 직렬화 되기 직전 (유니티 데이터 저장 시스템 작동 시)
    public void OnBeforeSerialize()
    {
        if (cachedGameObject != null)
        {
            targetUnityObject = cachedGameObject;
        }
    }

    // 역직렬화 직후 (인스펙터가 로드되거나 씬이 켜질 때)
    public void OnAfterDeserialize()
    {
        // 유니티 백엔드 스레드에서 바로 변환을 시도하지 않고 안전하게 값만 대기시킵니다.
    }
}





