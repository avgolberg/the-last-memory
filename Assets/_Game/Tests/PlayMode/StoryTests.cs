using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TileVania.Tests
{
    /// <summary>
    /// Narrative coverage: the dialogue box, story-beat ordering, story points placed by the layouts,
    /// memory shards, the boss conversations, and the ending's loop back to the forest.
    /// Coordinates match the ASCII layouts (story markers are the digits 1-9).
    /// </summary>
    [Timeout(120000)] // a stuck dialogue (time scale 0) must fail a test, not hang the whole run
    public class StoryTests
    {
        PlayerController player;
        int savedCycles;

        static readonly DialogueLine[] TwoLines =
        {
            new DialogueLine("Tester", "The first line of a short conversation."),
            new DialogueLine("Tester", "And the second one."),
        };

        [SetUp]
        public void SetUp() => savedCycles = Story.CompletedCycles;

        [TearDown]
        public void TearDown()
        {
            GameInput.Simulated = default;
            DialogueBox.TestAutoAdvance = false;
            StoryDirector.CutsceneSpeed = 1f;
            Time.timeScale = 1f;
            Story.SetCompletedCycles(savedCycles);
        }

        IEnumerator Load(string scene, Story.Opening opening = Story.Opening.None, bool autoAdvance = false)
        {
            while (SceneLoader.IsLoading) yield return null;
            GameInput.Simulated = new GameInput.SimulatedInput { Enabled = true };
            DialogueBox.TestAutoAdvance = autoAdvance;
            StoryDirector.CutsceneSpeed = 6f;
            Story.BeginNewCycle(opening);
            SceneManager.LoadScene(scene);
            yield return null;
            yield return null;
            player = Object.FindFirstObjectByType<PlayerController>();
            yield return new WaitForSeconds(0.3f);
        }

        IEnumerator Teleport(Vector2 pos)
        {
            player.Teleport(pos);
            yield return new WaitForSeconds(0.3f);
        }

        /// <summary>Presses an advance key (in real time, so it also works while the world is frozen).</summary>
        static IEnumerator PressAdvance()
        {
            GameInput.Simulated.AdvancePressed = true;
            yield return new WaitForSecondsRealtime(0.2f);
        }

        static IEnumerator WaitUntilReal(System.Func<bool> condition, float timeout)
        {
            for (float t = 0; t < timeout && !condition(); t += Time.unscaledDeltaTime) yield return null;
        }

        // ------------------------------------------------------------------ story state

        [Test]
        public void Beats_FireOnce_AndNeverBeforeTheirPrerequisite()
        {
            Story.BeginNewCycle(Story.Opening.None);
            Assert.IsFalse(Story.TryBegin("boss_defeat", "boss_intro"), "the defeat line needs the intro first");
            Assert.IsTrue(Story.TryBegin("boss_intro"));
            Assert.IsFalse(Story.TryBegin("boss_intro"), "a beat fires once per cycle");
            Assert.IsTrue(Story.TryBegin("boss_defeat", "boss_intro"));
            Story.BeginNewCycle(Story.Opening.None);
            Assert.IsTrue(Story.TryBegin("boss_intro"), "a new cycle resets the beats");
        }

        [Test]
        public void StoryText_UsesTheNotchCount()
        {
            Story.SetCompletedCycles(0);
            Assert.AreEqual(Story.NotchesBeforeFirstPlay, Story.NotchCount);
            Assert.AreEqual("Seven of them.", Story.Format("{Notches} of them."));
            Story.SetCompletedCycles(1);
            Assert.AreEqual("eight times", Story.Format("{notches} times"));
        }

        [Test]
        public void Script_EveryBeatHasLines_AndTheNameIsNeverFinished()
        {
            foreach (var b in StoryScript.AllBeats)
                Assert.IsTrue(b.lines != null && b.lines.Length > 0 && b.lines.Length <= 6, $"beat {b.id} should have 1-6 lines");
            Assert.IsTrue(StoryScript.EndingName(false)[0].cutOff);
            StringAssert.Contains("Li—", StoryScript.EndingName(true)[0].text, "all memories: one syllable further");
            Assert.AreEqual("This time, don't open the chest.", StoryScript.LoopNote[0].text);
        }

        // ------------------------------------------------------------------ dialogue box

        [UnityTest]
        public IEnumerator Dialogue_Blocking_FreezesWorld_AndAdvancesWithKeys()
        {
            yield return Load(SceneLoader.Level1);
            bool done = false;
            DialogueBox.Play(TwoLines, DialogueMode.Blocking, () => done = true);
            yield return null;
            Assert.IsTrue(DialogueBox.IsBlocking);
            Assert.AreEqual(0f, Time.timeScale, "a blocking dialogue freezes enemies and projectiles");
            Assert.IsFalse(player.ControlEnabled, "the hero can't move during the conversation");
            Assert.AreEqual("Tester", DialogueBox.CurrentSpeaker);

            yield return new WaitForSecondsRealtime(0.25f);
            yield return PressAdvance();                 // completes the typewriter
            Assert.AreEqual(TwoLines[0].text, DialogueBox.CurrentText, "first press only finishes the line");
            yield return PressAdvance();                 // next line
            Assert.AreEqual(TwoLines[1].text, DialogueBox.CurrentText);
            yield return PressAdvance();
            yield return PressAdvance();
            yield return WaitUntilReal(() => done, 2f);
            Assert.IsTrue(done);
            Assert.IsFalse(DialogueBox.IsOpen);
            Assert.AreEqual(1f, Time.timeScale);
            yield return new WaitForSecondsRealtime(0.25f);
            Assert.IsTrue(player.ControlEnabled, "control returns shortly after the last line");
        }

        [UnityTest]
        public IEnumerator Dialogue_Bark_NeverTakesControl()
        {
            yield return Load(SceneLoader.Level1);
            Story.Set("forest_tree"); // running right passes the tree; only the bark is under test here
            DialogueBox.Play(StoryScript.FirstGem, DialogueMode.Bark);
            yield return null;
            Assert.IsTrue(DialogueBox.IsBark);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsTrue(player.ControlEnabled);
            float x0 = player.transform.position.x;
            GameInput.Simulated.Move = new Vector2(1f, 0f);
            yield return new WaitForSeconds(0.5f);
            GameInput.Simulated.Move = Vector2.zero;
            Assert.Greater(player.transform.position.x, x0 + 1f, "the hero keeps running during a thought");
            yield return WaitUntilReal(() => !DialogueBox.IsOpen, 12f);
            Assert.IsFalse(DialogueBox.IsOpen, "barks close by themselves");
        }

        [UnityTest]
        public IEnumerator Pause_IsIgnoredDuringDialogue()
        {
            yield return Load(SceneLoader.Level1);
            DialogueBox.Play(TwoLines, DialogueMode.Blocking);
            yield return null;
            GameInput.Simulated.PausePressed = true;
            yield return null;
            yield return null;
            Assert.IsFalse(GameMenus.IsPaused, "Esc must not open the pause menu over a dialogue");
            DialogueBox.TestAutoAdvance = true;
            yield return WaitUntilReal(() => !DialogueBox.IsOpen, 2f);
            Assert.AreEqual(1f, Time.timeScale);
        }

        // ------------------------------------------------------------------ forest

        [UnityTest]
        public IEnumerator Opening_FirstWaking_PlaysOnce_ThenGivesControl()
        {
            yield return Load(SceneLoader.Level1, Story.Opening.FirstWaking, autoAdvance: true);
            Assert.IsTrue(StoryDirector.InCutscene, "the opening starts as the forest loads");
            Assert.IsFalse(player.ControlEnabled);
            Assert.AreEqual(Story.Opening.None, Story.PendingOpening, "the opening is consumed");
            yield return WaitUntilReal(() => !StoryDirector.InCutscene, 20f);
            Assert.IsFalse(StoryDirector.InCutscene);
            Assert.IsTrue(player.ControlEnabled, "the player takes over after waking up");
            Assert.AreEqual(Quaternion.identity, player.visual.localRotation, "the hero is standing");
            Assert.Less(CinematicOverlay.FadeAlpha, 0.05f, "the screen has faded in");
            Assert.IsFalse(Story.Has("forest_tree"), "the tree is a separate beat, met by walking");
        }

        [UnityTest]
        public IEnumerator NotchedTree_ShowsTheCount_AndTriggersOnce()
        {
            yield return Load(SceneLoader.Level1, autoAdvance: true);
            var marks = Object.FindFirstObjectByType<TallyMarks>();
            Assert.IsNotNull(marks, "the notched tree is in the forest");
            Assert.AreEqual(Story.NotchCount, marks.Count);
            Story.Set("first_gem"); // the run continues into the gems; count only the tree's dialogue
            int started = DialogueBox.StartedCount;
            GameInput.Simulated.Move = new Vector2(1f, 0f);
            yield return new WaitForSeconds(1.0f);
            GameInput.Simulated.Move = Vector2.zero;
            Assert.IsTrue(Story.Has("forest_tree"), "walking past the tree reads the note");
            Assert.AreEqual(started + 1, DialogueBox.StartedCount);
            yield return Teleport(new Vector2(3.5f, 8f));
            yield return Teleport(new Vector2(6.5f, 8f));
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(started + 1, DialogueBox.StartedCount, "the note is read once per cycle");
        }

        [UnityTest]
        public IEnumerator Lantern_NeedsTheExamineKey()
        {
            yield return Load(SceneLoader.Level1, autoAdvance: true);
            int started = DialogueBox.StartedCount;
            yield return Teleport(new Vector2(74.5f, 10f));
            Assert.IsTrue(HUD.PromptVisible, "standing by the lantern shows the Examine prompt");
            Assert.AreEqual(started, DialogueBox.StartedCount, "nothing plays until the player asks");
            GameInput.Simulated.InteractPressed = true;
            yield return new WaitForSeconds(0.2f);
            Assert.AreEqual(started + 1, DialogueBox.StartedCount);
            Assert.IsTrue(Story.Has("forest_lantern"));
            yield return Teleport(new Vector2(60.5f, 10f));
            Assert.IsFalse(HUD.PromptVisible, "the prompt hides when walking away");
        }

        [UnityTest]
        public IEnumerator MemoryShard_IsRemembered_AndStaysFoundAfterRestart()
        {
            yield return Load(SceneLoader.Level1, autoAdvance: true);
            var shard = Object.FindObjectsByType<MemoryShard>(FindObjectsSortMode.None).First(s => s.memoryId == "memory_1");
            int gems = GameSession.Instance.Gems;
            yield return Teleport(shard.transform.position + Vector3.down * 0.6f);
            yield return new WaitForSeconds(0.3f);
            Assert.IsTrue(Story.Has("memory_1"));
            Assert.AreEqual(1, Story.MemoriesFound);
            Assert.AreEqual(gems + 3, GameSession.Instance.Gems, "a shard is worth three gems");

            // Restart the level (same cycle): the recovered memory must not come back.
            SceneManager.LoadScene(SceneLoader.Level1);
            yield return null;
            yield return null;
            yield return new WaitForSeconds(0.2f);
            Assert.IsFalse(Object.FindObjectsByType<MemoryShard>(FindObjectsSortMode.None).Any(s => s.memoryId == "memory_1"));
            Assert.IsTrue(Object.FindObjectsByType<MemoryShard>(FindObjectsSortMode.None).Any(s => s.memoryId == "memory_2"));
        }

        [UnityTest]
        public IEnumerator Layouts_PlaceEveryStoryPoint()
        {
            var expected = new Dictionary<string, string[]>
            {
                { SceneLoader.Level1, new[] { "forest_lantern", "forest_tree", "memory_1", "memory_2" } },
                { SceneLoader.Level2, new[] { "castle_dejavu", "castle_door", "castle_ghost", "memory_3", "memory_4" } },
            };
            foreach (var kv in expected)
            {
                yield return Load(kv.Key);
                var ids = Object.FindObjectsByType<StoryPoint>(FindObjectsSortMode.None).Select(p => p.beatId)
                    .Concat(Object.FindObjectsByType<MemoryShard>(FindObjectsSortMode.None).Select(m => m.memoryId))
                    .OrderBy(i => i).ToArray();
                CollectionAssert.AreEqual(kv.Value, ids, kv.Key);
                Assert.IsNotNull(Object.FindFirstObjectByType<StoryDirector>());
            }
        }

        // ------------------------------------------------------------------ castle

        [UnityTest]
        public IEnumerator CastleDoor_WaitsForTheGhost()
        {
            yield return Load(SceneLoader.Level2, autoAdvance: true);
            int started = DialogueBox.StartedCount;
            yield return Teleport(new Vector2(123.5f, 3f));
            Assert.IsFalse(Story.Has("castle_door"), "out of order: the door beat needs the ghost first");
            Assert.AreEqual(started, DialogueBox.StartedCount);

            yield return Teleport(new Vector2(60.5f, 3f));
            Assert.IsTrue(Story.Has("castle_ghost"));
            yield return Teleport(new Vector2(110.5f, 3f));
            yield return Teleport(new Vector2(123.5f, 3f));
            Assert.IsTrue(Story.Has("castle_door"), "in order, the door beat plays");
            Assert.AreEqual(started + 2, DialogueBox.StartedCount);
        }

        // ------------------------------------------------------------------ treasure chamber

        [UnityTest]
        public IEnumerator Boss_SpeaksBeforeTheFight_WhileTheWorldIsFrozen()
        {
            yield return Load(SceneLoader.Boss);
            var boss = Object.FindFirstObjectByType<DragonBoss>();
            yield return Teleport(new Vector2(12.5f, 3f));
            yield return WaitUntilReal(() => DialogueBox.IsBlocking, 5f);
            Assert.IsTrue(DialogueBox.IsBlocking, "the dragon recognises the hero");
            Assert.AreEqual(StoryScript.Dragon, DialogueBox.CurrentSpeaker);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.IsFalse(boss.hurtbox.enabled, "no attacks or damage until the conversation ends");

            for (int i = 0; i < 20 && DialogueBox.IsOpen; i++) yield return PressAdvance();
            Assert.IsFalse(DialogueBox.IsOpen);
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsTrue(boss.IsFighting);
            Assert.IsTrue(boss.hurtbox.enabled, "the fight starts after the dialogue");
            Assert.IsTrue(Story.Has("boss_intro"));
        }

        [UnityTest]
        public IEnumerator Boss_RetryDoesNotRepeatTheConversation()
        {
            yield return Load(SceneLoader.Boss, autoAdvance: true);
            var boss = Object.FindFirstObjectByType<DragonBoss>();
            yield return Teleport(new Vector2(12.5f, 3f));
            yield return new WaitForSeconds(2.5f);
            int started = DialogueBox.StartedCount;
            player.GetComponent<PlayerHealth>().TakeDamage(new DamageInfo(3, Vector2.zero));
            yield return new WaitForSeconds(2.2f);
            LevelManager.Instance.Retry();
            yield return new WaitForSeconds(0.3f);
            yield return Teleport(new Vector2(12.5f, 3f));
            yield return new WaitForSeconds(1.6f);
            Assert.IsTrue(boss.IsFighting);
            Assert.AreEqual(started + 1, DialogueBox.StartedCount, "only the short 'Again, then.' line plays");
            Assert.AreEqual(1f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator Ending_LoopsBackToTheForest_WithOneMoreNotch()
        {
            yield return Load(SceneLoader.Boss, autoAdvance: true);
            int cyclesBefore = Story.CompletedCycles;
            var boss = Object.FindFirstObjectByType<DragonBoss>();
            yield return Teleport(new Vector2(12.5f, 3f));
            yield return new WaitForSeconds(2.5f);
            player.GetComponent<PlayerHealth>().Heal(3);
            while (!boss.IsDead) { boss.TakeDamage(new DamageInfo(1, Vector2.zero)); yield return null; }
            yield return new WaitForSeconds(3.5f);
            Assert.IsTrue(Story.Has("boss_defeat"), "the dragon's last words follow its intro");
            GameSession.Instance.AddGems(5);
            var chest = GameObject.Find("Chest_Gold");
            Assert.IsNotNull(chest);
            yield return Teleport(chest.transform.position + Vector3.up * 0.1f);
            yield return WaitUntilReal(() => StoryDirector.InCutscene, 2f);
            Assert.IsTrue(StoryDirector.InCutscene, "the chest starts the ending, not a victory screen");
            Assert.IsFalse(GameMenus.EndingVisible);

            yield return WaitUntilReal(() => SceneManager.GetActiveScene().name == SceneLoader.Level1 && GameMenus.EndingVisible, 40f);
            Assert.AreEqual(SceneLoader.Level1, SceneManager.GetActiveScene().name, "everything fades back to the forest");
            Assert.IsTrue(GameMenus.EndingVisible, "the loop ends on the ending panel");
            Assert.AreEqual(cyclesBefore + 1, Story.CompletedCycles, "one more cycle");
            var marks = Object.FindFirstObjectByType<TallyMarks>();
            Assert.AreEqual(Story.NotchesBeforeFirstPlay + cyclesBefore + 1, marks.Count, "one more notch on the tree");
            Assert.AreEqual(marks.Count, Story.LastCycle.notches);
            Assert.GreaterOrEqual(Story.LastCycle.gems, 5, "the panel reports the gems of the finished cycle");
            Assert.AreEqual(0, GameSession.Instance.Gems, "the memories scattered: the new cycle starts empty");
            Assert.IsTrue(Story.Has("forest_tree"), "the loop already showed the note");
            Assert.AreEqual(Quaternion.identity, Object.FindFirstObjectByType<PlayerController>().visual.localRotation);

            player = Object.FindFirstObjectByType<PlayerController>();
            Assert.IsFalse(player.ControlEnabled, "the panel decides what happens next");
            GameObject.Find("Button_BeginAgain").GetComponent<Button>().onClick.Invoke();
            yield return new WaitForSeconds(0.3f);
            Assert.IsFalse(GameMenus.EndingVisible);
            Assert.IsTrue(player.ControlEnabled, "Begin Again continues the new cycle");
        }

        [UnityTest]
        public IEnumerator MainMenu_Play_StartsWithTheOpening()
        {
            while (SceneLoader.IsLoading) yield return null;
            GameInput.Simulated = default;
            StoryDirector.CutsceneSpeed = 6f;
            DialogueBox.TestAutoAdvance = true;
            SceneManager.LoadScene(SceneLoader.MainMenu);
            yield return new WaitForSeconds(0.5f);
            GameObject.Find("Button_Play").GetComponent<Button>().onClick.Invoke();
            yield return WaitUntilReal(() => SceneManager.GetActiveScene().name == SceneLoader.Level1, 5f);
            yield return null;
            yield return null;
            Assert.IsTrue(StoryDirector.InCutscene, "a new game opens on the hero waking up");
            yield return WaitUntilReal(() => !StoryDirector.InCutscene, 20f);
            Assert.IsTrue(Object.FindFirstObjectByType<PlayerController>().ControlEnabled);
        }
    }
}
