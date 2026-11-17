# The Last Memory: Student Project Guide

A practical explanation of the game and how it is built, for presenting and defending the project.
More detail: `GAME_DESIGN.md` (mechanics and numbers), `STORY_SPEC.md` (every line of the story),
`QA_CHECKLIST.md` (manual test plan), `ASSET_CREDITS.md` (art and audio licences).

---

## 1. Title and overview

**The Last Memory** is a short 2D pixel-art platformer made with **Unity 6 (6000.2.13f1)** and **C#**, using the
Universal Render Pipeline (2D renderer, 2D lights), Tilemaps and the Input System.
A full play-through (three levels, a boss and the ending) takes about 15–25 minutes.

*Project history:* the project started from the "TileVania" platformer of a Unity course. It was then rebuilt
almost completely: new code, new levels, licensed art and audio, a boss, menus, a story and automated tests.
The original course content was removed from the project (it is still in the Git history under the tag
`pre-modernization`). The local folder and the GitHub repository still use the old name.

## 2. Genre and storyline

**Genre:** 2D action platformer with a narrative.

**Story in short:** a hero wakes up in a forest with no memory. A note on an old tree says *"This time, don't
open the chest."*, and the tree has seven notches carved into it. The hero crosses the forest and an abandoned
castle, collecting gems that turn out to be fragments of their memory, and defeats the dragon that guards a
chest: **the Keeper**. When the chest opens, a flashback reveals the truth. The hero once left their little sister
**Lia** at home to steal the dragon's gold, sent her away harshly when she followed them into the forest, and
never saw her again. Unable to live with that, they asked the Keeper to take the memory, and made it promise
never to give it back. Now, remembering everything, the hero is offered the chance to keep the memory, and
**chooses to forget again**. They wake up in the forest. The tree has one more notch.

The story is a loop: each finished game adds a notch, and the count is saved between sessions.
Lia's fate is never revealed. The last line is *"Somewhere, a lantern is still burning."*

## 3. Main gameplay objectives

1. Reach the exit portal of each level (find the key that opens the gate in front of it).
2. In the castle, find the **Treasure Key** to open the door to the Treasure Chamber.
3. Defeat the Keeper and open the chest.
4. Optional: collect gems and the four glowing **memory shards** (finding all four changes the ending slightly).

## 4. The three levels

| Level | Scene | Size (tiles) | Character |
|---|---|---|---|
| 1. The Whispering Forest | `Level1_Forest` | 128×30 | Bright tutorial meadow, ruins with a ladder and the Gold Key, a chasm, an underground cave, a gated portal chamber. Introduces every enemy except the bat. |
| 2. The Abandoned Castle | `Level2_Castle` | 140×34 | Darker, torch-lit. Spike halls crossed on wooden bridges, a broken bridge over a pit, a watchtower climbed with ladders, bats, two keys, and the treasure door. |
| 3. The Treasure Chamber | `Level3_TreasureChamber` | 40×22 | A single hall for the boss fight, then the ending. |

## 5. Player controls

| Action | Keyboard | Gamepad |
|---|---|---|
| Move | ← / → | Left stick / D-pad |
| Jump | ↑, Z or Space | South (A) |
| Climb ladders | ↑ / ↓ near a ladder | Stick up / down |
| Shoot forward | X | West (X) |
| Shoot upward | **C** (or ↑ + X) | Right shoulder (or stick up + West) |
| Drop through a wooden ledge | ↓ + Z | Down + South |
| Examine | Enter (when the prompt shows) | North (Y) |
| Advance dialogue | Z / Space / X / C / Enter | South / East / West |
| Pause | Esc | Start |

## 6. Movement, jumping, climbing and shooting

All of this is in `Scripts/Runtime/Player/PlayerController.cs`. Input is read in `Update`, physics is applied in
`FixedUpdate` on a `Rigidbody2D`.

- **Running** accelerates and decelerates instead of snapping to full speed (`Mathf.MoveTowards` on the
  velocity), which makes the hero feel responsive but not slippery.
- **Jumping** sets an upward velocity. Three standard "game feel" helpers make it forgiving:
  - *coyote time* (0.1 s): you can still jump just after running off a ledge;
  - *jump buffer* (0.12 s): a jump pressed just before landing still happens;
  - *variable height*: releasing the key early cuts the jump short.
  Falling uses stronger gravity than rising, so jumps feel snappy.
