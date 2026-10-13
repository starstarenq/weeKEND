using UnityEngine;

namespace SysKill.DialogueSample
{
    [CreateAssetMenu(menuName = "GameLecture/Dialogue Settings", fileName = "DialogueSettings")]
    public sealed class DialogueSettings : ScriptableObject
    {
        [Header("Typewriter")]
        public bool typewriterEnabled = true;
        [Min(1)] public float charactersPerSecond = 30;
        [Min(0)] public float punctuationPause = .12f;
        public bool useUnscaledTime = true;

        [Header("Keyboard sound")]
        public bool soundEnabled = true;
        public AudioClip typingClip;
        [Range(0, 1)] public float volume = .35f;
        [Min(.01f)] public float minimumSoundInterval = .035f;
        [Range(.5f, 2)] public float pitchMin = .95f;
        [Range(.5f, 2)] public float pitchMax = 1.05f;
    }
}
