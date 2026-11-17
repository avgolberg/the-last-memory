using System.Collections;
using UnityEngine;

namespace TileVania
{
    /// <summary>
    /// A story beat placed in a level (digits 1-9 in the ASCII layouts). Depending on the beat it plays when the
    /// player walks into its trigger zone, or offers an "Examine" prompt (Enter). Every beat fires once per cycle
    /// and only after its prerequisite (see <see cref="Story.TryBegin"/>); readables can be re-read.
    /// </summary>
    public class StoryPoint : MonoBehaviour
    {
        public string beatId;
        [Tooltip("Scenery that fades away once the beat has played (e.g. the faint ghost).")]
        public GameObject fadeWhenDone;
        [Tooltip("Readables: half-size of the area in which the prompt appears, measured from the pivot.")]
        public Vector2 readRange = new Vector2(1.3f, 1.6f);

        StoryBeat beat;
        PlayerController player;
        bool prompting;

        public StoryBeat Beat => beat;

        void Awake() => beat = StoryScript.Beat(beatId);

        void Start()
        {
            player = FindFirstObjectByType<PlayerController>();
            if (beat == null) { Debug.LogWarning($"StoryPoint: unknown beat '{beatId}'", this); enabled = false; return; }
            if (fadeWhenDone != null && beat.kind != StoryPointKind.Readable && Story.Has(beat.id)) fadeWhenDone.SetActive(false);
            if (beat.kind == StoryPointKind.Readable && Story.Has(beat.id)) StopGlint();
        }

        void OnDisable() => ClearPrompt();

        // Auto beats ------------------------------------------------------------------
        void OnTriggerEnter2D(Collider2D other) => OnTriggerStay2D(other);

        void OnTriggerStay2D(Collider2D other)
        {
            if (beat == null || beat.kind == StoryPointKind.Readable || beat.kind == StoryPointKind.MemoryShard) return;
            var pc = other.attachedRigidbody != null ? other.attachedRigidbody.GetComponent<PlayerController>() : null;
            if (pc == null || !CanPlayNow(pc)) return;
            if (!Story.TryBegin(beat.id, beat.requires)) return;
            var mode = beat.kind == StoryPointKind.AutoBark ? DialogueMode.Bark : DialogueMode.Blocking;
            DialogueBox.Play(beat.lines, mode, OnBeatDone);
        }

        static bool CanPlayNow(PlayerController pc)
        {
            if (StoryDirector.InCutscene || DialogueBox.IsBlocking || DialogueBox.IsCinematic || SceneLoader.IsLoading) return false;
            if (!pc.ControlEnabled) return false;
            var h = pc.GetComponent<PlayerHealth>();
            return h == null || !h.IsDead;
        }

        void OnBeatDone()
        {
            if (fadeWhenDone != null && isActiveAndEnabled) StartCoroutine(FadeAway(fadeWhenDone));
        }

        IEnumerator FadeAway(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<SpriteRenderer>();
            var start = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) start[i] = renderers[i].color;
            Vector3 p0 = go.transform.localPosition;
            for (float t = 0; t < 1.2f; t += Time.deltaTime)
            {
                float k = t / 1.2f;
                for (int i = 0; i < renderers.Length; i++) { var c = start[i]; c.a *= 1f - k; renderers[i].color = c; }
                go.transform.localPosition = p0 + Vector3.up * (k * 0.8f);
                yield return null;
            }
            Fx.Burst(go.transform.position, new Color(0.75f, 0.8f, 1f, 0.8f), 10, 1.5f, 0.8f, -1f, 1.5f);
            go.SetActive(false);
        }

        // Readables -------------------------------------------------------------------
        void Update()
        {
            if (beat == null || beat.kind != StoryPointKind.Readable || player == null) return;
            Vector2 d = player.transform.position - transform.position;
            bool near = Mathf.Abs(d.x) <= readRange.x && d.y >= -0.6f && d.y <= readRange.y;
            bool can = near && CanPlayNow(player) && !DialogueBox.IsOpen;
            if (!can) { ClearPrompt(); return; }

            if (!prompting) { prompting = true; HUD.SetPrompt(this, beat.prompt); }
            if (GameInput.InteractPressed)
            {
                ClearPrompt();
                Story.TryBegin(beat.id, beat.requires); // first read marks the beat; re-reading is allowed
                StopGlint();
                DialogueBox.Play(beat.lines, DialogueMode.Blocking);
            }
        }

        void ClearPrompt()
        {
            if (!prompting) return;
            prompting = false;
            HUD.ClearPrompt(this);
        }

        void StopGlint()
        {
            var glint = GetComponentInChildren<Glint>();
            if (glint != null) glint.enabled = false;
        }
    }
}
