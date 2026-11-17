# Development Progress: *The Last Memory*

**Branch:** `modernization`. **Recovery point:** tag `pre-modernization` on `1921e3d`, the original course project. GitHub: `avgolberg/tilevania` (iterations 1–3 were pushed; iteration 4 is committed locally and **not pushed**). **Unity:** 6000.2.13f1, URP 2D, Input System.

## Iteration 5: code readability and one dependency removed

No gameplay, story, visuals, controls, levels, balance, scenes or prefabs changed.

| Area | What changed |
|---|---|
| **Hot Reload removed** | `com.singularitygroup.hotreload` (Git-hosted, a development tool) removed from `Packages/manifest.json` and `packages-lock.json`. No script, asset, setting or define symbol referenced it. Its two dependencies (`com.unity.ugui`, `com.unity.modules.unitywebrequest`) stay because the game lists them itself. |
| **Readability** | Small, behaviour-preserving edits in `PlayerController`, `ShooterEnemy`, `DragonBoss`, `DialogueBox`, `Layers`, `PlayerHealth`, `PatrolEnemy`, plus a named helper `PlayerController.FromCollider` used by seven trigger scripts. See below. |
| **Guide** | New section **Understanding the C# Code** in `STUDENT_PROJECT_GUIDE.md`, with a step-by-step walkthrough of a shot from key press to enemy damage. |

Readability changes, in detail:
- `PlayerController`: nested `?:` expressions replaced by plain `if`/`else` (acceleration rate, ladder grab, animation state, climb-animation pause); shooting picks direction/origin/spawn in one `if (aimUp)` block instead of three ternaries; a redundant variable removed; short comments on jump buffering and coyote time.
- `PlayerController.FromCollider(other)` replaces `other.attachedRigidbody != null ? other.attachedRigidbody.GetComponent<PlayerController>() : null` in `Pickup`, `TreasureChest`, `LockedGate`, `Checkpoint`, `LevelExit`, `BossArena` and `StoryPoint`.
- `ShooterEnemy`: the muzzle and aim-point formulas, written twice, are now the properties `Muzzle` and `AimPoint`; the range/line-of-sight checks have names.
- `DragonBoss`: the "which wall to fly to" ternary is an `if`; `FacePlayer` reuses `FaceTowards` instead of repeating it.
- `DialogueBox.Enqueue`: nested branches flattened into four commented cases.
- `Layers.ShouldCollide`: a long chain of `||` calls became a readable table, `CollidingPairs`, checked in a loop.

## Iteration 4: final polish for submission

| Area | What changed |
|---|---|
| **Name** | The game is now **The Last Memory** everywhere players see it: window title / product name, main menu, credits, boss bar, ending panel, build folder and executable (`Builds/TheLastMemory_Windows/The Last Memory.exe`), docs and a new `README.md`. The repository URL and local folder keep the old name. Note: changing the product name moves PlayerPrefs, so a build starts with fresh settings and the notch count back at 7. |
| **Ending** | The chest now opens into a **flashback** (`MemoryFlashback`, `MemoryActor`): four short staged memories (leaving home, the last encounter with Lia, the empty house at dawn, the bargain with the Keeper), using a small cottage-and-forest set that the level builder places to the right of the Treasure Chamber, plus new project-made pixel art (Lia, the cottage, a castle silhouette, a sack of gold). Back in the present, the Keeper offers to let the hero keep the memory and the hero **chooses to forget** ("Take it back"). With all four shards, the hero hesitates first. See `STORY_SPEC.md`. |
| **Keeper** | After its defeat the Keeper sinks to the floor and stays in the hall (wounded, harmless) so it can take part in the ending. Its fight is unchanged. |
| **Dialogue** | Every line reviewed and rewritten where needed: the Keeper's lines explain the promise in plain terms, the ghost hints at the bargain, the shards match scenes in the flashback, the old "Your name is... Li—" device is replaced by the hero finally saying her name. |
| **Shooting up** | New dedicated key **C** (gamepad right shoulder): always shoots straight up, never jumps. Up + X still works. Examine moved from C to **Enter** so C has one meaning. |
| **Tile seams** | Root cause found and fixed (below). |
| **Structure** | Removed `Assets/_Legacy` (unused original course content, including course-restricted art) and the Cinemachine package it needed. Namespace and assemblies renamed `TileVania` → `LastMemory`; menu **Tools → The Last Memory**. `Runtime/Audio` and `Runtime/Effects` split out of `Core`. The "victory" panel is now called the ending panel in code. Python bytecode no longer tracked. The pre-modernization audit moved to `Docs/History/`. |
| **Docs** | New `STUDENT_PROJECT_GUIDE.md` and `README.md`; `STORY_SPEC.md`, `GAME_DESIGN.md`, `QA_CHECKLIST.md`, `ASSET_CREDITS.md` and the level tools README updated. |

