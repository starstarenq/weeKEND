using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SysKill.DialogueSample.Editor
{
    [InitializeOnLoad]
    public static class DialogueIdMigration
    {
        static DialogueIdMigration() { EditorApplication.delayCall += Migrate; }

        [MenuItem("GameLecture/Dialogue/Migrate Legacy IDs")]
        public static void Migrate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            foreach (var guid in AssetDatabase.FindAssets("t:DialogueConfig"))
            {
                var config = AssetDatabase.LoadAssetAtPath<DialogueConfig>(AssetDatabase.GUIDToAssetPath(guid));
                if (config == null || config.idSchemaVersion >= 2) continue;
                try { MigrateAsset(config); }
                catch (Exception exception) { Debug.LogError("Dialogue ID migration failed; asset unchanged: " + exception.Message, config); }
            }
        }

        private static void MigrateAsset(DialogueConfig config)
        {
            if (config.lines == null || config.lines.Length == 0) throw new ArgumentException("Empty dialogue.");
            var nodes = new DialogueNode[config.lines.Length];
            var used = new HashSet<int>();
            var oldIds = new Dictionary<string, int>();
            for (int i = 0; i < nodes.Length; i++)
            {
                var line = config.lines[i];
                if (line == null) throw new ArgumentException("Null line.");
                int id = line.id;
                if (id == 0 && int.TryParse(line.LegacyId, out int numeric) && DialogueIds.IsValid(numeric)) id = numeric;
                if (id != 0 && (!DialogueIds.IsValid(id) || !used.Add(id)))
                    throw new ArgumentException("Invalid or duplicate ID: " + id);
                nodes[i] = new DialogueNode { id = id, speaker = line.speaker, text = line.text, end = line.end };
            }
            int available = DialogueIds.Min;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].id == 0)
                {
                    while (used.Contains(available)) available++;
                    if (available > DialogueIds.Max) throw new ArgumentException("ID range exhausted.");
                    nodes[i].id = available; used.Add(available++);
                }
                var legacy = string.IsNullOrWhiteSpace(config.lines[i].LegacyId) ? "@" + i : config.lines[i].LegacyId;
                if (oldIds.ContainsKey(legacy)) throw new ArgumentException("Duplicate legacy ID: " + legacy);
                oldIds.Add(legacy, nodes[i].id);
            }
            for (int i = 0; i < nodes.Length; i++)
            {
                var line = config.lines[i];
                nodes[i].nextId = Resolve(line.nextId, line.LegacyNextId, oldIds);
                nodes[i].choices = DialogueConfigChoice.ToData(line.choices);
                if (line.choices == null) continue;
                for (int c = 0; c < line.choices.Length; c++)
                {
                    if (line.choices[c] == null) throw new ArgumentException("Null choice.");
                    nodes[i].choices[c].targetId = Resolve(line.choices[c].targetId, line.choices[c].LegacyTargetId, oldIds);
                }
            }
            new DialogueProgress(nodes); // Validate entire conversion before touching the asset.
            Undo.RecordObject(config, "Migrate Dialogue IDs");
            for (int i = 0; i < nodes.Length; i++)
            {
                var line = config.lines[i];
                line.id = nodes[i].id; line.nextId = nodes[i].nextId; line.ClearLegacyIds();
                if (line.choices == null) continue;
                for (int c = 0; c < line.choices.Length; c++)
                {
                    line.choices[c].targetId = nodes[i].choices[c].targetId;
                    line.choices[c].ClearLegacyId();
                }
            }
            config.idSchemaVersion = 2;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssetIfDirty(config);
        }

        private static int Resolve(int current, string legacy, Dictionary<string, int> ids)
        {
            if (current != 0 || string.IsNullOrEmpty(legacy)) return current;
            if (!ids.TryGetValue(legacy, out int result)) throw new ArgumentException("Unknown legacy target: " + legacy);
            return result;
        }
    }
}
