using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SysKill.DialogueSample.Editor
{
    public static class DialogueAuthoringAssets
    {
        public const string JsonFolder = "Assets/DialogueSample/Resources/Data/JsonData";
        public const string ConfigFolder = "Assets/DialogueSample/Resources/Config";

        public static string SpriteId(Sprite sprite) => sprite == null ? null : GlobalObjectId.GetGlobalObjectIdSlow(sprite).ToString();
        public static Sprite FindSprite(string id)
        {
            return !string.IsNullOrEmpty(id) && GlobalObjectId.TryParse(id, out var globalId)
                ? GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalId) as Sprite : null;
        }

        // Match the framework ResourcesLoader's default preload roots and name keys.
        public static bool IsPreloaded(string path)
        {
            return new[] { "Config", "Prefabs", "Data/JsonData", "Sounds" }
                .Any(root => path.Contains("/Resources/" + root + "/"));
        }

        private static Dictionary<string, List<Sprite>> SpriteCatalog()
        {
            var result = new Dictionary<string, List<Sprite>>(StringComparer.Ordinal);
            var guids = AssetDatabase.FindAssets("t:Sprite").Concat(AssetDatabase.FindAssets("t:Texture2D")).Distinct();
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!IsPreloaded(path)) continue;
                foreach (var sprite in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>())
                {
                    if (!result.TryGetValue(sprite.name, out var list)) result[sprite.name] = list = new List<Sprite>();
                    if (!list.Contains(sprite)) list.Add(sprite);
                }
            }
            return result;
        }

        public static DialogueAuthoringDocument Import(UnityEngine.Object asset)
        {
            var document = new DialogueAuthoringDocument { resourceName = asset == null ? "MyDialogue" : asset.name };
            if (asset is TextAsset json)
            {
                var data = JsonConvert.DeserializeObject<DialogueData>(json.text);
                if (data?.lines == null) throw new ArgumentException("lines 배열이 있는 대화 JSON을 선택하세요.");
                var catalog = SpriteCatalog();
                foreach (var line in data.lines)
                {
                    if (line == null) throw new ArgumentException("비어 있는 대사가 있습니다.");
                    Sprite sprite = null;
                    if (!string.IsNullOrEmpty(line.spriteKey) && catalog.TryGetValue(line.spriteKey, out var matches) && matches.Count == 1)
                        sprite = matches[0];
                    document.entries.Add(new DialogueAuthoringEntry
                    {
                        node = new DialogueNode { id = line.id, nextId = line.nextId, end = line.end,
                            speaker = line.speaker, text = line.text, choices = DialogueAuthoringDocument.CloneChoices(line.choices) },
                        spriteKey = line.spriteKey, spriteGlobalId = SpriteId(sprite)
                    });
                }
            }
            else if (asset is DialogueConfig config)
            {
                if (config.lines == null) throw new ArgumentException("대사가 없는 SO입니다.");
                foreach (var line in config.lines)
                {
                    if (line == null) throw new ArgumentException("비어 있는 대사가 있습니다.");
                    document.entries.Add(new DialogueAuthoringEntry
                    {
                        node = new DialogueNode { id = line.id, nextId = line.nextId, end = line.end,
                            speaker = line.speaker, text = line.text, choices = DialogueConfigChoice.ToData(line.choices) },
                        spriteKey = line.sprite == null ? null : line.sprite.name, spriteGlobalId = SpriteId(line.sprite)
                    });
                }
            }
            else throw new ArgumentException("대화 JSON(TextAsset) 또는 DialogueConfig를 선택하세요.");
            document.Validate();
            return document;
        }

        private static Sprite ResolvePortrait(DialogueAuthoringEntry entry, bool forJson, Dictionary<string, List<Sprite>> catalog)
        {
            var sprite = FindSprite(entry.spriteGlobalId);
            string key = entry.spriteKey;
            if (sprite == null && !string.IsNullOrEmpty(entry.spriteGlobalId))
                throw new ArgumentException($"{entry.node.id}: Sprite 참조가 삭제되었습니다. 다시 선택하거나 비우세요.");
            if (sprite == null && string.IsNullOrWhiteSpace(key)) return null;
            if (sprite != null && !forJson) return sprite;
            if (string.IsNullOrWhiteSpace(key) && sprite != null) key = sprite.name;
            if (!catalog.TryGetValue(key, out var matches) || matches.Count != 1)
                throw new ArgumentException($"{entry.node.id}: Sprite 키 '{key}'가 기본 Resources 로딩 경로에서 없거나 중복됩니다.");
            if (sprite != null && matches[0] != sprite)
                throw new ArgumentException($"{entry.node.id}: Sprite와 spriteKey가 일치하지 않습니다.");
            return matches[0];
        }

        public static DialogueData ToJsonData(DialogueAuthoringDocument document)
        {
            document.Validate();
            var catalog = SpriteCatalog();
            var data = new DialogueData { lines = new DialogueLine[document.entries.Count] };
            for (int i = 0; i < data.lines.Length; i++)
            {
                var entry = document.entries[i]; var node = entry.node;
                var sprite = ResolvePortrait(entry, true, catalog);
                data.lines[i] = new DialogueLine { id = node.id, nextId = node.nextId, end = node.end,
                    speaker = node.speaker, text = node.text, spriteKey = sprite == null ? "" : sprite.name,
                    choices = DialogueAuthoringDocument.CloneChoices(node.choices) };
            }
            return data;
        }

        public static DialogueConfigLine[] ToConfigLines(DialogueAuthoringDocument document)
        {
            document.Validate();
            var catalog = SpriteCatalog();
            return document.entries.Select(entry => new DialogueConfigLine
            {
                id = entry.node.id, nextId = entry.node.nextId, end = entry.node.end,
                speaker = entry.node.speaker, text = entry.node.text,
                sprite = ResolvePortrait(entry, false, catalog),
                choices = DialogueConfigChoice.FromData(entry.node.choices)
            }).ToArray();
        }

        public static string OutputPath(DialogueAuthoringDocument document, bool json)
        {
            string name = document.resourceName;
            if (string.IsNullOrWhiteSpace(name) || name != name.Trim() || name == "." || name == ".." ||
                name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name.Contains(".") || name.Contains("/") || name.Contains("\\"))
                throw new ArgumentException("리소스 이름에는 확장자/경로/공백 접두·접미사를 넣지 마세요.");
            string path = (json ? JsonFolder : ConfigFolder) + "/" + name + (json ? ".json" : ".asset");
            foreach (var guid in AssetDatabase.FindAssets(json ? "t:TextAsset" : "t:DialogueConfig"))
            {
                var otherPath = AssetDatabase.GUIDToAssetPath(guid);
                if (otherPath == path || !IsPreloaded(otherPath)) continue;
                var other = AssetDatabase.LoadMainAssetAtPath(otherPath);
                if (other != null && other.name == name) throw new ArgumentException("같은 이름의 리소스가 다른 경로에 있습니다: " + otherPath);
            }
            return path;
        }

        public static UnityEngine.Object Export(DialogueAuthoringDocument document, bool json)
        {
            string path = OutputPath(document, json);
            // Prepare and validate before any overwrite prompt or mutation.
            string text = json ? JsonConvert.SerializeObject(ToJsonData(document), Formatting.Indented) : null;
            var lines = json ? null : ToConfigLines(document);
            var existing = AssetDatabase.LoadMainAssetAtPath(path);
            if (existing != null && !(json ? existing is TextAsset : existing is DialogueConfig))
                throw new ArgumentException("출력 경로에 다른 타입의 자산이 있습니다: " + path);
            if (File.Exists(path) && !EditorUtility.DisplayDialog("대화 내보내기", path + " 파일을 덮어쓸까요?", "덮어쓰기", "취소")) return null;
            EnsureFolder(json ? JsonFolder : ConfigFolder);
            if (json)
            {
                File.WriteAllText(path, text, new UTF8Encoding(false));
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            }
            else
            {
                var config = existing as DialogueConfig;
                if (config == null)
                {
                    config = ScriptableObject.CreateInstance<DialogueConfig>();
                    config.lines = lines; config.idSchemaVersion = 2;
                    AssetDatabase.CreateAsset(config, path);
                }
                else
                {
                    Undo.RecordObject(config, "Export dialogue");
                    config.lines = lines; config.idSchemaVersion = 2;
                    EditorUtility.SetDirty(config);
                }
                AssetDatabase.SaveAssetIfDirty(config);
            }
            return AssetDatabase.LoadMainAssetAtPath(path);
        }

        public static void ApplyToPlayer(UnityEngine.Object asset, DialogueSamplePlayer player)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || player == null ||
                !player.gameObject.scene.IsValid() || EditorUtility.IsPersistent(player))
                throw new ArgumentException("편집 모드에서 열린 Scene의 DialogueSamplePlayer를 선택하세요.");
            if (!(asset is DialogueConfig) && !(asset is TextAsset))
                throw new ArgumentException("내보낸 JSON 또는 SO를 선택하세요.");
            Undo.RecordObject(player, "Apply dialogue data");
            if (asset is DialogueConfig)
            {
                player.source = DialogueSamplePlayer.DataSource.ScriptableObject; player.configKey = asset.name;
            }
            else { player.source = DialogueSamplePlayer.DataSource.Json; player.jsonKey = asset.name; }
            PrefabUtility.RecordPrefabInstancePropertyModifications(player);
            EditorUtility.SetDirty(player);
            EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = path.Substring(0, path.LastIndexOf('/'));
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
