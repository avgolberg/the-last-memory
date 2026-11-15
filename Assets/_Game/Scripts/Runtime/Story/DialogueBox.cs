using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TileVania
{
    public enum DialogueMode
    {
        /// <summary>Freezes the world (time scale 0) and the player until the last line is dismissed.</summary>
        Blocking,
        /// <summary>For cutscenes: time keeps running, the caller controls the player; lines also advance on their own.</summary>
        Cinematic,
        /// <summary>A short thought that never takes control away; lines advance on their own.</summary>
        Bark,
    }

    /// <summary>
    /// Reusable dialogue box: speaker name, typewriter text, keyboard-only advancing
    /// (Z / Space / X / Enter / C; the first press completes the line, the next advances).
    /// Lives on the in-game canvas; use the static API from anywhere.
    /// </summary>
    public class DialogueBox : MonoBehaviour
    {
        static DialogueBox instance;

        public CanvasGroup group;
        public TMP_Text speakerText, bodyText, hintText;
        public GameObject speakerPlate;
        public float charactersPerSecond = 42f;

        /// <summary>Tests: complete and dismiss every line on the next frame.</summary>
        public static bool TestAutoAdvance;

        /// <summary>Total number of dialogues started (tests use it to check that a beat fired exactly once).</summary>
        public static int StartedCount { get; private set; }

        class Request
        {
            public DialogueLine[] lines;
            public DialogueMode mode;
            public Action onDone;
        }

        readonly Queue<Request> queue = new Queue<Request>();
        Coroutine running;
        Request current;

        static readonly Color SpeechColor = new Color(0.95f, 0.92f, 0.85f);
        static readonly Color ThoughtColor = new Color(0.78f, 0.78f, 0.9f);
        static readonly Color NoteColor = new Color(1f, 0.88f, 0.66f);
        static readonly Color VoiceColor = new Color(0.68f, 0.84f, 1f);
        static readonly Color MemoryColor = new Color(1f, 0.82f, 0.45f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            TestAutoAdvance = false;
            StartedCount = 0;
        }

        public static bool IsOpen => instance != null && instance.current != null;
        public static bool IsBlocking => IsOpen && instance.current.mode == DialogueMode.Blocking;
        public static bool IsCinematic => IsOpen && instance.current.mode == DialogueMode.Cinematic;
        public static bool IsBark => IsOpen && instance.current.mode == DialogueMode.Bark;

        /// <summary>The text of the line on screen (empty when closed).</summary>
        public static string CurrentText => IsOpen && instance.bodyText != null ? instance.bodyText.text : "";
        public static string CurrentSpeaker => IsOpen && instance.speakerText != null ? instance.speakerText.text : "";

        /// <summary>True while the current line is still being typed out.</summary>
        public static bool IsTyping { get; private set; }

        void Awake()
        {
            instance = this;
            if (group != null) { group.alpha = 0f; group.blocksRaycasts = false; }
        }

        void OnDestroy()
        {
            if (instance != this) return;
            // Never leave the world frozen if the scene unloads mid-dialogue.
            if (current != null && current.mode == DialogueMode.Blocking) Time.timeScale = 1f;
            instance = null;
            IsTyping = false;
        }

        /// <summary>
        /// Shows a dialogue. Blocking and cinematic dialogues replace a bark that is on screen and otherwise queue.
        /// A bark is dropped if anything else is showing. Without a dialogue box in the scene, onDone runs at once.
        /// </summary>
        public static void Play(DialogueLine[] lines, DialogueMode mode, Action onDone = null)
        {
            if (instance == null || lines == null || lines.Length == 0) { onDone?.Invoke(); return; }
            instance.Enqueue(new Request { lines = lines, mode = mode, onDone = onDone });
        }

        /// <summary>Coroutine helper: plays the lines and waits until they are done.</summary>
        public static IEnumerator Run(DialogueLine[] lines, DialogueMode mode)
        {
            bool done = false;
            Play(lines, mode, () => done = true);
            while (!done) yield return null;
        }

        void Enqueue(Request r)
        {
            if (current != null)
            {
                if (r.mode == DialogueMode.Bark) { r.onDone?.Invoke(); return; }
                if (current.mode == DialogueMode.Bark)
                {
                    StopCoroutine(running);
                    Finish(current);
                }
                else
                {
                    queue.Enqueue(r);
                    return;
                }
            }
            StartRequest(r);
        }

        void StartRequest(Request r)
        {
            current = r;
            StartedCount++;
            running = StartCoroutine(RunRequest(r));
        }

        IEnumerator RunRequest(Request r)
        {
            PlayerController player = null;
            bool restoreControl = false;
            if (r.mode == DialogueMode.Blocking)
            {
                player = FindFirstObjectByType<PlayerController>();
                if (player != null && player.ControlEnabled) { player.SetControl(false); restoreControl = true; }
                Time.timeScale = 0f;
            }

            if (group != null) group.alpha = 1f;
            foreach (var line in r.lines) yield return ShowLine(line, r.mode);

            if (r.mode == DialogueMode.Blocking) Time.timeScale = 1f;
            Finish(r);
            if (restoreControl) StartCoroutine(RestoreControl(player));
            if (current == null && queue.Count > 0) StartRequest(queue.Dequeue());
        }

        void Finish(Request r)
        {
            if (group != null) group.alpha = 0f;
            IsTyping = false;
            current = null;
            running = null;
            r.onDone?.Invoke();
        }

        /// <summary>
        /// Gives control back a moment later, so the key that closed the dialogue does not also jump or shoot.
        /// </summary>
        IEnumerator RestoreControl(PlayerController player)
        {
            yield return new WaitForSecondsRealtime(0.12f);
            if (player == null || IsBlocking || StoryDirector.InCutscene) yield break;
            var health = player.GetComponent<PlayerHealth>();
            if (health != null && health.IsDead) yield break;
            player.SetControl(true);
        }

        IEnumerator ShowLine(DialogueLine line, DialogueMode mode)
        {
            bool hasSpeaker = !string.IsNullOrEmpty(line.speaker);
            if (speakerPlate != null) speakerPlate.SetActive(hasSpeaker);
            if (speakerText != null) speakerText.text = line.speaker ?? "";
            bodyText.text = Story.Format(line.text);
            bodyText.color = StyleColor(line.style);
            bodyText.fontStyle = line.style == LineStyle.Speech ? FontStyles.Normal : FontStyles.Italic;
            bodyText.ForceMeshUpdate();
            int total = bodyText.textInfo.characterCount;
            bodyText.maxVisibleCharacters = 0;
            if (hintText != null) hintText.enabled = false;

            // Typewriter (real time, so it also runs while the world is frozen)
            IsTyping = true;
            float t = 0f;
            while (true)
            {
                if (TestAutoAdvance) break;
                t += Time.unscaledDeltaTime;
                int visible = Mathf.Min(total, Mathf.FloorToInt(t * charactersPerSecond));
                bodyText.maxVisibleCharacters = visible;
                if (visible >= total) break;
                // Ignore the press that opened the dialogue (C / Enter on a readable).
                if (mode != DialogueMode.Bark && t > 0.15f && GameInput.AdvancePressed) break;
                yield return null;
            }
            bodyText.maxVisibleCharacters = total;
            IsTyping = false;
            yield return null; // a press that completed the line must not also dismiss it

            if (TestAutoAdvance) yield break;
            if (line.cutOff) { yield return new WaitForSecondsRealtime(0.35f); yield break; }

            float hold = mode == DialogueMode.Bark ? 1.6f + total * 0.035f : 2.4f + total * 0.05f;
            if (hintText != null) hintText.enabled = mode != DialogueMode.Bark;
            float shown = 0f;
            while (true)
            {
                shown += Time.unscaledDeltaTime;
                if (mode != DialogueMode.Bark && shown > 0.12f && GameInput.AdvancePressed) break;
                // Blocking dialogues wait for the player; cinematic lines and barks also move on by themselves.
                if (mode != DialogueMode.Blocking && shown >= hold) break;
                if (hintText != null && mode != DialogueMode.Bark) hintText.alpha = 0.55f + 0.45f * Mathf.PingPong(shown * 2f, 1f);
                yield return null;
            }
        }

        static Color StyleColor(LineStyle s) => s switch
        {
            LineStyle.Thought => ThoughtColor,
            LineStyle.Note => NoteColor,
            LineStyle.Voice => VoiceColor,
            LineStyle.Memory => MemoryColor,
            _ => SpeechColor,
        };
    }
}
