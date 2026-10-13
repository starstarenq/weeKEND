using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SysKill.DialogueSample
{
    [Serializable]
    public sealed class DialogueConfigLine
    {
        [SerializeField, InspectorName("Id"), Tooltip("10000000..10009999; unique in this dialogue.")]
        private int nodeId;
        [SerializeField, InspectorName("Next Id"), Tooltip("0 = sequential; otherwise 10000000..10009999.")]
        private int nextNodeId;
        [SerializeField, HideInInspector, FormerlySerializedAs("id")] private string legacyId;
        [SerializeField, HideInInspector, FormerlySerializedAs("nextId")] private string legacyNextId;
        public int id { get => nodeId; set => nodeId = value; }
        public int nextId { get => nextNodeId; set => nextNodeId = value; }
        public string LegacyId => legacyId;
        public string LegacyNextId => legacyNextId;
        public void ClearLegacyIds() { legacyId = null; legacyNextId = null; }
        public bool end;
        public DialogueConfigChoice[] choices;
        public string speaker;
        public Sprite sprite;
        [TextArea(2, 6)] public string text;
    }

    [Serializable]
    public sealed class DialogueConfigChoice
    {
        public string text;
        [SerializeField, InspectorName("Target Id"), Tooltip("10000000..10009999; an existing dialogue Id.")]
        private int targetNodeId;
        [SerializeField, HideInInspector, FormerlySerializedAs("targetId")] private string legacyTargetId;
        public int targetId { get => targetNodeId; set => targetNodeId = value; }
        public string LegacyTargetId => legacyTargetId;
        public void ClearLegacyId() { legacyTargetId = null; }
        public static DialogueConfigChoice[] FromData(DialogueChoice[] values)
        {
            return values == null ? null : Array.ConvertAll(values, value => value == null ? null :
                new DialogueConfigChoice { text = value.text, targetId = value.targetId });
        }
        public static DialogueChoice[] ToData(DialogueConfigChoice[] values)
        {
            return values == null ? null : Array.ConvertAll(values, value => value == null ? null :
                new DialogueChoice { text = value.text, targetId = value.targetId });
        }
    }

    [CreateAssetMenu(menuName = "GameLecture/Dialogue Config", fileName = "DialogueSampleConfig")]
    public sealed class DialogueConfig : ScriptableObject
    {
        [HideInInspector] public int idSchemaVersion;
        public DialogueConfigLine[] lines;
    }
}
