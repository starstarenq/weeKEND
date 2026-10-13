using System;
using System.Globalization;

namespace SysKill.DialogueSample
{
    // Text-element boundaries preserve Korean combining marks and surrogate pairs.
    public sealed class DialogueTypewriter
    {
        private string text = "";
        private int[] starts = new int[0];
        private int count;
        private double remaining;
        public string VisibleText { get; private set; } = "";
        public int VisibleEnd => count < starts.Length ? starts[count] : text.Length;
        public bool IsTyping { get { return count < starts.Length; } }

        public void Begin(string value, bool animate)
        {
            text = value ?? "";
            starts = StringInfo.ParseCombiningCharacters(text);
            count = 0;
            remaining = 0;
            VisibleText = "";
            if (!animate) Complete();
        }

        // Return at most one sound request per frame, even after a long frame.
        public bool Tick(double delta, double charactersPerSecond, double punctuationPause)
        {
            if (!IsTyping) return false;
            remaining -= Math.Max(0, delta);
            bool audible = false;
            while (IsTyping && remaining <= 0)
            {
                int start = starts[count++];
                int end = count < starts.Length ? starts[count] : text.Length;
                var element = text.Substring(start, end - start);
                audible |= !string.IsNullOrWhiteSpace(element);
                remaining += 1 / Math.Max(1, charactersPerSecond);
                if (".,!?;:…。！？".IndexOf(element, StringComparison.Ordinal) >= 0)
                    remaining += Math.Max(0, punctuationPause);
            }
            int visibleEnd = count < starts.Length ? starts[count] : text.Length;
            VisibleText = text.Substring(0, visibleEnd);
            return audible;
        }

        public void Complete()
        {
            count = starts.Length;
            VisibleText = text;
            remaining = 0;
        }
    }
}