**Tile seam investigation.** A temporary editor probe rendered the forest and castle ground tilemaps unlit over a magenta background at 1920×1080, 1600×900, 1366×768, 1280×720, 960×540 and 2560×1440, at 40 random camera positions each, both with and without the camera's pixel snapping. It counted pixels where the background showed through inside fully surrounded tiles, and pixels whose colour didn't belong to that tile's sprite.
- *Before:* with pixel snapping, 0 problems. Without it, full-width one-pixel seam lines in 7 frames, all in the forest (1366×768, 1600×900, 960×540).
- *Cause:* the seam row was exactly where a tile edge fell on a screen-pixel centre. That pixel samples the texel just outside the tile's rectangle, i.e. the neighbouring cell of the sprite sheet, which in the forest sheet is often transparent. The camera's pixel snapping normally keeps edges off pixel centres, which is why the seams were only occasional.
- *Ruled out:* MSAA (off), texture filtering/compression (point, uncompressed), PPU (consistent 16), the sprite mesh type (switching to Full Rect changed nothing, so that change was reverted), and geometry gaps.
- *Fix:* the importer copies each tilemap tile into its own slot with a 1-pixel border of its own edge pixels (`ArtSetup.ExtrudeTiles` → `Forest_Tiles_Extruded.png`, `Castle_Tiles_Extruded.png`); tilemaps use those. Geometry and colliders are unchanged.
- *After:* the same 960 camera positions show 0 gap and 0 bleeding pixels at every resolution, snapped or not. The probe was temporary and is not in the project.
- *Uncertainty:* I could not observe your exact playthrough setup, so I can't prove this was the exact seam you saw. It is the only seam mechanism the probe found, and it no longer occurs; please confirm during manual QA.

**Validation (iteration 4)**, run in batch mode in this project (the Editor was closed; the uncommitted `Symtext SDF.asset` change was restored byte-for-byte after every run):

| Check | Result |
|---|---|
| Full regeneration (`BuildAllBatch`) | ✅ 0 compile errors; validator **0 problems** (now also checks the flashback wiring) |
| Layout reachability (`reach.py`) | ✅ ALL OK (layouts unchanged; `levels.py` output matches the `.txt` files) |
| **Play Mode tests** | ✅ **60 / 60 pass** (plus 1 opt-in screenshot test, skipped in normal runs). New: flashback order and the hero's choice, one key press advances at most one line, HUD hidden in the flashback, the Keeper stays after defeat, notch count saved in PlayerPrefs, C shoots up standing / running / in the air without jumping, C no longer examines |
| Flashback screenshots | ✅ `FlashbackScreenshots` played the real ending at normal speed and saved `Docs/Screenshots/Flashback_*.png` (17 images, reviewed) |
| Windows player | ✅ Build succeeded, 109 MB, 0 errors; a short batch-mode launch of the `.exe` logged no errors |

**Not performed:** human play-testing of the new ending's pacing and emotional clarity, the feel of the C key, and the seam fix on your display. See `QA_CHECKLIST.md`.

## Iteration 3: story, *The Last Memory*

The full narrative is in **`STORY_SPEC.md`**. The hero wakes in the forest with no memory, pursues the memory the Keeper (the dragon) guards, almost recovers it at the chest, loses it, and wakes in the forest again with one more notch on the tree. How it fits into the game is in `GAME_DESIGN.md` §5 (story points), §6 (boss) and §7a (cutscenes).

