#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(UnityObjectReference))]
public class UnityObjectReferenceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 래퍼 구조체 내부에 있는 'targetUnityObject' 필드를 찾아옵니다.
        SerializedProperty objectProp = property.FindPropertyRelative("targetUnityObject");

        EditorGUI.BeginProperty(position, label, property);

        // 인스펙터상에서 유니티 기본 GameObject 드롭 필드로 강제 렌더링합니다.
        objectProp.objectReferenceValue = EditorGUI.ObjectField(
            position,
            label,
            objectProp.objectReferenceValue,
            typeof(GameObject),
            true // Scene Object(씬 하이어라키 개체) 허용 여부
        );

        EditorGUI.EndProperty();
    }
}
#endif
