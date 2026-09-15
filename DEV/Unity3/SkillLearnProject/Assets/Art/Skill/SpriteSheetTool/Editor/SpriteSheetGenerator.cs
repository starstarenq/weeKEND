using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using SysKill.Characters;

namespace SysKill.EditorTools
{
    /// <summary>
    /// SpriteSheetManifest(또는 수동 그리드 폴백)를 입력받아
    /// 스프라이트 슬라이싱 → AnimationClip → AnimatorController(4방향 BlendTree) → Prefab
    /// 을 생성한다. 순수 로직만 담당하며 UI 는 <see cref="SpriteSheetToolWindow"/> 가 담당.
    /// </summary>
    public static class SpriteSheetGenerator
    {
        public enum PivotMode { Center, BottomCenter }

        public sealed class Options
        {
            public int frameRate = 10;
            public PivotMode pivot = PivotMode.BottomCenter;
            public int pixelsPerUnit = 0; // 0 = frameSize 사용
            public bool sliceSprites = true;
            public bool buildClips = true;
            public bool buildAnimator = true;
            public bool buildPrefab = true;
            public bool attachControllerScript = true;

            // 수동 폴백(매니페스트가 없을 때) 그리드 설정
            public int fallbackFrameSize = 64;
        }

        public sealed class Entry
        {
            public string sourceFolderAssetPath; // 예: "Assets/Art/xxx"
            public string characterName;
            public string outputFolder;          // 예: "Assets/Generated/xxx"
            public SpriteSheetManifest manifest;  // null 이면 수동 폴백
        }

        static readonly Dictionary<string, Vector2> CardinalMap = new()
        {
            { "up", new Vector2(0f, 1f) },
            { "down", new Vector2(0f, -1f) },
            { "left", new Vector2(-1f, 0f) },
            { "right", new Vector2(1f, 0f) },
        };

        static readonly string[] LoopingClips = { "idle", "walk", "run" };

        /// <summary>한 엔트리를 생성한다. 로그 문자열을 반환하고 실패 시 예외를 던진다.</summary>
        public static string Generate(Entry entry, Options options)
        {
            if (string.IsNullOrEmpty(entry.sourceFolderAssetPath))
                throw new ArgumentException("소스 폴더가 지정되지 않았습니다.");
            if (string.IsNullOrEmpty(entry.characterName))
                throw new ArgumentException("캐릭터 이름이 비어 있습니다.");

            var manifest = entry.manifest ?? BuildFallbackManifest(entry, options);
            if (manifest.clips.Count == 0)
                throw new InvalidOperationException($"'{entry.sourceFolderAssetPath}' 에서 처리할 스프라이트 시트를 찾지 못했습니다.");

            int frameSize = manifest.frameSize > 0 ? manifest.frameSize : options.fallbackFrameSize;
            int ppu = options.pixelsPerUnit > 0 ? options.pixelsPerUnit : frameSize;
            var log = new StringBuilder();
            log.AppendLine($"[{entry.characterName}] 생성 시작 (frameSize={frameSize}, ppu={ppu})");

            EnsureFolder(entry.outputFolder);

            // clipName -> (direction -> AnimationClip)
            var clipMap = new Dictionary<string, Dictionary<string, AnimationClip>>();
            // clipName -> (direction -> 첫 프레임 스프라이트) : 프리팹 기본 스프라이트 용도
            var firstSprite = new Dictionary<string, Dictionary<string, Sprite>>();

            // NOTE: 슬라이스(SaveAndReimport)는 즉시 임포트되어야 그 결과 스프라이트로
            // 곧바로 클립을 만들 수 있으므로 StartAssetEditing 배치로 감싸지 않는다.
            foreach (var clip in manifest.clips)
            {
                string pngPath = ResolveAssetPath(entry.sourceFolderAssetPath, clip.file);
                if (string.IsNullOrEmpty(pngPath) || !File.Exists(ToAbsolute(pngPath)))
                {
                    log.AppendLine($"  - 건너뜀(파일 없음): {clip.file}");
                    continue;
                }

                Dictionary<string, Sprite> spritesByName;
                if (options.sliceSprites)
                    spritesByName = SliceSheet(pngPath, clip, frameSize, ppu, options.pivot, log);
                else
                    spritesByName = LoadSprites(pngPath);

                if (options.buildClips)
                    BuildClipsForSheet(entry, options, clip, spritesByName, clipMap, firstSprite, log);
            }

            AnimatorController controller = null;
            if (options.buildAnimator && clipMap.Count > 0)
            {
                controller = BuildAnimator(entry, clipMap, log);
            }

            if (options.buildPrefab)
            {
                BuildPrefab(entry, options, controller, firstSprite, log);
            }

            AssetDatabase.SaveAssets();
            log.AppendLine($"[{entry.characterName}] 완료");
            return log.ToString();
        }

