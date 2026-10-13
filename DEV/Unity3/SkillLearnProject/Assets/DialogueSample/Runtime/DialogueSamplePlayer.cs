using System;
using GameLecture.Framework;
using UnityEngine;

namespace SysKill.DialogueSample
{
    // Data/progression owner. Legacy presentation remains available until scene migration.
    public sealed class DialogueSamplePlayer : MonoBehaviour
    {
        public enum DataSource { Json, ScriptableObject }
        public DataSource source;
        public string jsonKey = "DialogueSampleData";
        public string configKey = "DialogueSampleConfig";
        [Tooltip("Resources/Config asset name; shared by JSON and SO dialogue.")]
        public string settingsKey = "DialogueSettings";
        public bool useUGUI;
        public event Action StateChanged;
        public event Action RevealChanged;
        private DialogueUGUIView activeView;
        private bool resourcesReady;
        private bool closed;
        private int commandFrame = -1;
        public int Revision { get; private set; }
        public bool Ready => resourcesReady && progress != null && lines != null && error == null;
        public bool ResourcesReady => resourcesReady;
        public bool Closed => closed;
        public bool Completed => Ready && progress.Completed;
        public bool IsTyping => Ready && !closed && !Completed && typewriter.IsTyping;
        public bool HasChoices => Ready && progress.HasChoices;
        public bool CanAdvance => Ready && !closed && !Completed && (IsTyping || !HasChoices);
        public bool CanPrevious => Ready && !closed && progress.CanBack;
        public string AdvanceLabel => IsTyping ? "전체 표시" : Ready && progress.AtEnd ? "종료" : "다음";
        public string Status => error ?? status ?? (!resourcesReady ? "불러오는 중…" : Completed || closed ? "대화 종료" : "");
        public string Speaker => Ready && !closed ? lines[index].speaker : "";
        public string FullText => Ready && !closed ? lines[index].text : "";
        public Sprite Portrait => Ready && !closed ? lines[index].sprite : null;
        public int CurrentId => Ready ? lines[index].id : 0;
        public int VisibleEnd => typewriter.VisibleEnd;
        public int ChoiceCount => Ready && !closed && !Completed && !IsTyping && HasChoices ? lines[index].choices.Length : 0;
        public string ChoiceText(int choice) => lines[index].choices[choice].text;

        public bool AttachView(DialogueUGUIView view)
        {
            if (!useUGUI || (activeView != null && activeView != view)) return false;
            activeView = view;
            return true;
        }

        public void DetachView(DialogueUGUIView view)
        {
            if (activeView != view) return;
            activeView = null;
            if (typingAudio != null) typingAudio.Stop();
        }

        private bool AcceptCommand()
        {
            if (!isActiveAndEnabled || !Ready || commandFrame == Time.frameCount) return false;
            commandFrame = Time.frameCount;
            return true;
        }

        public void Advance()
        {
            if (!CanAdvance || !AcceptCommand()) return;
            if (IsTyping)
            {
                typewriter.Complete();
                if (typingAudio != null) typingAudio.Stop();
                StateChanged?.Invoke();
                RevealChanged?.Invoke();
            }
            else { progress.Next(); BeginLine(); }
        }

        public void Choose(int choiceIndex) => Choose(choiceIndex, Revision);
        public void Choose(int choiceIndex, int revision)
        {
            if (revision != Revision || choiceIndex < 0 || choiceIndex >= ChoiceCount || !AcceptCommand()) return;
            progress.Choose(choiceIndex);
            BeginLine();
        }

        public void Previous()
        {
            if (!CanPrevious || !AcceptCommand()) return;
            progress.Back(); BeginLine();
        }

        public void Restart()
        {
            if (!AcceptCommand()) return;
            closed = false;
            progress.Restart(); BeginLine();
        }

