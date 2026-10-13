using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SysKill.DialogueSample
{
    public static class DialogueIds
    {
        public const int Min = 10000000;
        public const int Max = 10009999;
        public const int None = 0;
        public static bool IsValid(int id) { return id >= Min && id <= Max; }
    }
    [Serializable]
    public sealed class DialogueChoice
    {
        public string text;
        public int targetId;
    }

    [Serializable]
    public sealed class DialogueSave
    {
        public int version = 2;
        public string source;
        public string resourceKey;
        public string fingerprint;
        public int currentId;
        public int[] history;
        public bool completed;
    }

    // Unity-independent graph used by both data formats and by save validation.
    [Serializable]
    public sealed class DialogueNode
    {
        public int id, nextId;
        public string speaker, text;
        public bool end;
        public DialogueChoice[] choices;
    }

    public sealed class DialogueProgress
    {
        private readonly DialogueNode[] nodes;
        private readonly Dictionary<int, int> lookup = new Dictionary<int, int>();
        private readonly List<int> history = new List<int>();
        public int Index { get; private set; }
        public bool Completed { get; private set; }
        public bool CanBack { get { return Completed || history.Count > 0; } }
        public string Fingerprint { get; private set; }
        public bool HasChoices { get { return nodes[Index].choices != null && nodes[Index].choices.Length > 0; } }
        public bool AtEnd { get { return nodes[Index].end || (!HasChoices && nodes[Index].nextId == DialogueIds.None && Index == nodes.Length - 1); } }

        public DialogueProgress(DialogueNode[] value)
        {
            if (value == null || value.Length == 0) throw new ArgumentException("Empty dialogue.");
            nodes = value;
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node == null || string.IsNullOrWhiteSpace(node.speaker) || string.IsNullOrWhiteSpace(node.text))
                    throw new ArgumentException("Every node needs speaker and text.");
                int id = Id(i);
                if (!DialogueIds.IsValid(id)) throw new ArgumentException("Dialogue id must be 10000000..10009999: " + id);
                if (lookup.ContainsKey(id)) throw new ArgumentException("Duplicate dialogue id: " + id);
                lookup.Add(id, i);
            }
            var signature = new StringBuilder();
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                bool choices = node.choices != null && node.choices.Length > 0;
                if ((node.end && (choices || node.nextId != DialogueIds.None)) ||
                    (choices && node.nextId != DialogueIds.None))
                    throw new ArgumentException("Use only one of end, nextId, choices: " + Id(i));
                if (node.nextId != DialogueIds.None) RequireTarget(node.nextId);
                Append(signature, Id(i)); Append(signature, node.speaker); Append(signature, node.text);
                Append(signature, node.nextId); Append(signature, node.end.ToString());
                Append(signature, (node.choices == null ? 0 : node.choices.Length).ToString());
                if (node.choices == null) continue;
                foreach (var choice in node.choices)
                {
                    if (choice == null || string.IsNullOrWhiteSpace(choice.text)) throw new ArgumentException("Empty choice: " + Id(i));
                    RequireTarget(choice.targetId);
                    Append(signature, choice.text); Append(signature, choice.targetId);
                }
            }
            using (var hash = SHA256.Create())
                Fingerprint = Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(signature.ToString())));
        }

        private static void Append(StringBuilder target, string value)
        {
            value = value ?? "";
            target.Append(value.Length).Append(':').Append(value);
        }
        private static void Append(StringBuilder target, int value)
        {
            Append(target, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        private int Id(int i) { return nodes[i].id; }
        private void RequireTarget(int id)
        {
            if (!DialogueIds.IsValid(id) || !lookup.ContainsKey(id)) throw new ArgumentException("Invalid or unknown target: " + id);
        }
        private void Move(int target) { history.Add(Index); Index = target; }
        public void Next()
        {
            if (Completed || HasChoices) return;
            if (AtEnd) { Completed = true; return; }
            Move(nodes[Index].nextId == DialogueIds.None ? Index + 1 : lookup[nodes[Index].nextId]);
        }
        public void Choose(int choice)
        {
            if (Completed || !HasChoices || choice < 0 || choice >= nodes[Index].choices.Length) return;
            Move(lookup[nodes[Index].choices[choice].targetId]);
        }
        public void Back()
        {
            if (Completed) { Completed = false; return; }
            if (history.Count == 0) return;
            Index = history[history.Count - 1]; history.RemoveAt(history.Count - 1);
        }
        public void Restart() { Index = 0; history.Clear(); Completed = false; }
        public DialogueSave Capture(string source, string key)
        {
            return new DialogueSave { source = source, resourceKey = key, fingerprint = Fingerprint,
                currentId = Id(Index), history = history.ConvertAll(Id).ToArray(), completed = Completed };
        }
        private bool CanMove(int from, int to)
        {
            var node = nodes[from];
            if (node.end) return false;
            if (node.choices != null && node.choices.Length > 0)
            {
                foreach (var choice in node.choices) if (lookup[choice.targetId] == to) return true;
                return false;
            }
            return node.nextId == DialogueIds.None ? from + 1 == to : lookup[node.nextId] == to;
        }
        public void Restore(DialogueSave save, string source, string key)
        {
            if (save == null || save.version != 2 || save.source != source || save.resourceKey != key ||
                save.fingerprint != Fingerprint || save.history == null || save.history.Length > 100000)
                throw new ArgumentException("Save is incompatible with this dialogue.");
            var path = new List<int>();
            foreach (var id in save.history)
            {
                RequireTarget(id); path.Add(lookup[id]);
            }
            RequireTarget(save.currentId);
            int current = lookup[save.currentId];
            path.Add(current);
            if (path[0] != 0) throw new ArgumentException("Invalid save starting node.");
            for (int i = 1; i < path.Count; i++)
                if (!CanMove(path[i - 1], path[i])) throw new ArgumentException("Invalid saved branch history.");
            var node = nodes[current];
            bool terminal = node.end || ((node.choices == null || node.choices.Length == 0) &&
                node.nextId == DialogueIds.None && current == nodes.Length - 1);
            if (save.completed && !terminal) throw new ArgumentException("Invalid completed state.");
            // Commit only after every check succeeds.
            path.RemoveAt(path.Count - 1);
            history.Clear(); history.AddRange(path); Index = current; Completed = save.completed;
        }
    }
}