        // ---------------------------------------------------------------
        // 1) 슬라이싱 (모던 ISpriteEditorDataProvider API)
        // ---------------------------------------------------------------
        static Dictionary<string, Sprite> SliceSheet(
            string pngPath, SpriteSheetManifest.ClipDef clip,
            int frameSize, int ppu, PivotMode pivotMode, StringBuilder log)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(pngPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; // 픽셀아트: 사각 메시
            settings.wrapMode = TextureWrapMode.Clamp;
            importer.SetTextureSettings(settings);

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);
            int texW = tex != null ? tex.width : clip.columns * frameSize;
            int texH = tex != null ? tex.height : clip.rows * frameSize;
            string baseName = Path.GetFileNameWithoutExtension(pngPath);

            GetPivot(pivotMode, out var alignment, out var pivot);

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dp = factory.GetSpriteEditorDataProviderFromObject(importer);
            dp.InitSpriteEditorDataProvider();

            // 기존 GUID 재사용(재생성 시 참조 유지)
            var idByName = new Dictionary<string, GUID>();
            foreach (var er in dp.GetSpriteRects())
                idByName[er.name] = er.spriteID;

            var rects = new List<SpriteRect>();
            for (int r = 0; r < clip.rows; r++)
            {
                for (int c = 0; c < clip.columns; c++)
                {
                    float x = c * frameSize;
                    float y = texH - (r + 1) * frameSize; // Unity 텍스처 원점은 좌하단
                    if (x + frameSize > texW || y < 0f)
                        continue; // 시트 범위 밖 방어

                    int index = r * clip.columns + c;
                    string name = $"{baseName}_{index}";
                    var sr = new SpriteRect
                    {
                        name = name,
                        alignment = alignment,
                        pivot = pivot,
                        rect = new Rect(x, y, frameSize, frameSize),
                        border = Vector4.zero,
                        spriteID = idByName.TryGetValue(name, out var gid) ? gid : GUID.Generate(),
                    };
                    rects.Add(sr);
                }
            }

            dp.SetSpriteRects(rects.ToArray());
            var nameIdDp = dp.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (nameIdDp != null)
            {
                var pairs = rects.Select(x => new SpriteNameFileIdPair(x.name, x.spriteID)).ToList();
                nameIdDp.SetNameFileIdPairs(pairs);
            }
            dp.Apply();
            importer.SaveAndReimport();

            log.AppendLine($"  - 슬라이스: {baseName}.png ({clip.columns}x{clip.rows} → {rects.Count}개)");
            return LoadSprites(pngPath);
        }