        public void CloseDialogue()
        {
            if (closed || !AcceptCommand()) return;
            closed = true; StopTyping(); Revision++;
            StateChanged?.Invoke();
        }
        private DialogueSettings settings;
        private readonly DialogueTypewriter typewriter = new DialogueTypewriter();
        private AudioSource typingAudio;
        private AudioListener ownedListener;
        private bool ownsSettings;
        private double soundCooldown;
        private DialogueConfigLine[] lines;
        private DialogueProgress progress;
        private int index => progress.Index;
        private JsonSaveStore<DialogueSave> store;
        private JsonSaveStore<DialogueSave> saveStore => store ??= new JsonSaveStore<DialogueSave>("dialogue-progress.json");
        private string loadedSource, loadedKey;
        private string status;
        private Vector2 choiceScroll;
        private string CurrentKey => source == DataSource.Json ? jsonKey : configKey;
        private string error;
        private Font font;

        private void Start()
        {
            if (!useUGUI) font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Arial" }, 22);
            // Keep the application's existing loader. Portraits live under Config,
            // one of the framework's default preload roots.
            ResourceManager.Instance.LoadAll(onComplete: () =>
            {
                if (this == null) return;
                resourcesReady = true;
                Reload();
            });
        }

        public void Reload()
        {
            if (!resourcesReady) return;
            closed = false;
            Revision++;
            progress = null;
            status = null;
            lines = null;
            error = null;
            StopTyping();
            try
            {
                if (settings == null)
                {
                    settings = GameLecture.Framework.DataManager.Instance.LoadConfig<DialogueSettings>(settingsKey);
                    if (settings == null)
                    {
                        settings = ScriptableObject.CreateInstance<DialogueSettings>();
                        ownsSettings = true;
                        Debug.LogWarning("Create Dialogue Settings using GameLecture/Dialogue/Create Sample Scene.", this);
                    }
                }
                if (source == DataSource.Json)
                {
                    var data = GameLecture.Framework.DataManager.Instance.LoadJson<DialogueData>(jsonKey);
                    var result = new DialogueConfigLine[data.lines.Length];
                    for (var i = 0; i < result.Length; i++)
                    {
                        var entry = data.lines[i];
                        var sprite = string.IsNullOrWhiteSpace(entry.spriteKey) ? null :
                            ResourceManager.Instance.Get<Sprite>(entry.spriteKey);
                        if (!string.IsNullOrWhiteSpace(entry.spriteKey) && sprite == null)
                            throw new InvalidOperationException($"Sprite not found: {entry.spriteKey}");
                        result[i] = new DialogueConfigLine
                            { speaker = entry.speaker, sprite = sprite, text = entry.text,
                                id = entry.id, nextId = entry.nextId, end = entry.end, choices = DialogueConfigChoice.FromData(entry.choices) };
                    }
                    lines = result;
                }
                else
                {
                    var config = GameLecture.Framework.DataManager.Instance.LoadConfig<DialogueConfig>(configKey);
                    if (config == null || config.lines == null || config.lines.Length == 0)
                        throw new InvalidOperationException("Dialogue config is missing or empty.");
                    foreach (var entry in config.lines)
                        if (entry == null || string.IsNullOrWhiteSpace(entry.speaker) ||
                            string.IsNullOrWhiteSpace(entry.text))
                            throw new InvalidOperationException("Every line needs speaker and text.");
                    lines = config.lines;
                }
                var nodes = new DialogueNode[lines.Length];
                for (int i = 0; i < lines.Length; i++)
                {
                    var entry = lines[i];
                    nodes[i] = new DialogueNode { id = entry.id, nextId = entry.nextId, end = entry.end,
                        choices = DialogueConfigChoice.ToData(entry.choices), speaker = entry.speaker, text = entry.text };
                }
                progress = new DialogueProgress(nodes);
                loadedSource = source.ToString();
                loadedKey = CurrentKey;
                BeginLine();
            }
            catch (Exception exception)
            {
                lines = null;
                error = exception.Message;
                Debug.LogError($"Dialogue sample: {error}", this);
                StateChanged?.Invoke();
            }
        }

