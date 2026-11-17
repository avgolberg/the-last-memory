using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TileVania.Tests
{
    /// <summary>
    /// Not part of the normal run ([Explicit]): plays the real ending at normal speed and saves screenshots of the
    /// flashback to Docs/Screenshots/Flashback_*.png for visual review. Run it on its own:
    /// -runTests -testPlatform PlayMode -testFilter FlashbackScreenshots
    /// </summary>
    [Explicit, Timeout(400000)]
    public class FlashbackScreenshots
    {
        static readonly Vector2Int Size = new Vector2Int(1920, 1080);

        [TearDown]
        public void TearDown()
        {
            GameInput.Simulated = default;
            DialogueBox.TestAutoAdvance = false;
            StoryDirector.CutsceneSpeed = 1f;
        }

        [UnityTest]
        public IEnumerator CaptureTheEnding()
        {
            int savedCycles = Story.CompletedCycles;
            while (SceneLoader.IsLoading) yield return null;
            GameInput.Simulated = new GameInput.SimulatedInput { Enabled = true };
            DialogueBox.TestAutoAdvance = true; // the boss conversation
            Story.BeginNewCycle(Story.Opening.None);
            SceneManager.LoadScene(SceneLoader.Boss);
            yield return null; yield return null;
            var player = Object.FindFirstObjectByType<PlayerController>();
            var boss = Object.FindFirstObjectByType<DragonBoss>();
            player.Teleport(new Vector2(12.5f, 3f));
            yield return new WaitForSeconds(3f);
            player.GetComponent<PlayerHealth>().Heal(3);
            while (!boss.IsDead) { boss.TakeDamage(new DamageInfo(1, Vector2.zero)); yield return null; }
            GameObject chest = null;
            for (float t = 0; t < 10f && (chest = GameObject.Find("Chest_Gold")) == null; t += Time.unscaledDeltaTime) yield return null;
            yield return new WaitForSeconds(0.5f);
            yield return Capture("Flashback_00_keeper_rests");
            DialogueBox.TestAutoAdvance = false;

            // Lines to photograph (a little after they appear), and the shots without dialogue.
            var wanted = new Dictionary<string, string>
            {
                { "I know that voice...", "01_present_figure" },
                { "Will you be home by morning?", "02_leaving_home" },
                { "Then I'll keep the lantern in the window, so you can find your way home.", "03_lantern_promise" },
                { "Wait! Wait for me!", "05_lia_follows" },
                { "Go home! I don't want you following me!", "06_last_words" },
                { "(That was the last time I ever saw her.)", "07_alone" },
                { "Lia! I'm back! Look, we're rich!", "08_dawn" },
                { "LIA!", "09_search" },
                { "Your sister?", "10_bargain" },
                { "I promise.", "11_promise" },
                { "Lia...", "12_present_remember" },
                { "Take it back. Take all of it.", "13_choice" },
                { "Then I will keep it for you. As I promised.", "14_keeper" },
                { "I'm sorry, Lia.", "15_apology" },
            };
            string pending = null;
            float shownAt = 0f;
            System.Action<DialogueLine> onLine = l => { shownAt = Time.realtimeSinceStartup; pending = wanted.TryGetValue(l.text, out var n) ? n : null; };
            DialogueBox.LineShown += onLine;

            player.Teleport(chest.transform.position + Vector3.up * 0.1f);
            bool windowShot = false;
            for (float t = 0; t < 300f && SceneManager.GetActiveScene().name != SceneLoader.Level1; t += Time.unscaledDeltaTime)
            {
                if (!windowShot && MemoryFlashback.CurrentMemory == 2 && !DialogueBox.IsOpen && CinematicOverlay.FadeAlpha < 0.05f)
                {
                    windowShot = true;
                    yield return new WaitForSecondsRealtime(0.6f);
                    yield return Capture("Flashback_04_window");
                }
                if (DialogueBox.IsOpen && Time.realtimeSinceStartup - shownAt > 1.6f)
                {
                    if (pending != null) { yield return Capture("Flashback_" + pending); pending = null; }
                    GameInput.Simulated.AdvancePressed = true; // read faster than the automatic pace
                    shownAt = Time.realtimeSinceStartup;
                }
                yield return null;
            }
            for (float t = 0; t < 30f && CinematicOverlay.CaptionAlpha < 0.95f; t += Time.unscaledDeltaTime) yield return null;
            yield return Capture("Flashback_16_final_line");
            DialogueBox.LineShown -= onLine;
            Story.SetCompletedCycles(savedCycles);
        }

        static IEnumerator Capture(string name)
        {
            yield return null; // (WaitForEndOfFrame never resumes in batch mode)
            var cam = Camera.main;
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            var modes = new RenderMode[canvases.Length];
            for (int i = 0; i < canvases.Length; i++)
            {
                modes[i] = canvases[i].renderMode;
                canvases[i].renderMode = RenderMode.ScreenSpaceCamera;
                canvases[i].worldCamera = cam;
                canvases[i].planeDistance = 1f;
                canvases[i].sortingLayerName = "Foreground";
                canvases[i].sortingOrder = 1000;
            }
            var rt = new RenderTexture(Size.x, Size.y, 24);
            var tex = new Texture2D(Size.x, Size.y, TextureFormat.RGB24, false);
            cam.targetTexture = rt;
            Canvas.ForceUpdateCanvases();
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, Size.x, Size.y), 0, 0);
            tex.Apply();
            Directory.CreateDirectory("Docs/Screenshots");
            File.WriteAllBytes($"Docs/Screenshots/{name}.png", tex.EncodeToPNG());
            cam.targetTexture = null;
            RenderTexture.active = null;
            for (int i = 0; i < canvases.Length; i++) canvases[i].renderMode = modes[i];
            Object.Destroy(rt);
            Object.Destroy(tex);
        }
    }
}
