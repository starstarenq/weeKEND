#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class WeaponDataImporterWindow : EditorWindow
{
    private enum ImportSource { TextAsset, RawString }
    private ImportSource currentSource = ImportSource.TextAsset;

    private TextAsset jsonFile;
    private string rawJsonString = "";
    private Vector2 scrollPosition;

    [MenuItem("Tools/Weapon Data Importer")]
    public static void ShowWindow()
    {
        WeaponDataImporterWindow window = GetWindow<WeaponDataImporterWindow>("Weapon Importer");
        window.minSize = new Vector2(400, 350);
    }

    private void OnGUI()
    {
        // 타이틀 및 설명 레이아웃
        GUILayout.Label("Weapon Data JSON to ScriptableObject Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 입력 방식 선택 탭 (EditorGUILayout.EnumPopup 사용)
        currentSource = (ImportSource)EditorGUILayout.EnumPopup("JSON 입력 방식", currentSource);
        EditorGUILayout.Space();

        // 선택한 방식에 따른 EditorGUILayout 인터페이스 구성
        if (currentSource == ImportSource.TextAsset)
        {
            jsonFile = (TextAsset)EditorGUILayout.ObjectField("JSON 파일 에셋", jsonFile, typeof(TextAsset), false);
        }
        else
        {
            EditorGUILayout.LabelField("JSON 문자열 직접 입력:");
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
            rawJsonString = EditorGUILayout.TextArea(rawJsonString, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.Space(20);

        // 생성 버튼 레이아웃 (GUI 스킨 색상 적용으로 가시성 확보)
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("ScriptableObject 생성 시작", GUILayout.Height(40)))
        {
            ProcessImport();
        }
        GUI.backgroundColor = Color.white;
    }

    private void ProcessImport()
    {
        string targetJsonString = "";

        // 1. 소스 타입에 따른 JSON 문자열 확보
        if (currentSource == ImportSource.TextAsset)
        {
            if (jsonFile == null)
            {
                EditorUtility.DisplayDialog("경고", "JSON 파일 에셋을 지정해주세요.", "확인");
                return;
            }
            targetJsonString = jsonFile.text;
        }
        else
        {
            if (string.IsNullOrEmpty(rawJsonString.Trim()))
            {
                EditorUtility.DisplayDialog("경고", "JSON 문자열을 입력해주세요.", "확인");
                return;
            }
            targetJsonString = rawJsonString;
        }

        // 2. 유니티 JsonUtility 파싱을 위한 래핑 처리
        string wrappedJson = "{\"weapons\":" + targetJsonString + "}";
        WeaponDataContainer dataContainer = null;

        try
        {
            dataContainer = JsonUtility.FromJson<WeaponDataContainer>(wrappedJson);
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("에러", $"JSON 파싱 중 오류가 발생했습니다:\n{ex.Message}", "확인");
            return;
        }

        if (dataContainer == null || dataContainer.weapons == null || dataContainer.weapons.Count == 0)
        {
            EditorUtility.DisplayDialog("에러", "파싱된 무기 데이터가 없습니다. JSON 형식을 확인하세요.", "확인");
            return;
        }

        // 3. 대상 폴더 구성 (Assets/Data/WeaponData)
        string baseFolder = "Assets/Data";
        string targetFolder = "Assets/Data/WeaponData";

        if (!AssetDatabase.IsValidFolder("Assets/Data"))
        {
            AssetDatabase.CreateFolder("Assets", "Data");
        }
        if (!AssetDatabase.IsValidFolder(targetFolder))
        {
            AssetDatabase.CreateFolder(baseFolder, "WeaponData");
        }

        // 4. 데이터 기반 ScriptableObject 에셋 생성
        int successCount = 0;
        foreach (var weapon in dataContainer.weapons)
        {
            WeaponItemData asset = ScriptableObject.CreateInstance<WeaponItemData>();

            // 데이터 필드 매핑
            asset.Name = weapon.Name;
            asset.Korean_Name = weapon.Korean_Name;
            asset.Base_Damage_Min = weapon.Base_Damage_Min;
            asset.Base_Damage_Max = weapon.Base_Damage_Max;
            asset.Cooldown_Min = weapon.Cooldown_Min;
            asset.Cooldown_Max = weapon.Cooldown_Max;
            asset.Amount_Min = weapon.Amount_Min;
            asset.Amount_Max = weapon.Amount_Max;

            // 파일 이름에서 특수문자 및 공백 제거
            string safeName = weapon.Name.Replace(" ", "");
            string assetPath = $"{targetFolder}/{safeName}Data.asset";

            // 파일 저장
            AssetDatabase.CreateAsset(asset, assetPath);
            successCount++;
        }

        // 데이터 동기화 및 에디터 갱신
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("성공", $"총 {successCount}개의 무기 데이터가 '{targetFolder}'에 성공적으로 생성되었습니다!", "확인");
    }

    // 내부 JSON 역직렬화를 위한 임시 데이터 컨테이너 구조
    [System.Serializable]
    private class WeaponItemWrapper
    {
        public string Name;
        public string Korean_Name;
        public int Base_Damage_Min;
        public int Base_Damage_Max;
        public float Cooldown_Min;
        public float Cooldown_Max;
        public int Amount_Min;
        public int Amount_Max;
    }

    [System.Serializable]
    private class WeaponDataContainer
    {
        public List<WeaponItemWrapper> weapons;
    }
}
#endif
