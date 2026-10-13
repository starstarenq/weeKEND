using System;
using GameLecture.Framework;

namespace SysKill.DialogueSample
{
    [Serializable]
    public sealed class DialogueLine
    {
        public int id;
        public int nextId;
        public bool end;
        public DialogueChoice[] choices;
        public string speaker;
        public string spriteKey;
        [UnityEngine.TextArea(2, 6)] public string text;
    }

    [Serializable]
    public sealed class DialogueData : IValidate
    {
        public DialogueLine[] lines;

        public bool Validate()
        {
            if (lines == null || lines.Length == 0) return false;
            var nodes = new DialogueNode[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line == null) return false;
                nodes[i] = new DialogueNode { id = line.id, nextId = line.nextId, end = line.end,
                    speaker = line.speaker, text = line.text, choices = line.choices };
            }
            try { new DialogueProgress(nodes); return true; }
            catch (ArgumentException) { return false; }
        }
    }
}
