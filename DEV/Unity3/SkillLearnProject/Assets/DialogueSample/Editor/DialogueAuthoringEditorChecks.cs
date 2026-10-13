using System;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace SysKill.DialogueSample.Editor
{
    public static class DialogueAuthoringEditorChecks
    {
        [MenuItem("GameLecture/Dialogue/Run Creator Conversion Checks")]
        public static void Run()
        {
            var document = new DialogueAuthoringDocument();
            document.Add(); document.Add();
            document.entries[0].node.text = "한글 대사\n\"인용\"과 이모지 😀";
            document.entries[0].node.choices = new[] { new DialogueChoice { text = "진행", targetId = 10000001 } };
            document.entries[1].node.end = true;
            var json = new TextAsset(JsonConvert.SerializeObject(DialogueAuthoringAssets.ToJsonData(document))) { name = "ConversionCheck" };
            var config = ScriptableObject.CreateInstance<DialogueConfig>();
            try
            {
                var fromJson = DialogueAuthoringAssets.Import(json);
                config.lines = DialogueAuthoringAssets.ToConfigLines(fromJson);
                var fromSo = DialogueAuthoringAssets.Import(config);
                var restored = DialogueAuthoringAssets.ToJsonData(fromSo);
                if (!restored.Validate() || restored.lines[0].text != document.entries[0].node.text ||
                    restored.lines[0].choices[0].targetId != 10000001 || !restored.lines[1].end)
                    throw new Exception("JSON → SO → JSON round trip failed.");
                // No assets are saved; verifies deep copying rather than editing the imported SO.
                fromSo.entries[0].node.choices[0].text = "변경";
                if (config.lines[0].choices[0].text != "진행") throw new Exception("Import mutated source SO.");
                Debug.Log("PASS: Dialogue Creator JSON/SO conversion, Unicode, branches, source isolation.");
            }
            finally { UnityEngine.Object.DestroyImmediate(json); UnityEngine.Object.DestroyImmediate(config); }
        }
    }
}
