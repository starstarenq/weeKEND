using UnityEngine;
using UnityEditor;

public class MonsterGenerator : EditorWindow
{
    private string monsterName = "Cute Cube Monster";
    private Color bodyColor = Color.green;
    private Color hornColor = Color.red;

    [MenuItem("Tools/Create Simple 2D Monster")]
    public static void ShowWindow()
    {
        GetWindow<MonsterGenerator>("Monster Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("2D Monster Generator Settings", EditorStyles.boldLabel);

        monsterName = EditorGUILayout.TextField("Monster Name", monsterName);
        bodyColor = EditorGUILayout.ColorField("Body Color", bodyColor);
        hornColor = EditorGUILayout.ColorField("Horn Color", hornColor);

        GUILayout.Space(20);

        if (GUILayout.Button("Generate Monster in Scene", GUILayout.Height(40)))
        {
            CreateMonster();
        }
    }

    private void CreateMonster()
    {
        // 1. 최상위 몬스터 빈 오브젝트 생성
        GameObject monsterRoot = new GameObject(monsterName);
        monsterRoot.transform.position = Vector3.zero;

        // 2. 몸통 생성 (Square)
        GameObject body = new GameObject("Body");
        body.transform.SetParent(monsterRoot.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(2f, 2f, 1f);

        SpriteRenderer bodyRenderer = body.AddComponent<SpriteRenderer>();
        SetPrimitiveSprite(bodyRenderer, "Square");
        bodyRenderer.color = bodyColor;
        bodyRenderer.sortingOrder = 1;

        // 3. 뿔 생성 (Triangle - 왼쪽)
        GameObject leftHorn = new GameObject("Left Horn");
        leftHorn.transform.SetParent(monsterRoot.transform);
        leftHorn.transform.localPosition = new Vector3(-0.6f, 1.3f, 0f);
        leftHorn.transform.localScale = new Vector3(0.6f, 0.8f, 1f);
        leftHorn.transform.localRotation = Quaternion.Euler(0, 0, 15f);

        SpriteRenderer leftHornRenderer = leftHorn.AddComponent<SpriteRenderer>();
        SetPrimitiveSprite(leftHornRenderer, "Triangle");
        leftHornRenderer.color = hornColor;
        leftHornRenderer.sortingOrder = 0; // 몸통 뒤로

        // 4. 뿔 생성 (Triangle - 오른쪽)
        GameObject rightHorn = new GameObject("Right Horn");
        rightHorn.transform.SetParent(monsterRoot.transform);
        rightHorn.transform.localPosition = new Vector3(0.6f, 1.3f, 0f);
        rightHorn.transform.localScale = new Vector3(0.6f, 0.8f, 1f);
        rightHorn.transform.localRotation = Quaternion.Euler(0, 0, -15f);

        SpriteRenderer rightHornRenderer = rightHorn.AddComponent<SpriteRenderer>();
        SetPrimitiveSprite(rightHornRenderer, "Triangle");
        rightHornRenderer.color = hornColor;
        rightHornRenderer.sortingOrder = 0;

        // 5. 눈 생성 (Square - 왼쪽)
        GameObject leftEye = new GameObject("Left Eye");
        leftEye.transform.SetParent(monsterRoot.transform);
        leftEye.transform.localPosition = new Vector3(-0.4f, 0.3f, 0f);
        leftEye.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

        SpriteRenderer leftEyeRenderer = leftEye.AddComponent<SpriteRenderer>();
        SetPrimitiveSprite(leftEyeRenderer, "Square");
        leftEyeRenderer.color = Color.white;
        leftEyeRenderer.sortingOrder = 2;

        // 6. 눈 생성 (Square - 오른쪽)
        GameObject rightEye = new GameObject("Right Eye");
        rightEye.transform.SetParent(monsterRoot.transform);
        rightEye.transform.localPosition = new Vector3(0.4f, 0.3f, 0f);
        rightEye.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

        SpriteRenderer rightEyeRenderer = rightEye.AddComponent<SpriteRenderer>();
        SetPrimitiveSprite(rightEyeRenderer, "Square");
        rightEyeRenderer.color = Color.white;
        rightEyeRenderer.sortingOrder = 2;

        // 에디터 Undo 등록 및 자동 선택
        Undo.RegisterCreatedObjectUndo(monsterRoot, "Create Monster");
        Selection.activeGameObject = monsterRoot;
    }

    /// <summary>
    /// 에러 해결의 핵심: 유니티 에디터 내장 스탠다드 2D 스프라이트 패스 매핑
    /// </summary>
    private void SetPrimitiveSprite(SpriteRenderer renderer, string shape)
    {
        Sprite spr = null;

        if (shape == "Square")
        {
            // 유니티 공식 기본 Square 스프라이트 에셋 경로
            spr = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        }
        else if (shape == "Triangle")
        {
            // 유니티 2D 패키지 내장 기본 Triangle 혹은 유니티 에디터 폴백 처리
            spr = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
        }

        // 만약 여전히 null인 경우를 대비한 런타임 텍스처 생성형 폴백(절대 터지지 않음)
        if (spr == null)
        {
            Texture2D tex = new Texture2D(32, 32);
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    tex.SetPixel(x, y, Color.white);
                }
            }
            tex.Apply();
            spr = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        }

        renderer.sprite = spr;
    }
}