        public void SwitchSource(DataSource newSource, bool branch)
        {
            if (!resourcesReady || !isActiveAndEnabled || commandFrame == Time.frameCount) return;
            commandFrame = Time.frameCount;
            source = newSource;
            if (branch)
            {
                if (source == DataSource.Json) jsonKey = "DialogueBranchData";
                else configKey = "DialogueBranchConfig";
            }
            Reload();
        }

        private void BeginLine()
        {
            StopTyping();
            choiceScroll = Vector2.zero;
            typewriter.Begin(lines[index].text, settings.typewriterEnabled);
            if (progress.Completed) typewriter.Complete();
            Revision++;
            StateChanged?.Invoke();
            RevealChanged?.Invoke();
        }

        public void SaveDialogue()
        {
            if (!Ready || !isActiveAndEnabled) return;
            try
            {
                saveStore.Save(progress.Capture(loadedSource, loadedKey));
                status = "저장 완료. 불러오면 현재 대사를 처음부터 출력합니다.";
            }
            catch (Exception exception) { status = "저장 실패: " + exception.Message; }
            StateChanged?.Invoke();
        }

        public void LoadDialogue()
        {
            if (!AcceptCommand()) return;
            try
            {
                var save = saveStore.Load();
                if (save == null) { status = "저장된 대화가 없습니다."; StateChanged?.Invoke(); return; }
                progress.Restore(save, loadedSource, loadedKey);
                closed = false;
                BeginLine();
                status = "불러오기 완료.";
            }
            catch (Exception exception) { status = "불러오기 실패 (현재 진행 유지): " + exception.Message; }
            StateChanged?.Invoke();
        }

        private void StopTyping()
        {
            typewriter.Begin("", false);
            soundCooldown = 0;
            if (typingAudio != null) typingAudio.Stop();
        }