- **Ladders** are trigger tiles on the `Ladder` layer. Pressing ↑ near one (or ↓ above one) switches the hero
  into a climbing state with gravity turned off.
- **One-way ledges** use Unity's `PlatformEffector2D`. To drop through, the controller briefly ignores
  collisions between the Player and OneWay layers.
- **Shooting** spawns a `Projectile` prefab (forward with X, straight up with C). A short line check makes sure it
  never spawns inside a wall. There is a 0.3 s cooldown.

## 7. Enemy types and behaviour

| Enemy | Script(s) | Behaviour |
|---|---|---|
| Spikes | `ContactDamage` | Don't move, not animated; hurt on touch. |
| Slime | `EnemyHealth`, `SpriteFrameAnimator` | Stays in place, animated wobble. |
| Spider | `HangingThread`, `Bobbing` | Hangs from the ceiling on a thread and bobs up and down. |
| Skeleton | `PatrolEnemy` | Walks back and forth; raycasts detect walls and ledge edges so it turns around. |
| Ghost | `ShooterEnemy` | Hovers; when the hero is close and visible, it glows (warning), then fires an aimed bolt. |
| Bat | `FlyingEnemy` | Flies in a wave pattern. |

Every enemy has one trigger "hurtbox" and an `EnemyHealth` component. Player projectiles call
`IDamageable.TakeDamage`; touching an enemy damages the player through `ContactDamage`.

## 8. The boss: the Keeper

`Scripts/Runtime/Boss/DragonBoss.cs` (with `BossArena` for the trigger and door). 30 HP, three attacks chosen at
random (never the same twice in a row), each with a clear warning first:

1. **Fire Volley:** flies to the side and fires a fan of fireballs.
2. **Dive Slam:** a red marker appears under the hero, then the dragon slams down onto it.
3. **Fire Rain:** several floor markers appear, then fire falls on them (one gap is always safe).

Below half health it becomes **enraged** (faster, more projectiles, shockwaves). The fight is one long
**coroutine** (`FightLoop`), which keeps the boss logic readable from top to bottom. If the hero dies, the fight
resets completely. When the boss is defeated it speaks, sinks to the floor and stays there for the ending.

## 9. Collectibles and memory shards

All pickups inherit from the abstract class `Pickup`, which detects the player and makes sure an item is
collected only once. Subclasses only implement `OnCollected`: `Gem`, `HeartPickup`, `KeyPickup` and
`MemoryShard`. `GameSession` keeps the totals (gems, keys, deaths, time) and survives scene changes
(`DontDestroyOnLoad`).

**Memory shards** are optional glowing gems: each one freezes the game for a short remembered line. Each line
is a piece of the flashback at the end, so players who collected them recognise it.

## 10. Dialogue and narrative progression

- All text is in one file: `Story/StoryScript.cs`. Changing a line never requires touching scenes.
- `DialogueBox` shows lines with a typewriter effect. A *blocking* dialogue pauses the world
  (`Time.timeScale = 0`); a *bark* is a short thought that doesn't stop play.
- Story moments in levels are **digits 1–9 in the level layouts**. The builder turns each into a `StoryPoint`
  (an automatic trigger or an "Enter: Examine" object) or a `MemoryShard`.
