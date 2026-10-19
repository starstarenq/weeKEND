using UnityEngine;
using UnityEditor;
using System.IO;

public class EnemyDataEditor
{
    [MenuItem("Tools/Create Enemy Data Asset")]
    public static void CreateEnemyDataAsset()
    {
        // ScriptableObject 인스턴스 생성
        EnemyData asset = ScriptableObject.CreateInstance<EnemyData>();

        // 저장할 경로 설정 (Assets/Resources 또는 Assets/Data 등 원하는 경로)
        string dirPath = "Assets/Data";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{dirPath}/EnemyData.asset");

        // 프로젝트에 에셋 파일로 생성
        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();

        // 생성된 에셋을 에디터 하이라이트 및 포커스
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        Debug.Log($"[EnemyData] 새 적 데이터 오브젝트가 생성되었습니다: {assetPath}");
    }
}