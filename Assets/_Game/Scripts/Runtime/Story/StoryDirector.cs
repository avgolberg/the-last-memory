using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TileVania
{
    /// <summary>
    /// Runs the two story cutscenes with the existing gameplay objects (no timeline):
    ///  - the opening in the forest (first waking, or the loop after the ending), and
    ///  - the ending at the final chest, which returns to the forest.
    /// One per level scene (placed by the level builder). Docs/STORY_SPEC.md describes the sequence.
    /// </summary>
    public class StoryDirector : MonoBehaviour
    {
        public static StoryDirector Instance { get; private set; }

        /// <summary>True while a cutscene owns the player (pause and story points stay quiet).</summary>
        public static bool InCutscene { get; private set; }

        /// <summary>Tests speed up cutscene waits and fades with this.</summary>
        public static float CutsceneSpeed = 1f;

        [Header("Scene")]
        public bool isOpeningScene;
        public PlayerController player;
        [Tooltip("Forest: the notched tree's tally marks.")]
        public TallyMarks treeMarks;
        [Tooltip("Forest: where the hero stops in front of the tree during the loop.")]
        public float treeStandX = 5.6f;

        [Header("Ending art")]
        public Sprite silhouetteSprite;
        public Sprite lanternSprite;
        public Sprite[] gemSprites;
        public Material unlitMaterial;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            InCutscene = false;
            CutsceneSpeed = 1f;
        }

        void Awake() => Instance = this;

        void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            InCutscene = false;
        }

        static IEnumerator Wait(float seconds) => new WaitForSecondsRealtime(seconds / Mathf.Max(0.01f, CutsceneSpeed));

        /// <summary>Called by the LevelManager on level start. Returns true if an opening cutscene takes over.</summary>
        public bool TryHandleLevelStart()
        {
            if (!isOpeningScene || Story.PendingOpening == Story.Opening.None || player == null) return false;
            var kind = Story.PendingOpening;
            Story.PendingOpening = Story.Opening.None;
            InCutscene = true;
            player.SetControl(false);
            StartCoroutine(Opening(kind));
            return true;
        }

        // ------------------------------------------------------------------ opening
        IEnumerator Opening(Story.Opening kind)
        {
            InCutscene = true;
            player.SetControl(false);
            CinematicOverlay.SetFade(Color.black, 1f);
            CinematicOverlay.SetLetterbox(1f);
            if (kind == Story.Opening.Loop)
            {
                AudioManager.PlayMusic(null, 0.3f);
                Story.Set("forest_tree"); // the loop shows the tree itself; the walk-up beat must not fire again
            }
            SetLying(true);
            if (CameraController.Current != null) CameraController.Current.SnapToTarget();

            yield return Wait(1.0f);
            yield return DialogueBox.Run(StoryScript.OpeningVoice, DialogueMode.Cinematic);
            yield return CinematicOverlay.FadeTo(Color.black, 0f, 2.2f);
            yield return Wait(0.6f);
            yield return StandUp();
            yield return Wait(0.3f);
            yield return DialogueBox.Run(StoryScript.OpeningWake, DialogueMode.Cinematic);

            if (kind == Story.Opening.FirstWaking)
            {
                yield return CinematicOverlay.LetterboxTo(0f, 0.6f);
                if (LevelManager.Instance != null) LevelManager.Instance.ShowLevelTitle();
                EndCutscene(true);
                yield break;
            }

            // The loop: the same words, the same tree... and one more notch.
            yield return WalkTo(treeStandX);
            player.SetFacing(1);
            yield return Wait(0.5f);
            if (treeMarks != null)
            {
                treeMarks.PulseNewest(3.5f / CutsceneSpeed);
                AudioManager.Play(Sfx.Checkpoint, 0.35f);
            }
            yield return Wait(1.6f);
            yield return DialogueBox.Run(StoryScript.LoopNote, DialogueMode.Cinematic);
            yield return Wait(1.0f);
            yield return CinematicOverlay.FadeTo(Color.black, 1f, 1.6f);
            yield return CinematicOverlay.Caption(StoryScript.FinalLine, 1.4f, 3.4f, 1.4f);
            yield return Wait(0.4f);
            yield return CinematicOverlay.LetterboxTo(0f, 0.01f);
            StartCoroutine(CinematicOverlay.FadeTo(Color.black, 0f, 1.5f));
            GameSession.Instance.TimerRunning = false;
            GameMenus.ShowEnding(Story.LastCycle);
            EndCutscene(false); // the ending panel decides: Begin Again or Main Menu
        }

        void EndCutscene(bool giveControl)
        {
            InCutscene = false;
            if (giveControl && player != null) player.SetControl(true);
        }

        void SetLying(bool lying)
        {
            if (player == null || player.visual == null) return;
            player.visual.localRotation = lying ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.identity;
            player.visual.localPosition = lying ? new Vector3(0.55f, 0.32f, 0f) : Vector3.zero;
        }

        IEnumerator StandUp()
        {
            if (player == null || player.visual == null) yield break;
            var v = player.visual;
            Quaternion r0 = v.localRotation;
            Vector3 p0 = v.localPosition;
            AudioManager.Play(Sfx.Land, 0.4f);
            const float time = 0.55f;
            for (float t = 0; t < time; t += Time.unscaledDeltaTime * CutsceneSpeed)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / time);
                v.localRotation = Quaternion.Slerp(r0, Quaternion.identity, k);
                v.localPosition = Vector3.Lerp(p0, Vector3.zero, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.25f;
                yield return null;
            }
            SetLying(false);
            Fx.Dust(player.transform.position, 6);
        }

        IEnumerator WalkTo(float x, float speedFactor = 0.45f)
        {
            float dir = Mathf.Sign(x - player.transform.position.x);
            float timeout = 4f;
            while (Mathf.Abs(player.transform.position.x - x) > 0.15f && timeout > 0f)
            {
                if (Mathf.Sign(x - player.transform.position.x) != dir) break;
                player.ScriptedMoveX = dir * speedFactor;
                timeout -= Time.deltaTime;
                yield return null;
            }
            player.ScriptedMoveX = 0f;
        }

        // ------------------------------------------------------------------ ending
        /// <summary>The final chest was opened: the last memory, almost recovered, and the loop.</summary>
        public void PlayEnding(TreasureChest chest)
        {
            if (!Story.TryBegin("ending")) return;
            InCutscene = true;
            StartCoroutine(Ending(chest));
        }

        IEnumerator Ending(TreasureChest chest)
        {
            InCutscene = true;
            var summary = Story.CaptureSummary();
            GameSession.Instance.TimerRunning = false;
            if (player != null)
            {
                player.SetControl(false);
                player.Body.linearVelocity = new Vector2(0f, player.Body.linearVelocity.y);
            }
            AudioManager.PlayMusic(null, 2.5f);
            StartCoroutine(CinematicOverlay.LetterboxTo(1f, 0.8f));
            HUD.HideBossBar();

            Vector3 chestPos = chest.transform.position;
            float side = player != null && player.transform.position.x > chestPos.x ? -1f : 1f; // beyond the chest, away from the hero
            if (player != null) player.SetFacing((int)side);

            // The light swells
            var glow = chest.glow;
            float glow0 = glow != null ? glow.intensity : 0f, radius0 = glow != null ? glow.pointLightOuterRadius : 0f;
            for (float t = 0; t < 2f; t += Time.unscaledDeltaTime * CutsceneSpeed)
            {
                float k = t / 2f;
                if (glow != null) { glow.intensity = Mathf.Lerp(glow0, 3.2f, k); glow.pointLightOuterRadius = Mathf.Lerp(radius0, 9f, k); }
                if (Random.value < 0.25f) Fx.Burst(chestPos + new Vector3(Random.Range(-0.6f, 0.6f), 0.7f), new Color(1f, 0.85f, 0.45f), 2, 1.5f, 1.1f, -2f, 1.5f);
                yield return null;
            }

            // A small figure with a lantern, standing in the light
            var silhouette = MakeSilhouette(chestPos + new Vector3(side * 1.9f, 0f), side);
            var silSr = silhouette.GetComponent<SpriteRenderer>();
            var lanternLight = silhouette.GetComponentInChildren<Light2D>();
            for (float t = 0; t < 1.6f; t += Time.unscaledDeltaTime * CutsceneSpeed)
            {
                SetAlpha(silhouette, Mathf.Clamp01(t / 1.6f) * 0.95f);
                yield return null;
            }
            SetAlpha(silhouette, 0.95f);

            yield return DialogueBox.Run(StoryScript.EndingReunion, DialogueMode.Cinematic);
            yield return WalkTo(chestPos.x - side * 0.6f, 0.25f);
            yield return DialogueBox.Run(StoryScript.EndingName(Story.MemoriesFound >= Story.TotalMemories), DialogueMode.Cinematic);

            // It slips away
            Fx.Burst(silhouette.transform.position + Vector3.up * 0.6f, new Color(1f, 0.9f, 0.7f), 40, 3f, 1.2f, -1.5f, 2f);
            var flicker = silhouette.GetComponentInChildren<LightFlicker>();
            if (flicker != null) flicker.enabled = false;
            for (float t = 0; t < 0.5f; t += Time.unscaledDeltaTime * CutsceneSpeed)
            {
                SetAlpha(silhouette, 0.95f * (1f - t / 0.5f));
                if (lanternLight != null) lanternLight.intensity = Mathf.Lerp(1.2f, 0f, t / 0.5f);
                yield return null;
            }
            Destroy(silhouette);
            _ = silSr;

            // Every gem, every fragment, scatters
            var scatter = StartCoroutine(ScatterGems(chestPos + Vector3.up * 0.6f));
            CameraController.Shake(0.25f, 1.4f);
            AudioManager.Play(Sfx.Portal);
            AudioManager.Play(Sfx.BossDie, 0.4f);
            yield return Wait(0.5f);
            yield return CinematicOverlay.FadeTo(Color.white, 1f, 1.5f);
            yield return DialogueBox.Run(StoryScript.EndingLoss, DialogueMode.Cinematic);
            yield return CinematicOverlay.FadeTo(Color.black, 1f, 1.2f);
            yield return Wait(0.9f);
            AudioManager.Play(Sfx.RockImpact, 0.3f); // a knife in bark: one more notch
            yield return Wait(0.8f);
            yield return DialogueBox.Run(StoryScript.EndingWhisper, DialogueMode.Cinematic);
            yield return Wait(1.0f);

            // The scatter must be finished before the new cycle resets the counter (fast cutscenes in tests).
            StopCoroutine(scatter);
            Story.CompleteCycle(summary);
            GameSession.StartNewRun();
            Story.BeginNewCycle(Story.Opening.Loop);
            SceneLoader.Load(SceneLoader.Level1, 0.2f);
        }

        GameObject MakeSilhouette(Vector3 feet, float side)
        {
            var go = new GameObject("Silhouette");
            go.transform.position = feet;
            go.transform.localScale = new Vector3(0.72f, 0.72f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = silhouetteSprite;
            if (unlitMaterial != null) sr.sharedMaterial = unlitMaterial;
            sr.sortingLayerName = "Interactables";
            sr.sortingOrder = 5;
            sr.color = new Color(0.05f, 0.03f, 0.06f, 0f);
            sr.flipX = side > 0f; // face the hero

            var lantern = new GameObject("Lantern");
            lantern.transform.SetParent(go.transform, false);
            lantern.transform.localPosition = new Vector3(side > 0f ? -0.55f : 0.55f, 0.75f, 0f);
            var lsr = lantern.AddComponent<SpriteRenderer>();
            lsr.sprite = lanternSprite;
            if (unlitMaterial != null) lsr.sharedMaterial = unlitMaterial;
            lsr.sortingLayerName = "Interactables";
            lsr.sortingOrder = 6;
            lsr.color = new Color(1f, 1f, 1f, 0f);
            var light = lantern.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.color = new Color(1f, 0.7f, 0.35f);
            light.intensity = 1.2f;
            light.pointLightOuterRadius = 3.5f;
            light.pointLightInnerRadius = 0.4f;
            lantern.AddComponent<LightFlicker>();
            return go;
        }

        static void SetAlpha(GameObject root, float a)
        {
            foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>())
            {
                var c = sr.color; c.a = a; sr.color = c;
            }
        }

        IEnumerator ScatterGems(Vector3 origin)
        {
            const int count = 36;
            var gems = new Transform[count];
            var vel = new Vector2[count];
            var srs = new SpriteRenderer[count];
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("ScatteredGem");
                go.transform.position = origin;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = gemSprites != null && gemSprites.Length > 0 ? gemSprites[i % gemSprites.Length] : Fx.PixelSprite;
                if (unlitMaterial != null) sr.sharedMaterial = unlitMaterial;
                sr.sortingLayerName = "FX";
                gems[i] = go.transform;
                srs[i] = sr;
                float angle = Random.Range(10f, 170f) * Mathf.Deg2Rad;
                vel[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(6f, 13f);
            }

            // The gem counter drains with them.
            int gemsLeft = GameSession.Instance.Gems;
            float drainStep = gemsLeft > 0 ? 1.4f / gemsLeft : 0f;
            float drainTimer = 0f;
            for (float t = 0; t < 2.2f; t += Time.unscaledDeltaTime)
            {
                float dt = Time.unscaledDeltaTime;
                for (int i = 0; i < count; i++)
                {
                    if (gems[i] == null) continue;
                    gems[i].position += (Vector3)(vel[i] * dt);
                    gems[i].Rotate(0f, 0f, 220f * dt * (i % 2 == 0 ? 1f : -1f));
                    var c = srs[i].color; c.a = 1f - t / 2.2f; srs[i].color = c;
                }
                drainTimer += dt;
                while (gemsLeft > 0 && drainTimer >= drainStep)
                {
                    drainTimer -= drainStep;
                    gemsLeft--;
                    if (GameSession.Instance.Gems > 0) GameSession.Instance.AddGems(-1); // never below zero
                }
                yield return null;
            }
            if (GameSession.Instance.Gems > 0) GameSession.Instance.AddGems(-GameSession.Instance.Gems);
            foreach (var g in gems) if (g != null) Destroy(g.gameObject);
        }
    }
}