| Area | What was added |
|---|---|
| Dialogue | `DialogueBox`: speaker plate, typewriter text, keyboard-only advancing (Z / Space / X / C / Enter; first press completes the line). **Blocking** freezes the world (time scale 0) and takes control. **Cinematic** is for cutscenes. **Bark** is a thought that never takes control. Control returns 0.12 s after the last line, so the closing key doesn't also jump or shoot. Esc is ignored during dialogue and cutscenes. |
| Story state | `Story`: per-cycle flags, and `TryBegin(beat, requires)` so every beat fires once per cycle and never out of order (for example, the door scrawl needs the ghost, and the dragon's defeat lines need its intro). The completed-cycle count is saved in PlayerPrefs, and the tree shows 7 + that count. |
| Story points in the layouts | Digits 1–9 in the ASCII layouts (`StoryScript.BeatForDigit`). Forest: the notched tree with its pinned note (automatic), the rusted lantern (C to examine) and 2 memory shards. Castle: the déjà vu thought, the Faint Ghost, "I was so close." plus a tally by the treasure door, and 2 shards. The level builder creates the scenery; `reach.py` checks that every marker can be reached. |
| Cutscenes | `StoryDirector`, using existing objects (no timeline). It runs the forest opening (wake-up), the ending at the chest (silhouette with a lantern, the name cut off, gems scattering, white-out, the knock) and the loop (identical opening, the newest notch glows, "What chest?", the final caption, then the ending panel with **Begin Again** / **Main Menu**). `CinematicOverlay` provides the letterbox, fades and caption. |
| Boss | Mechanics unchanged. It is renamed **The Keeper**, speaks before the fight (frozen) and after its defeat, and says only "Again, then." on retries. |
| New art (generated, project-made) | `OldTree.png`, `Note.png`, `Lantern_Lit.png` and `Lantern_Rusted.png` in `Art/Generated`, plus a `MemoryShard` prefab (a tinted gem with a glow). The ghost, gems, hero silhouette, sounds and music are reused. |
| UI | An examine prompt, the dialogue box, cinematic overlays, the ending panel (replacing "Victory!"), the menu tagline, new hints and credits. The level subtitles are now the chapter names. |

**Fixed during validation:**
- The ending's gem-drain animation could push the counter below 0 when the new cycle started early. It is now clamped and stopped before the reset.
- Memory shard 4 touched the castle's east wall, so it moved one tile left.
- The tree and the door scrawl are backdrop scenery, so they live in a `StoryScenery` group that the sorting test exempts, like `Decor`.
- In the story tests, a stuck blocking dialogue could hang the whole run. The story tests now have a 120 s timeout.

**Validation (iteration 3)**, performed in a mirrored copy (`D:\TileVaniaValidationCopy`) and then synced back. Your project was verified to be byte-identical to the tested copy.

| Check | Result |
|---|---|
| Compile + full regeneration (`BuildAllBatch`) | ✅ 0 errors. The validator reports **0 problems**, including new checks: every scene has a `StoryDirector`, and story beats are known and not duplicated. |
| Layout reachability (`reach.py`) | ✅ ALL OK. All 9 story markers are reachable (the pre-existing conservative "unreachable gems" notes are unchanged). |
| **Play Mode tests** | ✅ **55 / 55 pass**: the 39 previous tests (now with story dialogue active, auto-advanced) plus **16 new `StoryTests`**. These cover dialogue freeze and advance, barks, pause blocking, the opening, the notched tree (once per cycle), the readable lantern, memory shards persisting across a restart, story points placed in the layouts, out-of-order prevention at the castle door, the boss intro while frozen, retry without repeating, and the full ending → forest loop with one more notch and **Begin Again**. |
| Windows player | ✅ Build succeeded (110 MB, 0 errors). A smoke run of the `.exe` started and loaded the menu with no errors logged. |
| Screenshots | ✅ New `*_story_*`, `*_lantern_*` and `*_ending_*` captures reviewed. The tally marks, note, scrawl, ghost, dialogue box and letterbox all read clearly. |

**Not performed:** human play-testing of the story's pacing, text speed, the readability of the tally at other resolutions, and the emotional landing of the ending. See the *Story* section of `QA_CHECKLIST.md`.

**Notes:**
- Finishing the game saves one extra notch in PlayerPrefs (`story_completed_cycles`). To reset it, delete that key (or clear PlayerPrefs). The tests restore it themselves.
- Loading the forest scene directly in the Editor skips the opening cutscene; only **Play** from the menu, or the loop, triggers it.

## Iteration 2: controls and gameplay fixes

| Issue | Root cause | Fix (survives Rebuild Game) |
|---|---|---|
| Could not climb down the Level 1 shaft ladder | **(1)** The hero passes wooden ledges by turning off Player↔OneWay collisions in the layer matrix. But `PlatformEffector2D` defaults to its own "collide with everything" mask (`useColliderMask = true`), so the matrix was ignored. On the shaft's ladder-top ledge, climbing started but the hero never moved. **Down + Jump drop-through was broken for the same reason.** **(2)** Grabbing from above tested a single point under the hero's centre, so standing slightly off-centre over the 1-tile shaft never grabbed it. | `LevelBuilder`: `useColliderMask = false`. `PlayerController`: ladder detection with 0.3-tile tolerance, evaluated at cell centres (no edge ambiguity), and snapping to the detected column. |
| (found while testing) Climbing **down** a ladder that stands on a wooden ledge (Level 2 watchtower) fell through the ledge | The bottom-exit rule only recognised *solid* ground | Exit when no ladder is under the feet and *any* ground (solid or one-way) is. Ledge collision is restored immediately. |
| Objects in Level 2 looked embedded in the floor | **Sorting, not transforms.** The castle floor's top lip (and the forest grass and flowers) was on the `Foreground` sorting layer, so it was drawn over the feet of everything standing on the floor. Spikes were half hidden. | The decor tilemap is now `SurfaceDecor` on the `Platforms` layer (behind all gameplay objects). |
| Chest sprite bounds extended below the floor | **Transform/slicing:** the 48×48 chest frames have 12 transparent rows under the art | Chests are sliced to the art (48×24) with the pivot at the true bottom. |
| Floating pickups could dip into ledges while bobbing | Placed at the cell centre regardless of sprite size or bob amplitude | `LevelBuilder.PickupPosition` computes the height from sprite bounds plus bob amplitude. |
| Spiders could bob into the ceiling | Body placed at the layout cell, ignoring the 1-unit bob | Placement keeps the whole bob below the anchor. |
| Two castle windows sat behind wooden ledges | A side effect of lowering ledges in iteration 1 | Windows moved up in the layout. |
| "Found the Gold Key!" visible in every screenshot | **Screenshot capture artifact:** the capture tool deliberately injected that toast to show the HUD. The runtime lifecycle was fine: toasts fade after about 3 s, and each scene has its own HUD. | The capture tool no longer fakes a toast. A test now verifies fade-out and that nothing carries into the next scene. |

**New controls:** see `GAME_DESIGN.md` §1. Arrow keys plus Z/Space/X/Esc, context-sensitive Up, no mouse in gameplay, gamepad kept.

**Validation (iteration 2):** clean compile; validator 0 problems; **39/39 Play Mode tests pass**; reachability check OK; Windows build succeeded; regenerated screenshots reviewed.

## Completed

| Area | Status |
|---|---|
| Preparation | Audit read; downloads inspected; recovery tag and branch created; licenses checked (see `ASSET_CREDITS.md`) |
| Asset import | Approved art and audio copied into `Assets/_Game` and configured consistently: point filter, uncompressed, no mips, explicit PPU, custom pivots, stable sprite IDs |
| Player | Acceleration movement, coyote time, jump buffer, variable jump, ladders, one-way drop-through, forward and upward shooting, 3 hearts, knockback, i-frames, death sequence, pits, juice (dust, squash, sounds) |
| Enemies | Spikes, slime, spider, skeleton (raycast patrol), ghost (telegraphed ranged), bat (flying). Single direction-independent hurtbox |
| Combat fixes from the audit | Dedicated projectile layers; projectiles can't touch the player, pickups or exits; spawn-in-wall guard; layer matrix enforced in code and project settings; no tag- or trigger-exit-based logic |
| Items and progression | Gems, hearts, gold and treasure keys, gates and doors, iron/silver/gold chests, checkpoints, animated portals, explicit scene flow (no looping) |
| Levels | Forest + underground (128×30), Castle (140×34), Treasure Chamber (40×22), built from ASCII layouts with auto-tiling, back-walls, decor, lights and parallax |
| Boss | The dragon (later named "The Keeper"): 3 telegraphed attacks, enrage phase, health bar, hit and defeat effects, treasure reveal, full reset on retry |
| UI | Main menu (Play/Settings/Credits/Quit), HUD, pause, death and victory panels, boss bar, level title cards, toasts, fades |
| Audio | Cross-fading music per area, 24 mapped SFX, independent saved Music/SFX volume |
| Tooling | Batch-capable builder (`Tools → The Last Memory → …`), scene validator, screenshot capture, Windows player build, Python level generator and reachability checker (`Tools/LevelTools`) |
| Docs | `GAME_DESIGN.md`, `ASSET_CREDITS.md`, `QA_CHECKLIST.md`, this file, `Docs/Screenshots/` |

## Verification actually performed

| Check | Result |
|---|---|
| C# compilation (batch mode) | ✅ 0 errors, 0 project warnings |
| Full regeneration (`GameBuildPipeline.BuildAllBatch`) | ✅ Completes. The validator reports **0 problems**: no missing scripts or references, non-empty collider geometry, every `nextScene` in the build list, managers and EventSystem present in every scene |
| Build settings | ✅ `MainMenu`, `Level1_Forest`, `Level2_Castle`, `Level3_TreasureChamber` (old course scenes removed from the build) |
| **Automated Play Mode tests** (real scenes, simulated input) | ✅ **39 / 39 pass** (iteration 2 added controls, ladder, placement and toast tests). Iteration 1 tests: spawn and run, jump height and landing, ladder climb to a ledge, forward shot kills the slime, the skeleton dies from either side, upward shot, gem pickup, spikes cost exactly 1 heart, pit respawn, death panel and retry, gate blocks then opens with the key, **shots don't trigger the exit**, portal loads the castle, treasure door needs the right key, boss starts/dies/reveals the treasure/shows victory, **boss beaten with real projectiles in under 30 s of simulated play**, boss resets on retry, menu Play loads Level 1, all scenes run without logged errors |
| Layout reachability (static solver) | ✅ Every key, gate, portal and treasure is reachable in order. It found, and I fixed, ledges and boss platforms that were 4–5 tiles high (the jump is 3.5) |
| Windows standalone build | ✅ Succeeded (110 MB). A 10 s headless run of the built `.exe` logged no errors |
| Visual review | ✅ Rendered screenshots of every area, the HUD, the pause menu, the boss bar and the victory panel were reviewed and fixed iteratively (`Docs/Screenshots/`) |

**Not performed:** no human hands-on play-testing. I could not drive the interactive Editor, so game *feel*, difficulty, audio mix and readability at speed still need your manual pass (`QA_CHECKLIST.md`). The gamepad bindings exist but are untested.

## Known limitations / remaining work

1. **Licenses to confirm** for four OpenGameArt packs and two music tracks. OpenGameArt blocked automated access; see the action items in `ASSET_CREDITS.md`.
2. **Missing animation frames in the hero pack:** climbing loops the jump frames, hurt uses one apex frame, death is procedural (spin and fade). Fine for now; a PRO pack or custom frames would improve it.
3. **Mixed pixel density:** the enemy sheets are higher resolution than the 16 px tiles (shown at 80 PPU). They're consistent in size but not in pixel grid.
4. **SFX picks** were chosen by file name from the Kenney packs, so some may not suit. Swap them in `Resources/SfxLibrary.asset` without touching code.
5. **Pixel scale** is exact at 1080p (5×). Other resolutions use a non-integer scale, so slight shimmer is possible. A Pixel Perfect Camera could be added later.
6. **The builder overwrites the generated scenes and prefabs.** For hand-edited scenes, either stop using Rebuild or put the changes in the layouts and builder code.
7. Difficulty is untuned by humans. The boss has 30 HP, and the simulated bot (with healing) beat it in under 30 s. Real players will take longer, but it may still need a tuning pass.
8. Nice-to-haves not done: save/continue, level select, footstep sounds, gamepad UI testing, a Pixel Perfect Camera, rebindable controls.
9. Cinemachine was removed in iteration 4 with the legacy content, and the Git-hosted Hot Reload package in iteration 5. The project no longer needs Git to open.

## Decisions for your review

- **Boss as a dedicated scene** (`Level3_TreasureChamber`). This makes respawn and reset logic simpler and keeps Level 2 smaller.
- **Glax's dragon is the boss.** It is the only creature in the approved packs large enough for a boss.
- **Level geometry comes from ASCII layouts plus an editor builder**, not hand-painted tilemaps. This made the levels verifiable, reproducible and testable from the CLI. The scenes are normal Unity scenes and can still be edited afterwards.
- **A custom camera script replaced Cinemachine** for exact bounds clamping, pixel snapping and screen shake with no extra wiring.
- **Legacy content moved** (`git mv`, GUIDs preserved) to `Assets/_Legacy` (removed in iteration 4; still in the Git history).
- **Ladder art is generated** because none of the approved packs has a ladder or rope.
- The **Symtext font change** (pre-existing, unrelated) is left uncommitted, as requested.