        private void Update()
        {
            if (!Ready || settings == null || progress.Completed || closed || (useUGUI && activeView == null)) return;
            bool wasTyping = typewriter.IsTyping;
            int previousEnd = typewriter.VisibleEnd;
            if (!settings.typewriterEnabled)
            {
                typewriter.Complete();
                if (typingAudio != null) typingAudio.Stop();
                if (wasTyping) { StateChanged?.Invoke(); RevealChanged?.Invoke(); }
                return;
            }
            var delta = settings.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            soundCooldown -= delta;
            bool tick = typewriter.Tick(delta, settings.charactersPerSecond, settings.punctuationPause);
            if (previousEnd != typewriter.VisibleEnd) RevealChanged?.Invoke();
            if (wasTyping && !typewriter.IsTyping) StateChanged?.Invoke();
            if (!settings.soundEnabled)
            {
                if (typingAudio != null) typingAudio.Stop();
                return;
            }
            if (!tick || soundCooldown > 0 || settings.typingClip == null) return;
            if (typingAudio == null)
            {
                typingAudio = gameObject.AddComponent<AudioSource>();
                typingAudio.playOnAwake = false;
                typingAudio.spatialBlend = 0;
                // The original sample camera has no AudioListener.
                bool hasListener = false;
                foreach (var listener in FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
                    hasListener |= listener.isActiveAndEnabled;
                if (!hasListener) ownedListener = gameObject.AddComponent<AudioListener>();
            }
            typingAudio.pitch = UnityEngine.Random.Range(Mathf.Min(settings.pitchMin, settings.pitchMax),
                Mathf.Max(settings.pitchMin, settings.pitchMax));
            typingAudio.PlayOneShot(settings.typingClip, Mathf.Clamp01(settings.volume));
            soundCooldown = Mathf.Max(.01f, settings.minimumSoundInterval);
        }

        private void OnDisable()
        {
            if (typingAudio != null) typingAudio.Stop();
            if (ownedListener != null) ownedListener.enabled = false;
        }

        private void OnEnable()
        {
            if (ownedListener != null) ownedListener.enabled = true;
        }

        private void OnGUI()
        {
            if (useUGUI) return;
            var oldMatrix = GUI.matrix;
            var scale = Mathf.Min(Screen.width / 960f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 960 * scale) / 2,
                (Screen.height - 720 * scale) / 2, 0), Quaternion.identity, Vector3.one * scale);
            var label = new GUIStyle(GUI.skin.label) { font = font, fontSize = 22, wordWrap = true };
            GUI.Box(new Rect(16, 16, 928, 688), GUIContent.none);
            GUI.Label(new Rect(40, 30, 880, 40), "Dialogue • GameLecture Framework", label);
            if (GUI.Button(new Rect(40, 85, 180, 40), "JSON")) SwitchSource(DataSource.Json, false);
            if (GUI.Button(new Rect(235, 85, 180, 40), "ScriptableObject"))
                SwitchSource(DataSource.ScriptableObject, false);
            GUI.Label(new Rect(650, 30, 270, 40), $"Source: {source}", label);
            if (GUI.Button(new Rect(440, 85, 180, 40), "Branch JSON"))
                SwitchSource(DataSource.Json, true);
            if (GUI.Button(new Rect(635, 85, 180, 40), "Branch SO"))
                SwitchSource(DataSource.ScriptableObject, true);
            if (error != null) GUI.Label(new Rect(40, 160, 880, 260), error, label);
            else if (lines != null)
            {
                var entry = lines[index];
                if (entry.sprite != null)
                {
                    var sprite = entry.sprite;
                    var rect = sprite.textureRect;
                    var uv = new Rect(rect.x / sprite.texture.width, rect.y / sprite.texture.height,
                        rect.width / sprite.texture.width, rect.height / sprite.texture.height);
                    var factor = Mathf.Min(200 / rect.width, 240 / rect.height);
                    GUI.DrawTextureWithTexCoords(new Rect(40, 160, rect.width * factor,
                        rect.height * factor), sprite.texture, uv);
                }
                GUI.Label(new Rect(270, 155, 630, 45), entry.speaker, label);
                GUI.Label(new Rect(270, 215, 630, 165), typewriter.VisibleText, label);
                if (!typewriter.IsTyping && progress.HasChoices && !progress.Completed)
                {
                    var button = new GUIStyle(GUI.skin.button) { font = font, fontSize = 18, wordWrap = true };
                    choiceScroll = GUI.BeginScrollView(new Rect(270, 385, 630, 160), choiceScroll,
                        new Rect(0, 0, 605, entry.choices.Length * 64));
                    int selected = -1;
                    for (int i = 0; i < entry.choices.Length; i++)
                        if (GUI.Button(new Rect(0, i * 64, 595, 58), entry.choices[i].text, button)) selected = i;
                    GUI.EndScrollView();
                    if (selected >= 0) Choose(selected);
                }
                GUI.Label(new Rect(40, 555, 200, 40), progress.Completed ? "대화 종료" : $"{index + 1} / {lines.Length}", label);
                GUI.enabled = progress.CanBack;
                if (GUI.Button(new Rect(270, 555, 180, 40), "Previous")) Previous();
                GUI.enabled = true;
                if (GUI.Button(new Rect(465, 555, 180, 40), "Restart")) Restart();
                GUI.enabled = !progress.Completed && (typewriter.IsTyping || !progress.HasChoices);
                if (GUI.Button(new Rect(660, 555, 180, 40), typewriter.IsTyping ? "Complete" : progress.AtEnd ? "Finish" : "Next"))
                    Advance();
                GUI.enabled = true;
                if (GUI.Button(new Rect(270, 605, 180, 35), "Save")) SaveDialogue();
                if (GUI.Button(new Rect(465, 605, 180, 35), "Load")) LoadDialogue();
                if (!string.IsNullOrEmpty(status))
                    GUI.Label(new Rect(40, 650, 870, 45), status,
                        new GUIStyle(label) { fontSize = 15 });
            }
            GUI.matrix = oldMatrix;
        }

        private void OnDestroy()
        {
            if (font != null) Destroy(font);
            if (typingAudio != null) Destroy(typingAudio);
            if (ownedListener != null) Destroy(ownedListener);
            if (ownsSettings && settings != null) Destroy(settings);
        }
    }
}