        static Dictionary<string, Sprite> LoadSprites(string pngPath)
        {
            var dict = new Dictionary<string, Sprite>();
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(pngPath))
                if (obj is Sprite sp)
                    dict[sp.name] = sp;
            return dict;
        }

        // ---------------------------------------------------------------
        // 2) AnimationClip 생성 (방향별)
        // ---------------------------------------------------------------
        static void BuildClipsForSheet(
            Entry entry, Options options, SpriteSheetManifest.ClipDef clip,
            Dictionary<string, Sprite> spritesByName,
            Dictionary<string, Dictionary<string, AnimationClip>> clipMap,
            Dictionary<string, Dictionary<string, Sprite>> firstSprite,
            StringBuilder log)
        {
            string baseName = Path.GetFileNameWithoutExtension(ResolveAssetPath(entry.sourceFolderAssetPath, clip.file));
            string animDir = entry.outputFolder + "/Animations";
            EnsureFolder(animDir);

            bool loop = LoopingClips.Contains(clip.name) && !clip.holdLastFrame;

            var byDir = new Dictionary<string, AnimationClip>();
            var byDirSprite = new Dictionary<string, Sprite>();

            for (int r = 0; r < clip.directions.Count; r++)
            {
                string dir = clip.directions[r];
                var frames = new List<Sprite>();
                foreach (int col in clip.cycle)
                {
                    int index = r * clip.columns + col;
                    string spName = $"{baseName}_{index}";
                    if (spritesByName.TryGetValue(spName, out var sp))
                        frames.Add(sp);
                }
                if (frames.Count == 0)
                    continue;

                string clipAssetName = $"{entry.characterName}_{clip.name}_{dir}";
                string savePath = $"{animDir}/{clipAssetName}.anim";
                var animClip = BuildClip(frames, options.frameRate, loop, savePath);
                byDir[dir] = animClip;
                byDirSprite[dir] = frames[0];
            }

            if (byDir.Count > 0)
            {
                clipMap[clip.name] = byDir;
                firstSprite[clip.name] = byDirSprite;
                log.AppendLine($"  - 클립: {clip.name} ({string.Join(", ", byDir.Keys)}){(loop ? " [loop]" : "")}");
            }
        }

        static AnimationClip BuildClip(List<Sprite> frames, int fps, bool loop, string savePath)
        {
            var clip = new AnimationClip { frameRate = fps };
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");

            var keys = new ObjectReferenceKeyframe[frames.Count + 1];
            for (int i = 0; i < frames.Count; i++)
                keys[i] = new ObjectReferenceKeyframe { time = i / (float)fps, value = frames[i] };
            // 마지막 프레임에 온전한 재생 시간(1/fps)을 주기 위한 트레일링 키
            keys[frames.Count] = new ObjectReferenceKeyframe { time = frames.Count / (float)fps, value = frames[frames.Count - 1] };

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(savePath);
            if (existing != null)
            {
                EditorUtility.CopySerialized(clip, existing);
                return existing;
            }
            AssetDatabase.CreateAsset(clip, savePath);
            return clip;
        }

        // ---------------------------------------------------------------
        // 3) AnimatorController (4방향 2D BlendTree + 파라미터)
        // ---------------------------------------------------------------
        static AnimatorController BuildAnimator(
            Entry entry,
            Dictionary<string, Dictionary<string, AnimationClip>> clipMap,
            StringBuilder log)
        {
            string ctrlPath = $"{entry.outputFolder}/{entry.characterName}.controller";
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);

            controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

            var sm = controller.layers[0].stateMachine;
            var states = new Dictionary<string, AnimatorState>();
            var pos = new Vector3(250, 0, 0);

            foreach (var kvp in clipMap)
            {
                var state = sm.AddState(kvp.Key, pos);
                state.motion = MakeMotion(kvp.Key, kvp.Value, controller);
                states[kvp.Key] = state;
                pos.y += 70f;
            }

            // 기본 상태: idle > walk > 첫 상태
            AnimatorState locomotion = states.GetValueOrDefault("idle")
                                       ?? states.GetValueOrDefault("walk")
                                       ?? states.Values.First();
            sm.defaultState = locomotion;

            // idle <-> walk (Speed)
            if (states.TryGetValue("idle", out var idle) && states.TryGetValue("walk", out var walk))
            {
                var toWalk = idle.AddTransition(walk);
                toWalk.hasExitTime = false; toWalk.duration = 0.05f;
                toWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

                var toIdle = walk.AddTransition(idle);
                toIdle.hasExitTime = false; toIdle.duration = 0.05f;
                toIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            }

            AnimatorState returnState = locomotion;

            // attack : AnyState 진입, 종료 후 복귀
            AddTriggeredOneShot(sm, states, "attack", "Attack", returnState);
            AddTriggeredOneShot(sm, states, "hit", "Hit", returnState);

            // death : AnyState 진입, 복귀 없음
            if (states.TryGetValue("death", out var death))
            {
                var t = sm.AddAnyStateTransition(death);
                t.hasExitTime = false; t.duration = 0.05f; t.canTransitionToSelf = false;
                t.AddCondition(AnimatorConditionMode.If, 0f, "Death");
            }

            EditorUtility.SetDirty(controller);
            log.AppendLine($"  - Animator: {states.Count}개 상태 ({string.Join(", ", states.Keys)})");
            return controller;
        }

        static void AddTriggeredOneShot(
            AnimatorStateMachine sm, Dictionary<string, AnimatorState> states,
            string stateName, string trigger, AnimatorState returnState)
        {
            if (!states.TryGetValue(stateName, out var state))
                return;

            var enter = sm.AddAnyStateTransition(state);
            enter.hasExitTime = false; enter.duration = 0.05f; enter.canTransitionToSelf = false;
            enter.AddCondition(AnimatorConditionMode.If, 0f, trigger);

            if (returnState != null && returnState != state)
            {
                var exit = state.AddTransition(returnState);
                exit.hasExitTime = true; exit.exitTime = 0.9f;
                exit.hasFixedDuration = true; exit.duration = 0.05f;
            }
        }

        static Motion MakeMotion(string clipName, Dictionary<string, AnimationClip> byDir, AnimatorController controller)
        {
            // 방향이 하나뿐이면 단일 클립을 그대로 사용
            if (byDir.Count == 1)
                return byDir.Values.First();

            var bt = new BlendTree
            {
                name = clipName,
                blendType = BlendTreeType.SimpleDirectional2D,
                blendParameter = "MoveX",
                blendParameterY = "MoveY",
                useAutomaticThresholds = false,
                hideFlags = HideFlags.HideInHierarchy,
            };
            foreach (var kvp in byDir)
            {
                Vector2 threshold = CardinalMap.GetValueOrDefault(kvp.Key, Vector2.down);
                bt.AddChild(kvp.Value, threshold);
            }
            AssetDatabase.AddObjectToAsset(bt, controller);
            return bt;
        }

        // ---------------------------------------------------------------
        // 4) Prefab
        // ---------------------------------------------------------------
        static void BuildPrefab(
            Entry entry, Options options, AnimatorController controller,
            Dictionary<string, Dictionary<string, Sprite>> firstSprite, StringBuilder log)
        {
            var go = new GameObject(entry.characterName);
            try
            {
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = PickDefaultSprite(firstSprite);

                var animator = go.AddComponent<Animator>();
                if (controller == null)
                {
                    string ctrlPath = $"{entry.outputFolder}/{entry.characterName}.controller";
                    controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
                }
                animator.runtimeAnimatorController = controller;

                if (options.attachControllerScript)
                    go.AddComponent<CharacterAnimator>();

                string prefabPath = $"{entry.outputFolder}/{entry.characterName}.prefab";
                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                log.AppendLine($"  - Prefab: {prefabPath}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        static Sprite PickDefaultSprite(Dictionary<string, Dictionary<string, Sprite>> firstSprite)
        {
            foreach (var pref in new[] { "idle", "walk" })
                if (firstSprite.TryGetValue(pref, out var d))
                    return d.GetValueOrDefault("down") ?? d.Values.FirstOrDefault();
            foreach (var d in firstSprite.Values)
            {
                var s = d.GetValueOrDefault("down") ?? d.Values.FirstOrDefault();
                if (s != null) return s;
            }
            return null;
        }

        // ---------------------------------------------------------------
        // 수동 폴백: 폴더 안의 모든 PNG 를 각각 하나의 클립으로 그리드 슬라이스
        // ---------------------------------------------------------------
        static SpriteSheetManifest BuildFallbackManifest(Entry entry, Options options)
        {
            var manifest = new SpriteSheetManifest { frameSize = options.fallbackFrameSize };
            string absFolder = ToAbsolute(entry.sourceFolderAssetPath);
            int cell = options.fallbackFrameSize;

            foreach (var png in Directory.GetFiles(absFolder, "*.png", SearchOption.AllDirectories))
            {
                string assetPath = ToAssetPath(png);
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                if (tex == null) continue;

                int columns = Mathf.Max(1, tex.width / cell);
                int rows = Mathf.Max(1, tex.height / cell);
                string rel = assetPath.Substring(entry.sourceFolderAssetPath.Length + 1);

                var clip = new SpriteSheetManifest.ClipDef
                {
                    name = Path.GetFileNameWithoutExtension(png),
                    file = rel,
                    columns = columns,
                    rows = rows,
                };
                for (int c = 0; c < columns; c++) clip.cycle.Add(c);
                for (int r = 0; r < rows; r++)
                    clip.directions.Add(r < manifest.directionOrder.Count ? manifest.directionOrder[r] : $"dir{r}");
                manifest.clips.Add(clip);
            }
            return manifest;
        }

        // ---------------------------------------------------------------
        // 경로/폴더 유틸
        // ---------------------------------------------------------------
        static void GetPivot(PivotMode mode, out SpriteAlignment alignment, out Vector2 pivot)
        {
            if (mode == PivotMode.BottomCenter)
            {
                alignment = SpriteAlignment.BottomCenter;
                pivot = new Vector2(0.5f, 0f);
            }
            else
            {
                alignment = SpriteAlignment.Center;
                pivot = new Vector2(0.5f, 0.5f);
            }
        }

        static string ResolveAssetPath(string folderAssetPath, string relativeFile)
        {
            if (string.IsNullOrEmpty(relativeFile)) return null;
            return $"{folderAssetPath}/{relativeFile.Replace('\\', '/')}";
        }

        public static void EnsureFolder(string assetFolder)
        {
            if (AssetDatabase.IsValidFolder(assetFolder))
                return;
            string parent = Path.GetDirectoryName(assetFolder).Replace('\\', '/');
            string leaf = Path.GetFileName(assetFolder);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        static string ToAbsolute(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
            return Path.Combine(projectRoot, assetPath).Replace('\\', '/');
        }

        static string ToAssetPath(string absolutePath)
        {
            absolutePath = absolutePath.Replace('\\', '/');
            int idx = absolutePath.IndexOf("/Assets/", StringComparison.Ordinal);
            if (idx >= 0) return absolutePath.Substring(idx + 1);
            if (absolutePath.StartsWith(Application.dataPath.Replace('\\', '/')))
                return "Assets" + absolutePath.Substring(Application.dataPath.Length);
            return absolutePath;
        }
    }
}