- `Story.cs` remembers which beats already happened in the current cycle (`TryBegin`), so each one plays once
  and in the right order (for example, the castle door's lines need the ghost first).

## 11. The cyclical ending

1. The chest opens, and a small figure with a lantern appears in its light.
2. **Flashback** (`MemoryFlashback.cs`): four short scenes staged with two simple actors (`MemoryActor`), camera
   cuts, and light changes. A small set (a cottage, a forest path, a castle on the horizon) is built to the right
   of the boss level, outside the playable area:
   1. Leaving home at night: the promise.
   2. The last encounter: Lia follows, the hero sends her home.
   3. Returning at dawn: the house is empty.
   4. The bargain with the Keeper, in the same hall.
3. Back in the present, the hero remembers, and **chooses to forget again** ("Take it back").
4. Gems scatter, the screen fades, one more notch is carved. The forest scene loads and the opening plays again.
   The notch count is stored with `PlayerPrefs`, so it survives quitting the game.

## 12. Scene management

- `SceneLoader` (a persistent object) fades to black, loads the scene by **name**, and fades back in.
  Levels never use build indexes, so reordering the build list can't break the flow.
- Each level scene has a `LevelManager`: it starts the music and title card, handles checkpoints, death,
  retry and level completion, and knows the name of the next scene.
- Build order: `MainMenu`, `Level1_Forest`, `Level2_Castle`, `Level3_TreasureChamber`.

## 13. Unity project folder structure

```
Assets/
  _Game/                 everything made for this game
    Animations/          hero animation clips + Animator controller
    Art/                 art by pack (Player, Enemies, Environment, Items, Portals, Boss, FX),
                         Generated/ (pixel art drawn by code for this project), Licenses/
    Audio/               Music/, SFX/
    Levels/              the three ASCII level layouts (.txt)
    Physics/             physics material (no friction)
    Prefabs/             player, enemies, pickups, chests, gates, boss, UI canvas (generated)
    Resources/           SfxLibrary.asset (which sound plays for which event)
    Scenes/              the four scenes (generated)
    Scripts/Runtime/     game code, one folder per topic (see §14)
    Scripts/Editor/      the builder that generates prefabs and scenes
    Tests/PlayMode/      automated tests
    Tiles/               tile assets (generated)
    UI/                  title text material
  Fonts/, Settings/, TextMesh Pro/      engine and third-party support files
Docs/                    documentation and screenshots
Tools/LevelTools/        Python scripts to create and check level layouts
```

## 14. Important scripts and their responsibilities

All runtime code is in the `LastMemory` namespace, in `Assets/_Game/Scripts/Runtime/`.

| Folder | Scripts | Responsibility |
|---|---|---|
| Core | `GameInput` | All controls, built in code with the Input System; tests can simulate input. |
| | `GameSession` | Gems, keys, deaths and play time across scenes. |
| | `SceneLoader` | Scene changes with fades. |
| | `CameraController` | Smooth follow, level bounds, screen shake, pixel snapping, cutscene shots. |
| | `Layers` | Physics layer numbers and the collision matrix. |
| | `GameInfo` | The game's title (menu, build settings, executable name). |
| Player | `PlayerController`, `PlayerHealth` | Movement, jumping, ladders, shooting; hearts, damage, death, respawn. |
| Enemies | `EnemyHealth`, `PatrolEnemy`, `ShooterEnemy`, `FlyingEnemy`, `HangingThread` | Enemy health and the four movement/attack behaviours. |
| Combat | `Projectile`, `ContactDamage`, `Damage` | Bullets, touch damage, the `IDamageable` interface. |
| Items | `Pickup` (+ `Gem`, `HeartPickup`, `KeyPickup`), `TreasureChest`, `LockedGate` | Collectibles, chests, gates opened by keys. |
| Level | `LevelManager`, `Checkpoint`, `LevelExit`, `KillZone` | Per-level flow, respawn points, portals, falling out of the level. |
| Boss | `DragonBoss`, `BossArena`, `WarningPulse` | The Keeper's fight and its warning markers. |
| Story | `StoryScript`, `Story`, `DialogueBox`, `StoryPoint`, `MemoryShard`, `TallyMarks`, `Glint`, `StoryDirector`, `MemoryFlashback`, `MemoryActor` | Text, story state, dialogue, story triggers, the opening and ending cutscenes, the flashback. |
| UI | `HUD`, `GameMenus`, `MainMenuController`, `CinematicOverlay`, `VolumeSlider`, `UIButtonSound` | HUD, pause/death/ending panels, main menu, letterbox and fades. |
| Audio | `AudioManager`, `SfxLibrary` | Music cross-fades and sound effects by name. |
| Effects | `Fx`, `SpriteFrameAnimator`, `Bobbing`, `LightFlicker`, `ParallaxLayer` | Particles, simple sprite animation, floating, torch flicker, parallax. |

Editor scripts (`Assets/_Game/Scripts/Editor/`, namespace `LastMemory.EditorTools`):
`ArtSetup` (texture import settings and slicing), `GeneratedArt` (draws the project's own pixel art),
`ContentBuilder` (prefabs, sound library, project settings), `UIBuilder` (canvases),
`LevelBuilder` (scenes from layouts), `MemoryStageBuilder` (the flashback set),
`GameBuildPipeline` (menu commands: rebuild, validate, screenshots, Windows build).

## Understanding the C# Code

This section explains how the main gameplay systems work in the real code. Two Unity ideas come up
everywhere:

- **Unity calls methods for you** at fixed moments: `Awake` (object created), `Start` (before its first frame),
  `Update` (every frame), `FixedUpdate` (every physics step, 50 times a second), `LateUpdate` (after all
  `Update`s), and `OnTriggerEnter2D` / `OnTriggerStay2D` (a trigger collider touches another collider).
- **Coroutines** (`IEnumerator` methods with `yield return`) are used for anything that happens over time: fades,
  boss attacks, cutscenes. `yield return null` waits one frame; `yield return new WaitForSeconds(0.5f)` waits
  half a second. They are started with `StartCoroutine(...)`.

### A complete example: the player presses X

1. **Input.** `GameInput` (a static class) creates an Input System `InputAction` called "Shoot", bound to
   `<Keyboard>/x` and the gamepad's west button. Nothing is set up in the Inspector; it is all in code.
2. **Reading it.** Every frame, `PlayerController.Update` calls `ReadActions()`, which checks
   `GameInput.ShootPressed` (true only on the frame the key goes down, via `WasPressedThisFrame()`).
   If the Up arrow is held at that moment, the shot aims up (C does the same through `GameInput.ShootUpPressed`).
3. **Creating the projectile.** `TryShoot(aimUp)` checks the 0.3 s cooldown, picks a direction and a spawn point,
   and uses `Physics2D.Linecast` to make sure the spawn point isn't inside a wall. Then
   `Instantiate(projectilePrefab, ...)` copies the `PlayerProjectile` prefab and `projectile.Launch(direction)`
   sets its `Rigidbody2D.linearVelocity`. `Launch` also calls `Destroy(gameObject, lifetime)`, so a shot that hits
   nothing disappears after its lifetime (0.9 s for the hero's slash, about 14 tiles).
4. **Collision.** The projectile has a trigger `CircleCollider2D` and sits on the *PlayerProjectile* physics layer.
   The collision matrix (`Layers.CollidingPairs`) lets that layer touch only *Ground* and *Enemy*, so shots pass
   through pickups and portals. When it touches something, Unity calls `Projectile.OnTriggerEnter2D(other)`.
5. **Damage.** If `other` is a wall, the projectile just bursts. Otherwise it looks for an `IDamageable` on the
   object (`other.GetComponentInParent<IDamageable>()`) and calls `TakeDamage(new DamageInfo(damage, position))`.
   The projectile doesn't know whether it hit a slime or the dragon; it only knows the interface.
6. **The enemy reacts.** `EnemyHealth.TakeDamage` subtracts health. If some is left, it plays a sound and starts a
   short red-flash coroutine; if not, `Die()` spawns particles, raises the `Died` event and destroys the enemy.
   `TakeDamage` returns `true`, so the projectile destroys itself too (`Impact()`).

### Input: `GameInput` (Core)

- **Does:** defines every control in code and offers simple read-only properties: `Move`, `JumpPressed`,
  `JumpHeld`, `UpJumpPressed`, `ShootPressed`, `ShootUpPressed`, `InteractPressed`, `AdvancePressed`, `PausePressed`.
- **Unity:** Input System `InputAction`s with keyboard and gamepad bindings.
- **Talks to others:** other scripts read its static properties. Tests fill in `GameInput.Simulated` instead of
  pressing real keys, which is how the automated tests "play" the game.
- **C# concepts:** `static class`, properties with `get`, a `struct` for the simulated input.

### Player movement, jumping and ladders: `PlayerController` (Player)

- **Unity:** `Rigidbody2D` (moved by setting its velocity), `CapsuleCollider2D`, an `Animator` on the child
  `Visual` object, `Physics2D.OverlapBox` / `OverlapPoint` for ground and ladder checks.
- **Main methods:**
  - `Update`: reads input (`ReadActions`), handles ladders (`UpdateLadder`), and updates the sprite (`UpdateVisual`).
  - `FixedUpdate`: the physics. It checks for ground (`CheckGround`), runs with acceleration
    (`Mathf.MoveTowards` on the velocity), starts jumps, applies stronger gravity when falling, and moves the hero
    up and down while climbing.
  - `ReadActions`: decides what the Up arrow means (climb on a ladder, otherwise jump after a 0.09 s wait so
    Up + X can aim instead).
  - `UpdateLadder`, `StartClimbing`, `StopClimbing`: grabbing a ladder turns gravity off and snaps the hero to the
    ladder's centre; leaving it turns gravity back on.
- **Why input and physics are split:** key presses are only reliable in `Update` (every frame), while velocity
  should be changed in `FixedUpdate` (every physics step). `ReadActions` stores *when* a jump was pressed
  (`lastJumpPressedTime`), and `FixedUpdate` uses it. That stored time is also what makes the jump buffer and
  coyote time work.
- **Talks to others:** `PlayerHealth` calls `Knockback` and `SetControl`; cutscenes use `SetControl(false)`,
  `SetFacing` and `ScriptedMoveX`. Triggers recognise the hero with `PlayerController.FromCollider(other)`.

### Health, damage and "lives": `PlayerHealth`, `IDamageable`, `ContactDamage`, `KillZone`

- The hero has **3 hearts** instead of lives. Losing all of them shows the death panel; *Retry* restores the hearts
  at the last checkpoint (`LevelManager.Retry` → `PlayerHealth.Revive`).
- `IDamageable` (in `Damage.cs`) is an **interface** with one method, `bool TakeDamage(DamageInfo info)`.
  `PlayerHealth`, `EnemyHealth` and `DragonBoss` all implement it. `DamageInfo` is a small `struct` (amount and
  where the hit came from, used for the knockback direction).
- `ContactDamage` is placed on spikes, enemy bodies and the boss. In `OnTriggerStay2D` it hurts the player.
  `PlayerHealth` ignores hits during the 1.2 s of invulnerability after a hit, so touching spikes costs exactly one
  heart.
- `KillZone` is a wide trigger under each level. Falling into it calls `PlayerHealth.FellIntoPit()`.
- **Events:** `PlayerHealth` raises `HeartsChanged`; the `HUD` subscribes to it (`+=`) and redraws the hearts.
  Neither script needs to know how the other works.

### Shooting and projectiles: `Projectile` (Combat)

- One script for every projectile: the hero's slash, the ghost's bolt, the boss's fireballs and shockwaves. Prefabs
  differ only in their settings (speed, damage, sprite) and **physics layer**, which decides what they can hit.
- `Launch(direction)` sets the velocity and lifetime; `OnTriggerEnter2D` handles walls and `IDamageable` targets.

### Enemies (Enemies)

| Script | Unity parts | How it works |
|---|---|---|
| `EnemyHealth` | `SpriteRenderer` | Implements `IDamageable`: health, hit flash (coroutine), death (particles, `Died` event, `Destroy`). |
| `PatrolEnemy` (skeleton) | kinematic `Rigidbody2D`, `Physics2D.Raycast` | In `FixedUpdate`: a raycast forward finds walls, a raycast down in front of the feet finds the floor; it turns around at either, then moves with `MovePosition`. |
| `ShooterEnemy` (ghost) | `SpriteRenderer`, `Physics2D.Linecast` | In `Update`: faces the hero, counts down a timer, and fires only when the hero is in range and no wall is in between (`Linecast`). `Fire()` is a coroutine: 0.6 s of flashing (the warning), then a `Projectile` aimed at the hero. |
| `FlyingEnemy` (bat) | `Transform` only | Moves along a formula: `Mathf.PingPong` for left–right, `Mathf.Sin` for the wave. |
| `HangingThread` + `Bobbing` (spider) | `SpriteRenderer` | `Bobbing` moves the spider up and down; `HangingThread` stretches a 1-pixel sprite from the ceiling to it. |

The slime has no special script: it is `EnemyHealth` + `ContactDamage` + an animation (`SpriteFrameAnimator`).

### Collectibles and keys: `Pickup`, `Gem`, `HeartPickup`, `KeyPickup`, `LockedGate`, `GameSession`

- `Pickup` is an **abstract base class**. It handles what every pickup has in common: detecting the hero in
  `OnTriggerEnter2D`, collecting only once, the sound and sparkle, `Destroy`, and the "fly to the player" movement
  of gems that come out of chests. Each subclass only says what collecting does by **overriding**
  `OnCollected(player)`:
  - `Gem`: `GameSession.Instance.AddGems(value)`.
  - `HeartPickup`: also overrides `CanCollect` so it stays in the world when your hearts are full.
  - `KeyPickup`: `GameSession.Instance.AddKey(key)` and a HUD message.
- `GameSession` stores gems, keys, deaths and time. It is a **singleton** created on first use and kept alive across
  scenes with `DontDestroyOnLoad`. It raises a `Changed` event that the `HUD` listens to.
- `LockedGate`: in `OnTriggerStay2D`, if `GameSession.Instance.HasKey(requiredKey)`, it uses the key and runs the
  `Open()` coroutine (disables the solid collider, slides and fades the bars); otherwise it shows "Locked".

### The boss: `DragonBoss` and `BossArena` (Boss)

- `BossArena` is a trigger at the hall entrance. `OnTriggerEnter2D` closes the gate behind the hero and calls
  `boss.StartFight()`. It also subscribes to `LevelManager.PlayerRespawned`, so dying resets the fight.
- `DragonBoss.StartFight` starts one long coroutine, `FightLoop`: fly in, speak (the blocking dialogue pauses the
  game), then repeat *pick a random attack (never the same one twice) → run it → hover* until the boss dies.
  Each attack (`FireVolley`, `DiveSlam`, `FireRain`) is its own coroutine, so you can read an attack from top to
  bottom like a script: move, warn (`Flash`, red markers), attack, wait.
- `TakeDamage` (from `IDamageable`) lowers health, updates the HUD bar, switches to the *enraged* mode below half
  health, and starts `Die()` at zero.
- **Unity:** kinematic `Rigidbody2D`, one trigger hurtbox, `ContactDamage`, `SpriteRenderer.flipX` for facing,
  `Instantiate` for fireballs.

### Scene transitions: `SceneLoader`, `LevelManager`, `LevelExit` (Core, Level)

- `SceneLoader.Load("Level2_Castle")` fades a black overlay in, loads the scene with
  `SceneManager.LoadSceneAsync`, and fades out. It lives on a `DontDestroyOnLoad` object, so the fade survives the
  scene change, and it ignores a second request while loading.
- Each level has one `LevelManager` (`LevelManager.Instance`). It starts the music and title card, keeps the active
  checkpoint, shows the death panel, handles *Retry*, and knows the name of the next scene
  (`CompleteLevel()` → `SceneLoader.Load(nextScene)`).
- `LevelExit` (the portal) opens its animation when the hero comes near, and on `OnTriggerEnter2D` shrinks the hero
  into the portal and calls `LevelManager.Instance.CompleteLevel()`.

### UI and menus: `HUD`, `GameMenus`, `MainMenuController`, `CinematicOverlay` (UI)

- **Unity:** `Canvas` + `CanvasScaler` (scales with the screen), `Image`, TextMesh Pro `TMP_Text`, `CanvasGroup`
  (fading whole panels with `alpha`), `Button.onClick`, and the `EventSystem` for keyboard navigation.
- `HUD` subscribes to `PlayerHealth.HeartsChanged` and `GameSession.Changed` and redraws only when something
  changes. Other scripts call its static methods, e.g. `HUD.ShowMessage("Checkpoint")`.
- `GameMenus` (pause, death, ending panels) and `MainMenuController` connect buttons in `Awake` with
  `button.onClick.AddListener(...)`. Pausing sets `Time.timeScale = 0`, which stops physics and every
  `Time.deltaTime`-based movement.

### Dialogue and story triggers (Story)

- `StoryScript` holds every line as `DialogueLine` arrays, plus a table that maps the digits in the level files to
  story beats. Changing text never requires touching a scene.
- `StoryPoint` sits on a trigger in the level. In `OnTriggerStay2D` it asks `Story.TryBegin(beatId, requires)`,
  which returns true only the first time this cycle (and only if the required earlier beat has happened). Then it
  calls `DialogueBox.Play(lines, mode)`. Readable objects (the lantern) instead check `GameInput.InteractPressed`
  in `Update` while the hero stands near them.
- `DialogueBox` shows one request at a time and queues the rest (`Enqueue`). Its `ShowLine` coroutine types the
  text letter by letter in real time (`Time.unscaledDeltaTime`), so it still works while a blocking dialogue has
  frozen the world with `Time.timeScale = 0`. When a request ends, its `onDone` callback (an `Action`) runs, which
  is how cutscenes wait for a conversation (`yield return DialogueBox.Run(...)`).
- `StoryDirector` and `MemoryFlashback` are the cutscenes: long coroutines that move the hero, the actors and the
  camera, fade the screen and play dialogue, one step after another.

### Parts that look complicated on purpose

- **The jump timing in `PlayerController`** (buffer, coyote time, the Up-arrow wait) is what makes the controls feel
  fair. It's a few timestamps compared in `FixedUpdate`, and the comments name each one.
- **The editor builder** (`Scripts/Editor`) is large because it creates every scene, prefab and import setting from
  code. That's what keeps the levels in sync with the `.txt` layouts. You don't need it to explain the gameplay.
- **`ArtSetup.ExtrudeTiles`** copies each tile with a 1-pixel border so no thin lines appear between tiles when the
  camera is between two pixel positions.
- **`Fx`** reuses a pool of particle objects instead of creating new ones for every effect, to avoid stutters.

## 15. Physics and collisions

- Rigidbody2D physics for the hero, enemies and projectiles. Level geometry uses `TilemapCollider2D` merged into
  a `CompositeCollider2D` (one smooth collider instead of hundreds of boxes, so the hero never snags on tile
  edges). Wooden ledges add a `PlatformEffector2D` (one-way).
- **Layers** decide who collides with whom (the `CollidingPairs` table in `Layers.cs`; the matrix is set at startup). For example, player
  projectiles only hit Ground and Enemy, so they can't trigger the exit portal or collect items.
- Objects are recognised **by component** (`GetComponent<PlayerController>()`), not by tag strings.
- Ground detection uses a small `OverlapBox` under the feet; skeleton patrols use raycasts.

## 16. Animation system

- **Hero:** an Animator Controller with eight states (Idle, Run, Jump, Apex, Fall, Climb, Attack, Hurt). The
  controller picks the state from the hero's velocity and actions in code and calls `animator.Play` when it
  changes. This is easier to follow than a web of transitions.
- **Enemies, portals, pickups, the flashback characters:** `SpriteFrameAnimator` / `MemoryActor` simply cycle
  through an array of sprites at a set frame rate.
- **Procedural animation:** squash and stretch, bobbing, flashing, the death spin and fades are done in code.

## 17. UI and HUD

- Unity UI with TextMesh Pro, on a Canvas that scales with the screen size (reference 1920×1080), so it works at
  any resolution.
- HUD: hearts, gem counter, held keys, level title cards, short messages, the examine prompt, the boss health bar.
- Menus: main menu (Play, Settings, Credits, Quit), pause, death and ending panels. All usable with the keyboard.
- `CinematicOverlay`: letterbox bars, full-screen fades and the final caption.

## 18. Sound effects and music

- `AudioManager` is a persistent object with one music source (with cross-fading) and a pool for sound effects.
- Sound effects are requested by name (`AudioManager.Play(Sfx.Jump)`); `SfxLibrary.asset` maps each name to an
  audio file and volume, so sounds can be swapped without changing code.
- Three music tracks (menu/forest, castle, boss). Music and SFX volume are saved in `PlayerPrefs`.

## 19. How the ASCII level generator works

**Where:** `Assets/_Game/Levels/Level1_Forest.txt`, `Level2_Castle.txt`, `Level3_TreasureChamber.txt`.
The first line is a header (theme and back-wall rule); every following character is one 16×16 tile.

**Symbols (most used):** `#` ground, `=` wooden ledge, `H` ladder, `^` spikes, `P` player start, `C` checkpoint,
`g` gem, `h` heart, `k`/`K` keys, `L`/`D` gate/door, `c`/`$` chests, `E` exit portal, `s m o b p` enemies,
`B` boss, `T` final chest, `1`–`9` story points. The full table is in `GAME_DESIGN.md` §9.

**What the builder does** (`LevelBuilder.Build`):
1. Reads the text file into a 2D array.
2. Paints the tilemaps, choosing the right tile for each `#` from its neighbours (auto-tiling: grass on top,
   edges at the sides).
3. Places a prefab for every object symbol, and turns digits into story points.
4. Adds the player, camera, lights, parallax background, managers and UI, and saves the scene.

**Rebuild the scenes:** in Unity, **Tools → The Last Memory → Rebuild Game**. Or, with the Editor closed:

    Unity.exe -batchmode -projectPath "<project folder>" -executeMethod LastMemory.EditorTools.GameBuildPipeline.BuildAllBatch

Then **Tools → The Last Memory → Validate Scenes** (the rebuild also validates).

**Edit or extend a level:** change the `.txt` file (keep all rows the same width), run
`python Tools/LevelTools/reach.py Assets/_Game/Levels` to check everything can still be reached, then rebuild.
To add a new symbol, add a `case` to the `switch` in `LevelBuilder.Build`.

**Generated scenes:** all four (`MainMenu`, `Level1_Forest`, `Level2_Castle`, `Level3_TreasureChamber`).
**Don't edit by hand** (the rebuild overwrites them): the four scenes, `Prefabs/*.prefab`, `Tiles/*.asset`,
`Animations/Hero.controller`, `Resources/SfxLibrary.asset`, `Art/Generated/*.png` and the `*_Extruded.png`
tile sheets. Change the layouts or the builder code instead.

## 20. How to run and build

- **Open:** Unity Hub → Add → this folder → open with Unity **6000.2.13f1**.
- **Play in the Editor:** open `Assets/_Game/Scenes/MainMenu.unity` and press Play.
- **Tests:** Window → General → Test Runner → PlayMode → Run All.
- **Build:** Tools → The Last Memory → Build Windows Player. The result is
  `Builds/TheLastMemory_Windows/The Last Memory.exe`.

## 21. Presenting the project (about 10 minutes)

1. **Pitch (30 s):** "A platformer about a hero who keeps choosing to forget." Show the main menu.
2. **Play Level 1 (2–3 min):** run, jump, climb the ruins ladder, shoot the slime (X) and a ghost, shoot upward (C),
   show the note and the notches on the tree, examine the lantern (Enter), pick up a memory shard.
3. **Level 2 (1 min):** show the castle atmosphere, the Faint Ghost and "I was so close." by the treasure door.
   (Tip: open the scene directly to save time; story points still work.)
4. **Boss (2 min):** open `Level3_TreasureChamber`, fight the Keeper and point out the warnings for each attack.
5. **Ending (2–3 min):** let the flashback play; mention it is staged with two small actors, camera shots and
   lights, not a video. Point out the notch count increasing.
6. **Code (2 min):** show `GameInput` (input in code), `PlayerController.ReadActions` (jump/climb/shoot rules),
   `StoryScript` (all text in one place) and a level `.txt` file next to its scene.
7. **Quality:** run a few Play Mode tests; mention the scene validator and the seam fix (`ArtSetup.ExtrudeTiles`).

**Assignment checklist to show:** see the table below.

## Assignment requirements: where each one is implemented

| Requirement | Where in the game | Where in the code |
|---|---|---|
| 2D platformer | All levels | `PlayerController`, Tilemaps, `LevelBuilder` |
| Animated player | Idle, run, jump, apex, fall, climb, attack, hurt | `Animations/Hero.controller`, `PlayerController.UpdateVisual` |
| Running | ← / → | `PlayerController.FixedUpdate` |
| Jumping | ↑ / Z / Space | `PlayerController.ReadActions`, `FixedUpdate` |
| Ladder climbing | Forest ruins and shaft, castle towers | `PlayerController.UpdateLadder`, `H`/`I` tiles |
| Shooting | X forward, C upward | `PlayerController.TryShoot`, `Projectile` |
| Stationary, non-animated enemy | Spikes | `Hazard_Spikes` prefab, `ContactDamage` |
| Stationary, animated enemy | Slime (and the hanging spider) | `Enemy_Slime`, `Enemy_Spider`, `SpriteFrameAnimator`, `HangingThread` |
| Moving, animated enemy | Skeleton (and the bat) | `PatrolEnemy`, `FlyingEnemy` |
| Shooting enemy | Ghost | `ShooterEnemy` |
| Background music | Three tracks, cross-faded | `AudioManager.PlayMusic` |
| Sound effects | 24 effects | `SfxLibrary.asset`, `AudioManager.Play` |
| Main menu | Play / Settings / Credits / Quit | `MainMenuController`, `UIBuilder.BuildMainMenuCanvas` |
| At least two levels | Three: forest, castle, treasure chamber | `Levels/*.txt`, `Scenes/` |
| Boss encounter | The Keeper | `DragonBoss`, `BossArena` |

All of these are also covered by the automated Play Mode tests (`GameplayTests`, `ControlsAndLadderTests`,
`StoryTests`).
