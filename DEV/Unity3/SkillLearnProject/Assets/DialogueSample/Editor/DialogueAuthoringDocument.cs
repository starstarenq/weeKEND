using System;
using System.Collections.Generic;

namespace SysKill.DialogueSample.Editor
{
    [Serializable]
    public sealed class DialogueAuthoringEntry
    {
        public DialogueNode node = new DialogueNode();
        public string spriteKey;
        public string spriteGlobalId;
    }

    [Serializable]
    public sealed class DialogueAuthoringDocument
    {
        public string resourceName = "MyDialogue";
        public List<DialogueAuthoringEntry> entries = new List<DialogueAuthoringEntry>();

        public int AllocateId()
        {
            var used = new HashSet<int>();
            foreach (var entry in entries) used.Add(entry.node.id);
            for (int id = DialogueIds.Min; id <= DialogueIds.Max; id++)
                if (!used.Contains(id)) return id;
            throw new InvalidOperationException("사용 가능한 ID가 없습니다 (최대 10,000개).");
        }

        public void Add()
        {
            entries.Add(new DialogueAuthoringEntry { node = new DialogueNode
                { id = AllocateId(), speaker = "화자", text = "대사를 입력하세요." } });
        }

        public static DialogueChoice[] CloneChoices(DialogueChoice[] choices)
        {
            return choices == null ? null : Array.ConvertAll(choices, c => c == null ? null :
                new DialogueChoice { text = c.text, targetId = c.targetId });
        }

        public void Duplicate(int index)
        {
            var source = entries[index];
            int id = AllocateId();
            entries.Insert(index + 1, new DialogueAuthoringEntry
            {
                spriteKey = source.spriteKey, spriteGlobalId = source.spriteGlobalId,
                node = new DialogueNode { id = id, speaker = source.node.speaker, text = source.node.text,
                    nextId = source.node.nextId, end = source.node.end, choices = CloneChoices(source.node.choices) }
            });
        }

        public void RenameId(int index, int id)
        {
            if (!DialogueIds.IsValid(id)) throw new ArgumentException("ID는 10000000~10009999여야 합니다.");
            int old = entries[index].node.id;
            if (id == old) return;
            foreach (var entry in entries)
                if (entry.node.id == id) throw new ArgumentException("이미 사용하는 ID입니다: " + id);
            entries[index].node.id = id;
            foreach (var entry in entries)
            {
                if (entry.node.nextId == old) entry.node.nextId = id;
                if (entry.node.choices == null) continue;
                foreach (var choice in entry.node.choices)
                    if (choice != null && choice.targetId == old) choice.targetId = id;
            }
        }

        public void Remove(int index)
        {
            int id = entries[index].node.id;
            for (int i = 0; i < entries.Count; i++)
            {
                if (i == index) continue;
                var node = entries[i].node;
                if (node.nextId == id) throw new InvalidOperationException("다른 대사가 참조합니다: " + node.id);
                if (node.choices == null) continue;
                foreach (var choice in node.choices)
                    if (choice != null && choice.targetId == id)
                        throw new InvalidOperationException("다른 선택지가 참조합니다: " + node.id);
            }
            entries.RemoveAt(index);
        }

        public void Move(int index, int delta)
        {
            int target = index + delta;
            if (target < 0 || target >= entries.Count) return;
            var entry = entries[index]; entries.RemoveAt(index); entries.Insert(target, entry);
        }

        public void Validate()
        {
            if (entries == null) throw new ArgumentException("대사 목록이 없습니다.");
            var nodes = new DialogueNode[entries.Count];
            for (int i = 0; i < entries.Count; i++) nodes[i] = entries[i]?.node;
            new DialogueProgress(nodes);
        }
    }
}
