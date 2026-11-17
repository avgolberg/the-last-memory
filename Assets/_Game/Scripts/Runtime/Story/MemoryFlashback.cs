using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TileVania
{
    /// <summary>
    /// The flashback in the ending (Docs/STORY_SPEC.md §2.4). Four short memories, played with two actors, the
    /// camera and the lighting, no timeline:
    ///   1. Leaving home      - the cottage at night: the promise, and the lantern in the window.
    ///   2. The last encounter - Lia follows with her lantern; the hero sends her home and walks on.
    ///   3. Returning home    - dawn: the house is empty and Lia is never found.
    ///   4. The bargain       - in this same hall: the hero asks the Keeper to take the memory, and to never give it back.
    /// Memories 1-3 use the "MemoryStage" set that the level builder places to the right of the Treasure Chamber;
    /// memory 4 uses the chamber itself. StoryDirector starts this from the ending.
    /// </summary>
    public class MemoryFlashback : MonoBehaviour
    {
        /// <summary>Where the stage starts (world x). It is far enough right that the level is never in view.</summary>
        public const float StageX = 70f;
        /// <summary>The cottage's centre on the stage (stage-local x). The ground surface is at y = 0.</summary>
        public const float HouseX = 6f;
        /// <summary>The forest path where Lia catches up (stage-local x of the camera).</summary>
        public const float ForestX = 24f;

        const float NormalZoom = 6.75f; // the gameplay camera (5x pixel scale at 1080p)
        const float CloseZoom = 5.625f; // 6x at 1080p

        [Header("Actors")]
        public MemoryActor hero;
        public MemoryActor lia;
        [Tooltip("The lantern Lia carries (child of Lia, with a light).")]
        public GameObject liaLantern;
        [Tooltip("The sack of gold the hero brings home (child of the hero).")]
        public GameObject goldSack;

        [Header("The cottage")]
        [Tooltip("World x of the door and of the window.")]
        public float doorX, windowX;
        [Tooltip("The room seen through the window: warm when lit, dark when empty.")]
        public SpriteRenderer room;
        [Tooltip("The lantern on the windowsill (with a light).")]
        public GameObject windowLantern;
        [Tooltip("Light from the open door.")]
        public GameObject doorway;

        [Header("Sky")]
        public SpriteRenderer sky;
        public SpriteRenderer stars;

        [Header("The present scene")]
        public Light2D globalLight;
        [Tooltip("The level's parallax background (hidden while the camera is on the stage).")]
        public GameObject levelBackdrop;
        public DragonBoss keeper;
        public PlayerController player;
        public TreasureChest chest;

        static readonly Color Sepia = new Color(0.55f, 0.38f, 0.2f);
        static readonly Color RoomLit = new Color(1f, 0.78f, 0.42f);
        static readonly Color RoomDark = new Color(0.08f, 0.06f, 0.07f);

        /// <summary>Which memory is playing (1-4), or 0 when none. Tests follow the sequence with it.</summary>
        public static int CurrentMemory { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => CurrentMemory = 0;

        void Awake()
        {
            // The actors only appear inside the memories.
            if (hero != null) hero.gameObject.SetActive(false);
            if (lia != null) lia.gameObject.SetActive(false);
        }

        void OnDestroy() => CurrentMemory = 0;

        static float Stage(float localX) => StageX + localX;
        static IEnumerator Wait(float seconds) => StoryDirector.Wait(seconds);
        static IEnumerator Say(DialogueLine[] lines) => DialogueBox.Run(lines, DialogueMode.Cinematic);

        /// <summary>
        /// Plays the four memories. It starts and ends on a white screen; the caller fades in and out of white.
        /// <paramref name="presentFigure"/> (Lia's figure in the chest light) is hidden while memory 4 plays.
        /// </summary>
        public IEnumerator Play(GameObject presentFigure)
        {
            var cam = CameraController.Current;
            Color lightColor = globalLight != null ? globalLight.color : Color.white;
            float lightIntensity = globalLight != null ? globalLight.intensity : 1f;
            HUD.SetVisible(false);
            if (levelBackdrop != null) levelBackdrop.SetActive(false);

            yield return LeavingHome(cam);
            yield return LastEncounter(cam);
            yield return ReturningHome(cam);
            yield return TheBargain(cam, presentFigure);

            // Back to the present (still under the white screen).
            CurrentMemory = 0;
            hero.gameObject.SetActive(false);
            lia.gameObject.SetActive(false);
            SetLight(lightColor, lightIntensity);
            if (cam != null) cam.EndShot();
        }

        // ------------------------------------------------------------------ 1. leaving home
        IEnumerator LeavingHome(CameraController cam)
        {
            CurrentMemory = 1;
            SetNight();
            SetRoom(lit: true, lanternInWindow: false, doorOpen: true);
            ShowActor(lia, doorX - 0.1f, 1);
            liaLantern.SetActive(false);
            ShowActor(hero, doorX + 1.3f, -1);
            goldSack.SetActive(false);
            if (cam != null) cam.CutTo(new Vector2(doorX + 1.2f, 2.6f), CloseZoom);

            yield return CinematicOverlay.FadeTo(Color.white, 0f, 1.2f);
            yield return Wait(0.5f);
            yield return Say(StoryScript.MemoryLeaving);

            // A moment together, then the hero sets off toward the castle. Lia watches, and goes inside.
            yield return hero.WalkTo(doorX + 0.75f, 0.5f);
            yield return Wait(0.8f);
            hero.StartWalk(doorX + 10f);
            yield return Wait(2.2f);
            yield return lia.FadeTo(0f, 0.6f);
            doorway.SetActive(false);
            yield return Wait(0.4f);
            yield return CinematicOverlay.FadeTo(Color.black, 1f, 0.9f);
        }

        // ------------------------------------------------------------------ 2. the last encounter
        IEnumerator LastEncounter(CameraController cam)
        {
            CurrentMemory = 2;

            // a) Lia waits at the window with the lantern... then takes it and goes out.
            SetRoom(lit: true, lanternInWindow: true, doorOpen: false);
            hero.gameObject.SetActive(false);
            ShowActor(lia, windowX + 0.25f, 1, y: 0.55f);
            SetBehindCottage(lia, true);
            if (cam != null) cam.CutTo(new Vector2(windowX + 1.2f, 2.3f), CloseZoom);
            yield return CinematicOverlay.FadeTo(Color.black, 0f, 0.9f);
            yield return Wait(2.2f);
            windowLantern.SetActive(false);
            AudioManager.Play(Sfx.UISelect, 0.3f);
            SetRoom(lit: false, lanternInWindow: false, doorOpen: false);
            yield return lia.FadeTo(0f, 0.4f);
            yield return Wait(1.0f);
            yield return CinematicOverlay.FadeTo(Color.black, 1f, 0.7f);

            // b) The forest path. The hero is on the way to the castle; Lia runs after them.
            float shot = Stage(ForestX);
            SetBehindCottage(lia, false);
            ShowActor(hero, shot - 3f, 1);
            ShowActor(lia, shot - 13f, 1);
            liaLantern.SetActive(true);
            liaLantern.transform.localPosition = LanternHeld;
            if (cam != null) cam.CutTo(new Vector2(shot, 3f), NormalZoom);
            yield return CinematicOverlay.FadeTo(Color.black, 0f, 0.8f);

            var walk = hero.StartWalk(shot + 2.5f, 0.8f);
            yield return Wait(0.7f);
            var run = lia.StartWalk(shot - 1.5f, 1.7f);
            yield return Say(StoryScript.MemoryFollowCall);
            yield return walk;
            yield return Wait(0.2f);
            hero.Face(-1); // turns around
            yield return run;
            yield return Wait(0.3f);
            yield return Say(StoryScript.MemoryFollowHarsh);

            // The hero turns away and walks on. Lia stays, and lowers her lantern.
            hero.Face(1);
            yield return Wait(0.4f);
            hero.StartWalk(shot + 14f, 1.25f);
            yield return Wait(1.3f);
            liaLantern.transform.localPosition = LanternLowered;
            if (cam != null) cam.PanTo(new Vector2(lia.transform.position.x + 1.5f, 2.6f));
            yield return Wait(1.4f);
            yield return Say(StoryScript.MemoryFollowLast);
            yield return Wait(0.6f);
            yield return CinematicOverlay.FadeTo(Color.black, 1f, 1.2f);
            lia.gameObject.SetActive(false);
        }

        static readonly Vector3 LanternHeld = new Vector3(0.45f, 0.5f, 0f);
        static readonly Vector3 LanternLowered = new Vector3(0.38f, 0.36f, 0f);

        // ------------------------------------------------------------------ 3. returning home
        IEnumerator ReturningHome(CameraController cam)
        {
            CurrentMemory = 3;
            SetDawn();
            SetRoom(lit: false, lanternInWindow: false, doorOpen: false);
            ShowActor(hero, doorX + 9f, -1);
            goldSack.SetActive(true);
            if (cam != null) cam.CutTo(new Vector2(doorX + 2.5f, 2.6f), CloseZoom);
            yield return CinematicOverlay.FadeTo(Color.black, 0f, 1.0f);

            yield return hero.WalkTo(doorX + 1.2f);
            yield return Say(StoryScript.MemoryReturnArrive);

            // Inside: nobody. The lantern is gone too.
            yield return hero.WalkTo(doorX);
            doorway.SetActive(true);
            yield return hero.FadeTo(0f, 0.3f);
            yield return Wait(1.2f);
            yield return hero.FadeTo(1f, 0.3f);
            doorway.SetActive(false);
            hero.Face(1);
            yield return Say(StoryScript.MemoryReturnEmpty);

            // The gold no longer matters.
            DropSack();
            yield return Wait(0.7f);
            if (cam != null) cam.PanTo(new Vector2(doorX + 5f, 2.6f));
            yield return hero.WalkTo(doorX + 6.5f, 2f);
            CameraController.Shake(0.06f, 0.3f);
            yield return Say(StoryScript.MemoryReturnShout);
            yield return Wait(0.5f);
            yield return Say(StoryScript.MemoryReturnSearch);
            yield return CinematicOverlay.FadeTo(Color.black, 1f, 1.2f);
        }

        void DropSack()
        {
            var p = goldSack.transform.position;
            goldSack.SetActive(false);
            Fx.Burst(new Vector3(p.x, 0.2f, 0f), new Color(1f, 0.82f, 0.3f), 18, 3f, 0.7f, -6f, 2f);
            AudioManager.Play(Sfx.Gem, 0.6f);
        }

        // ------------------------------------------------------------------ 4. the bargain
        IEnumerator TheBargain(CameraController cam, GameObject presentFigure)
        {
            CurrentMemory = 4;
            // The same hall, long ago: no chest yet, no wounded Keeper, nobody else.
            if (levelBackdrop != null) levelBackdrop.SetActive(true);
            var heroVisual = player != null ? player.visual.gameObject : null;
            if (heroVisual != null) heroVisual.SetActive(false);
            bool chestShown = chest != null && chest.gameObject.activeSelf;
            if (chest != null) chest.gameObject.SetActive(false);
            if (presentFigure != null) presentFigure.SetActive(false);
            SetLight(new Color(1f, 0.82f, 0.62f), 0.55f); // an old, warm memory

            Vector3 keeperRest = Vector3.zero;
            Color keeperColor = Color.white;
            float floor = 3f, centre = 25f;
            if (keeper != null)
            {
                keeperRest = keeper.transform.position;
                keeperColor = keeper.spriteRenderer.color;
                floor = keeper.floorY;
                centre = (keeper.arenaLeft + keeper.arenaRight) * 0.5f;
                keeper.transform.position = new Vector3(centre + 4f, floor + 3.5f, 0f);
                keeper.spriteRenderer.color = Color.white;
                keeper.FaceTowards(centre - 10f);
            }
            ShowActor(hero, centre - 9f, 1, y: floor);
            goldSack.SetActive(false);
            if (cam != null) cam.CutTo(new Vector2(centre, floor + 4f), NormalZoom);
            yield return CinematicOverlay.FadeTo(Sepia, 0.22f, 1.0f); // a faint warm veil: this is the past

            yield return hero.WalkTo(centre - 1.5f, 0.6f);
            yield return Wait(0.6f);
            yield return Say(StoryScript.MemoryBargain);

            // The Keeper takes the memory.
            Fx.Burst(hero.transform.position + Vector3.up * 1.2f, new Color(1f, 0.92f, 0.7f), 30, 3f, 1.2f, -1f, 2f);
            AudioManager.Play(Sfx.Portal, 0.6f);
            CameraController.Shake(0.1f, 0.5f);
            yield return Wait(0.9f);
            yield return CinematicOverlay.FadeTo(Color.white, 1f, 1.0f);

            // Restore the present under the white screen.
            if (keeper != null)
            {
                keeper.transform.position = keeperRest;
                keeper.spriteRenderer.color = keeperColor;
                if (player != null) keeper.FaceTowards(player.transform.position.x);
            }
            if (heroVisual != null) heroVisual.SetActive(true);
            if (chest != null) chest.gameObject.SetActive(chestShown);
            if (presentFigure != null) presentFigure.SetActive(true);
        }

        // ------------------------------------------------------------------ helpers
        void ShowActor(MemoryActor actor, float x, int facing, float y = 0f)
        {
            actor.gameObject.SetActive(true);
            actor.Place(x, y, facing);
            foreach (var r in actor.GetComponentsInChildren<SpriteRenderer>(true))
            {
                var c = r.color; c.a = 1f; r.color = c;
            }
        }

        /// <summary>Lia at the window stands inside the cottage: drawn behind its walls, seen through the panes.</summary>
        static void SetBehindCottage(MemoryActor actor, bool behind)
        {
            foreach (var r in actor.GetComponentsInChildren<SpriteRenderer>(true))
            {
                r.sortingLayerName = behind ? "Background" : "Player";
                r.sortingOrder = behind ? 28 : (r == actor.body ? 0 : 1);
            }
        }

        void SetRoom(bool lit, bool lanternInWindow, bool doorOpen)
        {
            if (room != null) room.color = lit ? RoomLit : RoomDark;
            if (windowLantern != null) windowLantern.SetActive(lanternInWindow);
            if (doorway != null) doorway.SetActive(doorOpen);
        }

        void SetNight()
        {
            SetLight(new Color(0.55f, 0.6f, 1f), 0.5f);
            if (sky != null) sky.color = new Color(0.07f, 0.08f, 0.18f);
            if (stars != null) stars.color = new Color(0.8f, 0.85f, 1f, 0.7f);
        }

        void SetDawn()
        {
            SetLight(new Color(1f, 0.83f, 0.74f), 0.85f);
            if (sky != null) sky.color = new Color(0.62f, 0.5f, 0.58f);
            if (stars != null) stars.color = new Color(1f, 1f, 1f, 0f);
        }

        void SetLight(Color color, float intensity)
        {
            if (globalLight == null) return;
            globalLight.color = color;
            globalLight.intensity = intensity;
        }
    }
}
