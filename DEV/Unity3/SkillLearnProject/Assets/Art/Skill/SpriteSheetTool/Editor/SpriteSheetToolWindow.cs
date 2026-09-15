using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SysKill.EditorTools
{
    /// <summary>
    /// Art 폴더의 스프라이트 시트를 Animator / Prefab 으로 일괄 생성하는 에디터 창.
    /// 메뉴: Tools > SysKill > Sprite Sheet Tool
    /// </summary>
    public sealed class SpriteSheetToolWindow : EditorWindow
    {
        [Serializable]
        sealed class Row
        {
            public DefaultAsset sourceFolder; // Art 하위 폴더
            public string characterName = "";
            public string outputFolder = "";
        }

        const string DefaultArtRoot = "Assets/Art";
        const string DefaultOutputRoot = "Assets/Generated";

        [SerializeField] List<Row> _rows = new();
        [SerializeField] int _frameRate = 10;
        [SerializeField] SpriteSheetGenerator.PivotMode _pivot = SpriteSheetGenerator.PivotMode.BottomCenter;
        [SerializeField] int _pixelsPerUnit = 0;
        [SerializeField] int _fallbackFrameSize = 64;
        [SerializeField] bool _sliceSprites = true;
        [SerializeField] bool _buildClips = true;
        [SerializeField] bool _buildAnimator = true;
        [SerializeField] bool _buildPrefab = true;
        [SerializeField] bool _attachControllerScript = true;

        Vector2 _scroll;
        string _resultLog = "";

        [MenuItem("Tools/SysKill/Sprite Sheet Tool")]
        public static void Open()
        {
            var w = GetWindow<SpriteSheetToolWindow>("Sprite Sheet Tool");
            w.minSize = new Vector2(560, 480);
            w.Show();
        }

        void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.HelpBox(
                "Art 폴더의 스프라이트 시트를 슬라이스하여 AnimationClip / 4방향 BlendTree Animator / Prefab 을 생성합니다.\n" +
                "폴더에 manifest.json 이 있으면 자동으로 정확히 슬라이스하고, 없으면 아래 '수동 그리드 프레임 크기'로 폴백합니다.",
                MessageType.Info);

            DrawGlobalOptions();
            EditorGUILayout.Space(6);
            DrawEntries();
            EditorGUILayout.Space(6);
            DrawActions();
            EditorGUILayout.Space(6);
            DrawResults();

            EditorGUILayout.EndScrollView();
        }

        void DrawGlobalOptions()
        {
            EditorGUILayout.LabelField("전역 옵션", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                _frameRate = Mathf.Max(1, EditorGUILayout.IntField(new GUIContent("프레임레이트(FPS)", "생성되는 AnimationClip 의 초당 프레임 수"), _frameRate));
                _pivot = (SpriteSheetGenerator.PivotMode)EditorGUILayout.EnumPopup(new GUIContent("피벗", "스프라이트 피벗 위치"), _pivot);
                _pixelsPerUnit = EditorGUILayout.IntField(new GUIContent("Pixels Per Unit", "0 이면 프레임 크기(frameSize)를 사용"), _pixelsPerUnit);
                _fallbackFrameSize = Mathf.Max(1, EditorGUILayout.IntField(new GUIContent("수동 그리드 프레임 크기", "manifest.json 이 없을 때 사용하는 셀 크기(px)"), _fallbackFrameSize));

                EditorGUILayout.Space(2);
                using (new EditorGUILayout.HorizontalScope())
                {
                    _sliceSprites = GUILayout.Toggle(_sliceSprites, " 슬라이스", "Button");
                    _buildClips = GUILayout.Toggle(_buildClips, " 클립", "Button");
                    _buildAnimator = GUILayout.Toggle(_buildAnimator, " Animator", "Button");
                    _buildPrefab = GUILayout.Toggle(_buildPrefab, " Prefab", "Button");
                }
                _attachControllerScript = EditorGUILayout.ToggleLeft("Prefab 에 CharacterAnimator 스크립트 부착", _attachControllerScript);
            }
        }

        void DrawEntries()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"대상 목록 ({_rows.Count})", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Art 폴더 스캔", GUILayout.Width(110)))
                    ScanArtFolder();
                if (GUILayout.Button("행 추가", GUILayout.Width(70)))
                    _rows.Add(new Row());
                using (new EditorGUI.DisabledScope(_rows.Count == 0))
                    if (GUILayout.Button("전체 삭제", GUILayout.Width(70)))
                        _rows.Clear();
            }

            int removeAt = -1;
            for (int i = 0; i < _rows.Count; i++)
            {
                var row = _rows[i];
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUI.BeginChangeCheck();
                        var picked = (DefaultAsset)EditorGUILayout.ObjectField(
                            new GUIContent("소스 폴더"), row.sourceFolder, typeof(DefaultAsset), false);
                        if (EditorGUI.EndChangeCheck())
                        {
                            row.sourceFolder = picked;
                            AutoFillRow(row);
                        }
                        if (GUILayout.Button("✕", GUILayout.Width(24)))
                            removeAt = i;
                    }

                    row.characterName = EditorGUILayout.TextField("캐릭터 이름", row.characterName);
                    row.outputFolder = EditorGUILayout.TextField("출력 폴더", row.outputFolder);

                    DrawRowStatus(row);
                }
            }
            if (removeAt >= 0)
                _rows.RemoveAt(removeAt);
        }

        void DrawRowStatus(Row row)
        {
            string folderPath = row.sourceFolder != null ? AssetDatabase.GetAssetPath(row.sourceFolder) : null;
            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
            {
                EditorGUILayout.HelpBox("유효한 폴더를 지정하세요.", MessageType.Warning);
                return;
            }
            bool hasManifest = File.Exists(Path.Combine(ToAbsolute(folderPath), "manifest.json"));
            EditorGUILayout.LabelField(
                hasManifest ? "◆ manifest.json 감지됨 (정확 슬라이스)" : "◇ manifest 없음 → 수동 그리드 폴백",
                EditorStyles.miniLabel);
        }

        void DrawActions()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(_rows.Count == 0))
                {
                    if (GUILayout.Button("모두 생성", GUILayout.Height(30)))
                        GenerateAll();
                }
            }
        }

        void DrawResults()
        {
            if (string.IsNullOrEmpty(_resultLog))
                return;
            EditorGUILayout.LabelField("결과", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(_resultLog, GUILayout.MinHeight(120));
        }

        // ---------------------------------------------------------------
        void ScanArtFolder()
        {
            if (!AssetDatabase.IsValidFolder(DefaultArtRoot))
            {
                EditorUtility.DisplayDialog("Sprite Sheet Tool", $"{DefaultArtRoot} 폴더가 없습니다.", "확인");
                return;
            }

            var existing = new HashSet<string>(
                _rows.Where(r => r.sourceFolder != null)
                     .Select(r => AssetDatabase.GetAssetPath(r.sourceFolder)));

            string artAbs = ToAbsolute(DefaultArtRoot);
            foreach (var manifestPath in Directory.GetFiles(artAbs, "manifest.json", SearchOption.AllDirectories))
            {
                string folderAbs = Path.GetDirectoryName(manifestPath);
                string folderAsset = ToAssetPath(folderAbs);
                if (existing.Contains(folderAsset))
                    continue;

                var folderAsset2 = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folderAsset);
                var row = new Row { sourceFolder = folderAsset2 };
                AutoFillRow(row);
                _rows.Add(row);
            }

            if (_rows.Count == 0)
                _resultLog = "Art 폴더에서 manifest.json 을 찾지 못했습니다.";
        }

        static void AutoFillRow(Row row)
        {
            if (row.sourceFolder == null) return;
            string folderPath = AssetDatabase.GetAssetPath(row.sourceFolder);
            if (!AssetDatabase.IsValidFolder(folderPath)) return;

            string leaf = Path.GetFileName(folderPath);
            string clean = SanitizeName(leaf);
            if (string.IsNullOrEmpty(row.characterName))
                row.characterName = clean;
            if (string.IsNullOrEmpty(row.outputFolder))
                row.outputFolder = $"{DefaultOutputRoot}/{clean}";
        }

        static string SanitizeName(string raw)
        {
            // 타임스탬프/특수문자 정리: 첫 번째 날짜 토큰 이전까지만 사용
            string name = raw;
            int dateIdx = name.IndexOf("_2", StringComparison.Ordinal); // "_2026-..." 형태 컷
            if (dateIdx > 0) name = name.Substring(0, dateIdx);
            var arr = name.Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_').ToArray();
            name = new string(arr).Trim('_');
            return string.IsNullOrEmpty(name) ? "Character" : name;
        }

        void GenerateAll()
        {
            var options = new SpriteSheetGenerator.Options
            {
                frameRate = _frameRate,
                pivot = _pivot,
                pixelsPerUnit = _pixelsPerUnit,
                fallbackFrameSize = _fallbackFrameSize,
                sliceSprites = _sliceSprites,
                buildClips = _buildClips,
                buildAnimator = _buildAnimator,
                buildPrefab = _buildPrefab,
                attachControllerScript = _attachControllerScript,
            };

            var sb = new System.Text.StringBuilder();
            int ok = 0, fail = 0;

            for (int i = 0; i < _rows.Count; i++)
            {
                var row = _rows[i];
                string folderPath = row.sourceFolder != null ? AssetDatabase.GetAssetPath(row.sourceFolder) : null;
                if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
                {
                    sb.AppendLine($"[{i}] 유효하지 않은 소스 폴더 — 건너뜀");
                    fail++;
                    continue;
                }

                EditorUtility.DisplayProgressBar("Sprite Sheet Tool",
                    $"{row.characterName} 생성 중...", (float)i / _rows.Count);

                try
                {
                    var manifest = SpriteSheetManifest.LoadFromFolder(ToAbsolute(folderPath));
                    var entry = new SpriteSheetGenerator.Entry
                    {
                        sourceFolderAssetPath = folderPath,
                        characterName = string.IsNullOrEmpty(row.characterName) ? SanitizeName(Path.GetFileName(folderPath)) : row.characterName,
                        outputFolder = string.IsNullOrEmpty(row.outputFolder) ? $"{DefaultOutputRoot}/{SanitizeName(Path.GetFileName(folderPath))}" : row.outputFolder,
                        manifest = manifest,
                    };
                    sb.AppendLine(SpriteSheetGenerator.Generate(entry, options));
                    ok++;
                }
                catch (Exception e)
                {
                    sb.AppendLine($"[{row.characterName}] 실패: {e.Message}");
                    Debug.LogException(e);
                    fail++;
                }
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            sb.Insert(0, $"=== 완료: 성공 {ok} / 실패 {fail} ===\n");
            _resultLog = sb.ToString();
            Debug.Log(_resultLog);
        }

        // ---------------------------------------------------------------
        static string ToAbsolute(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
            return Path.Combine(projectRoot, assetPath).Replace('\\', '/');
        }

        static string ToAssetPath(string absolutePath)
        {
            absolutePath = absolutePath.Replace('\\', '/');
            string dataPath = Application.dataPath.Replace('\\', '/');
            if (absolutePath.StartsWith(dataPath))
                return "Assets" + absolutePath.Substring(dataPath.Length);
            int idx = absolutePath.IndexOf("/Assets/", StringComparison.Ordinal);
            return idx >= 0 ? absolutePath.Substring(idx + 1) : absolutePath;
        }
    }
}
