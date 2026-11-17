# The Last Memory: Game Design (as implemented)

A short 2D pixel-art action-platformer with a story. A hero wakes in a forest with no memory, crosses ancient ruins and an abandoned castle, and defeats the dragon that guards a chest: **the Keeper**. The chest holds the memory of their little sister, Lia. A flashback shows what happened, and the hero chooses to forget her again and wakes in the forest with one more notch on the tree. The full story and every line of dialogue are in **`STORY_SPEC.md`**.

**Flow:** Main Menu → Level 1 (Forest, opening cutscene) → Level 2 (Castle) → Treasure Chamber (Keeper) → ending (flashback and choice) → **back to Level 1** (the opening repeats, with one more notch on the tree) → ending panel → *Begin Again* (keep playing the new cycle) or Main Menu.
Level progression uses explicit scene names, not build-index arithmetic. The only loop is the deliberate story loop from the chest back to the forest.

---

## 1. Controls (keyboard only, no mouse needed; gamepad supported)

| Action | Keyboard | Gamepad |
|---|---|---|
| Move | ← / → | Left stick / D-pad |
| Jump | **↑**, or **Z** / **Space** | South button |
| Climb a ladder | **↑** near a ladder climbs (instead of jumping); **↓** climbs down, including from the top of a ladder | Stick / D-pad up / down |
| Shoot forward | **X** | West button |
| Shoot upward | **C**. Alternative: **↑ + X** | Right shoulder. Alternative: stick up + West |
| Drop through a wooden ledge | **↓ + Z** (or ↓ + Space) | Down + South |
| Jump off a ladder | Z / Space (← / → steps off) | South |
| Examine (the rusted lantern) | **Enter** when the "Enter Examine" prompt shows | North |
| Advance dialogue | **Z / Space / X / C / Enter**. The first press completes the line, the next one advances | South / East / West |
| Pause | Esc (ignored during dialogue and cutscenes) | Start |
| Menus | Arrow keys + Enter (mouse optional) | Stick + South |

**Upward shooting.** **C** always fires straight up. It never jumps and never changes movement, so it works standing, running, in the air, and right after a jump. (Like X, it doesn't fire while climbing a ladder.) The older **↑ + X** still works: on the ground, ↑ waits 0.09 s before jumping, and pressing X inside that window fires upward instead of jumping. That combination depends on timing and press order, which is why C was added (iteration 4).

**How ↑ is resolved.** On a ladder, ↑ climbs. Elsewhere, ↑ jumps after the 0.09 s grace window described above. Z and Space jump instantly. Holding any jump key (including ↑) gives the full jump height. Jump buffering and coyote time apply to every jump key. Input is defined in code (`GameInput.cs`) on the Input System.

**Ladders.** You can grab a ladder when the hero's centre is within 0.3 tiles of its column; the hero then snaps onto it. You can stop anywhere on the ladder. Reaching the bottom (solid ground or a wooden ledge) steps you off. Climbing past the top puts you on the ledge above.

## 2. Player

| Mechanic | Implementation / tuning |
|---|---|
| Run | Acceleration-based: max 7.5 u/s, ground accel 90, decel 100, air accel 55 |
| Jump | 16.5 u/s launch, gravity scale 4 → apex ≈ **3.5 tiles**. 0.1 s coyote time, 0.12 s jump buffer, variable height (release early to cut), 1.6× fall gravity, max fall 22 u/s |
| Ladders | ↑ near a ladder or ↓ above it grabs it (0.3-tile tolerance) and snaps to its centre. Climb speed 4.5 u/s with gravity off. Z/Space leaps off, ← / → steps off. Ladders pass through wooden ledges (`I` tiles). |
| One-way ledges | Jump up through them; ↓ + Z drops through. |
| Shooting | X fires a sword-energy slash forward; C (or ↑ + X) fires it straight up from above the head. 16 u/s, ≈14-tile range, 0.3 s cooldown shared by both directions. It never spawns inside a wall: point-blank against a wall it shows only a spark. |
| Facing | Driven by horizontal input only. The sprite flips on the child `Visual` transform, so colliders are never mirrored. |
| Health | 3 hearts. A hit costs 1 heart, knocks the hero back, and gives 1.2 s of blinking invulnerability. |
| Death | Brief freeze, then a hop and spinning fall with a red fade, then the **"You Fell"** panel: *Retry from Checkpoint* (full hearts) or *Main Menu*. |
| Pits | Falling out of the level costs 1 heart and fades you back to the last checkpoint. |
| Feedback | Jump/land dust, squash and stretch, landing sound scaled by impact, hit sparks, screen shake, sounds for every important action. |

Animations (Animator states played from code): Idle, Run, Jump, Apex, Fall, Climb, Attack, Hurt. The hero pack has no climb, hurt or death frames: climb loops the jump frames, hurt reuses an apex frame, and death is procedural.

---

## 3. Enemies and hazards

| # | Requirement | Implementation | Prefab | HP |
|---|---|---|---|---|
| 1 | Stationary, not animated | **Spikes** (contact damage) | `Hazard_Spikes` | — |
| 2 | Stationary, animated | **Slime** (wobbles in place) and **Spider** (dangles on a silk thread, bobbing up and down) | `Enemy_Slime`, `Enemy_Spider` | 2 / 2 |
| 3 | Moving, animated | **Skeleton**: walks and turns at walls and ledges, detected by raycasts | `Enemy_Skeleton` | 3 |
| 4 | Shoots at the player | **Ghost**: hovers. When the player is within 9 tiles with clear line of sight, it flashes violet for 0.6 s (telegraph), then fires an aimed bolt | `Enemy_Ghost` | 2 |
| + | Flying (optional) | **Bat**: wavy back-and-forth flight | `Enemy_Bat` | 1 |

All enemies take damage through **one hurtbox trigger**, so hits register the same from any direction. Touching any enemy body costs a heart. A defeated enemy bursts into particles with a sound and a small shake. Defeated enemies stay defeated when you retry from a checkpoint; restarting the level brings them back.

---

## 4. Collectibles, keys and chests

| Item | Purpose |
|---|---|
| **Gems** (green/blue/red/orange) | Main collectible: **fragments of the hero's memory**. Shown in the HUD and on the ending panel. 53 placed by hand, plus chest rewards (≈86 in total). |
| **Memory shard** | Optional, pale and glowing, 2 in the forest and 2 in the castle. Worth 3 gems; picking one up briefly freezes the world for a remembered line. Each one is a piece of the ending's flashback; finding all four adds the hero's moment of hesitation before the final choice. |
| **Heart** | Restores 1 heart. It stays in the world if you're already at full health. |
| **Gold Key** | Opens the iron **gate**. Level 1: it guards the portal chamber in the ruins. Level 2: it guards the castle's east wing. Keys are per level and consumed on use. |
| **Treasure Key** | Level 2 only. Opens the **Treasure Chamber door** that leads to the Keeper. |
| **Iron / Silver chest** | Touch to open: an animated lid, then 3 or 6 gems fly out and home in on you, and 1 heart is restored. |
| **Gold chest** | The Keeper's chest. It appears when the Keeper is defeated; opening it starts the ending. |
| **Checkpoint** | A stone pillar that lights a flame (with a 2D light), sets the respawn point and refills hearts. |

HUD (top-left): hearts, gem count, and icons for held keys. Level title cards appear at level start, and toasts (keys, locked gates, checkpoints) show at the bottom centre.

---

## 5. Levels

Layouts are ASCII files in `Assets/_Game/Levels/` that the editor builder turns into scenes (§9, and `STUDENT_PROJECT_GUIDE.md` §19).

### Level 1 — The Whispering Forest (`Level1_Forest`, 128×30 tiles)
1. **Meadow (tutorial):** run, jump, gems, a one-way ledge, a slime to shoot, the first spike dip, and the notched tree.
2. **Ruins:** checkpoint, a raised field with a patrolling skeleton, broken columns, a **ladder** up to a ledge holding the **Gold Key**, and a ghost overlooking the area.
3. **Chasm:** a bottomless 4-tile gap.
4. **Underground entrance:** checkpoint, then a ruined gateway (with the rusted lantern) and a ladder shaft down into a dark cave: skeletons, spikes, a hanging spider, an iron chest, a heart.
5. **Portal chamber:** the **Gold Gate** blocks it. Behind it are a silver chest and the **exit portal**.
6. Optional: the surface above the cave holds more gems, a ghost, a skeleton, a heart and a memory shard.

### Level 2 — The Abandoned Castle (`Level2_Castle`, 140×34 tiles)
Darker purple/blue palette, torch-lit walls with flickering lights, battlement back-walls, night sky.
1. **Entrance hall** with wooden ledges.
2. **Spike hall:** wooden bridges over two spike beds, with a bat in between.
3. **Broken bridge:** bottomless gap crossed on three pillars, with two bats, a ghost, and the Faint Ghost.
4. **Watchtower:** two ladders and wooden floors up into the night sky, past a spider and a ghost, to the **Gold Key**.
5. **Gold Gate** → **spike gallery:** a 17-tile spike floor crossed on three bridges, with a skeleton, a bat and a ghost.
6. **Treasure-key tower:** a ladder up to a ledge with a slime, a silver chest and the **Treasure Key**.
7. **Great hall:** two skeletons, a ghost, an iron chest and a heart; then the **Treasure Chamber door** (with "I was so close.") and the portal.

### Treasure Chamber (`Level3_TreasureChamber`, 40×22 tiles)
An entry corridor with a checkpoint opens into a torch-lit hall with two side platforms, a central platform and two hearts. Crossing the threshold drops an iron portcullis behind you and wakes the Keeper. The flashback set (§7a) is built to the right of this level, outside the playable area.

### Story points in the levels
Story markers are the digits **1–9** in the layouts. Their meaning per level is in `StoryScript.BeatForDigit`.

| Level | Digit | Beat | Trigger |
|---|---|---|---|
| Forest | 1 (x 6) | Notched tree + pinned note | automatic, required: 3 steps from the spawn |
| Forest | 2 (x 74) | Rusted lantern | optional, **Enter** to examine (glints) |
| Forest | 3, 4 | Memory shards 1 and 2 | pickup |
| Castle | 1 (x 13) | Déjà vu thought | automatic, doesn't stop play |
| Castle | 2 (x 60) | Faint Ghost | automatic; fades away afterwards |
| Castle | 3 (x 123) | "I was so close." + tally marks by the treasure door | automatic, **requires the ghost** |
| Castle | 4, 5 | Memory shards 3 and 4 | pickup |
| Chamber | — | Keeper intro, retry line, defeat, ending | scripted in `DragonBoss` / `StoryDirector` / `MemoryFlashback` |

---

## 6. Boss — The Keeper

A green dragon (Glax's dragon sprite at 2×) with **30 HP** and a health bar at the top of the screen. It is not evil: it guards the memory because the hero once made it promise to (see `STORY_SPEC.md`). After it swoops in it **speaks while the world is frozen**, and the fight starts when the dialogue ends. On a retry it only says "Again, then." without stopping play.

| Attack | Telegraph | Behaviour | How to deal with it |
|---|---|---|---|
| **Fire Volley** | Flies to the wall opposite you, flashes orange and shakes, bell sound | A fan of 3 aimed fireballs (5 when enraged, and twice) | Jump between platforms. It's level with the side platforms, so straight shots hit it. |
| **Dive Slam** | Hovers over you; a **red marker** pulses on the floor for 0.9 s | Slams down, shaking the screen. When enraged, it sends fire shockwaves along the floor both ways. | Move off the marker, then punish it while it recovers. Jump over the shockwaves. |
| **Fire Rain** | Rises over the hall; **4–6 red floor markers** pulse for 1.1 s | Fire falls on the marked spots. One gap is always left open. | Stand on an unmarked spot and shoot upward (C). |

- **Enraged** below 50% HP: a red tint, the message "The Keeper is enraged!", shorter pauses, more projectiles and shockwaves.
- **Defeat:** chained explosions and flashing, then its last words (frozen dialogue). It **sinks to the floor by the right wall, wounded and grey, and stays there** (it can neither hurt nor be hurt). The music stops and the gold chest appears. It takes part in the ending.
- **Retry:** if you die, the fight fully resets (HP, position, music), the door reopens, and you respawn at the corridor checkpoint.

---

## 7a. Story cutscenes (no timeline: plain coroutines moving existing objects)

- **Opening (forest, `StoryDirector`):** black screen with letterbox bars, "...wait for me...", fade in on the hero lying under the tree; they stand up and speak two lines; the title card appears. It plays when a cycle begins: **Play** from the menu, or the loop. Loading the forest directly in the Editor skips it.
- **Ending (chamber, `StoryDirector` + `MemoryFlashback`):**
  1. The chest's light swells; Lia's dark figure with a lit lantern appears in it. *"You said you'd be home by morning."*
  2. White flash into the **flashback** (HUD hidden, letterbox on): four memories staged with two actors (`MemoryActor`: the hero as they were, and Lia), camera cuts and pans, and lighting changes:
     - Memory 1, the cottage at night (open door, lit window): the promise.
     - Memory 2, close on the window (Lia with the lantern, then the lantern gone); then the forest path with the castle on the horizon: Lia follows, the hero sends her home and walks on; Lia is left alone with her lantern.
     - Memory 3, the same cottage at dawn: the empty house, the dropped sack of gold, the search.
     - Memory 4, the chamber itself, long ago (warm sepia veil, the Keeper unhurt, no chest): the bargain and the promise.
  3. White flash back to the present: the hero remembers, the Keeper offers to let them keep the memory, the hero says *"Take it back"*, turns to the Keeper (which rises), and says goodbye to Lia.
  4. Lia's figure dissolves, 36 gems burst out and scatter, and the HUD gem counter drains to 0. White-out, *"(And again, I chose to forget.)"*, black, a single knock, the whisper.
  5. The completed-cycle count is saved and the game loads the forest.
  - The flashback takes about 70–100 s depending on reading speed; the whole ending about 2–2.5 minutes. Lines advance with a key or by themselves.
- **Loop (forest again):** the same opening; the hero walks to the tree; the newest notch glows; the note, "What chest?", the caption ***"Somewhere, a lantern is still burning."***, then the **ending panel** with **Begin Again** and **Main Menu**.
- **Notches** = 7 + completed cycles, saved in PlayerPrefs (`story_completed_cycles`). The castle door's tally shows the same count.
- **Dialogue box** (`DialogueBox`): speaker plate, typewriter text, continue hint. Styles: speech, thought (italic), note, voice and memory. Modes: **Blocking** freezes the world and takes control; **Cinematic** for cutscenes (lines also advance after a reading pause); **Bark** is a thought that never takes control. Control comes back 0.12 s after the last line, so the closing key doesn't also jump or shoot. One key press advances at most one line.

## 7. UI and audio

- **Main menu:** the title *The Last Memory* and the tagline *"Every gem is something you have forgotten."* over a parallax forest diorama with the hero, a treasure chest and a portal. Buttons: Play, Settings (Music/SFX sliders), Credits, Quit. A control hint runs along the bottom. Fully keyboard-navigable.
- **Pause (Esc):** Resume, Restart Level, Music and SFX sliders, Main Menu.
- **Death panel** ("You Fell — Not yet. Not like this.") and the **ending panel** (§7a); fade-to-black transitions between scenes.
- **Examine prompt** ("Enter  Examine") above the toast line when standing at a readable object.
- **Music:** *8-bit Perilous Dungeon* (menu and forest), *Spooky Dungeon* (castle and pre-fight), *8-bit Slay the Evil* (boss fight). Cross-faded. The ending is silent apart from sound effects.
- **SFX:** Kenney Impact & Interface packs, mapped through `Resources/SfxLibrary.asset`.
- **Volume:** independent Music and SFX volumes, saved in PlayerPrefs.

---

## 8. Visual design

- Pixel art with point filtering, no compression and no mipmaps. Tiles, hero, items and the story art use 16 px = 1 unit. Enemy sheets use 80 PPU to match the hero's size. The dragon uses 8 PPU (an exact 2× upscale).
- **Tiles are extruded** (iteration 4): the importer copies every tilemap tile into its own slot with a 1-pixel border of its own edge pixels (`Forest_Tiles_Extruded.png`, `Castle_Tiles_Extruded.png`). Without this, a pixel landing exactly on a tile edge at a sub-pixel camera position could sample the neighbouring cell of the sheet and show a thin seam.
- Camera: orthographic size 6.75, exactly 5× pixel scale at 1080p. Smoothed follow with look-ahead, clamped to the level bounds, screen shake, snapped to the screen-pixel grid. Cutscenes can frame fixed shots outside the level (`CutTo`, `PanTo`, `EndShot`).
- Forest: greens and browns under a dusk sky, parallax mountains, trees and bushes; a dark underground. Castle: purple battlements, blue-grey bricks, wooden bridges, flickering torches, a dim bluish light, a night sky.
- Effects: pooled pixel-particle bursts (dust, sparkles, hits, explosions), squash and stretch, flashes, telegraph pulses.

---

## 9. Technical structure

```
Assets/_Game/
  Animations/     Hero clips + Hero.controller
  Art/            third-party art by pack, Generated/ (project-made), Licenses/
  Audio/          Music/, SFX/
  Levels/         ASCII layouts (the source of level geometry and placement)
  Physics/        NoFriction material
  Prefabs/        player, enemies, projectiles, pickups, chests, gates, checkpoint, portal, torch, boss, GameCanvas (generated)
  Resources/      SfxLibrary.asset
  Scenes/         MainMenu, Level1_Forest, Level2_Castle, Level3_TreasureChamber (generated)
  Scripts/Runtime LastMemory.Runtime.asmdef: Audio, Boss, Combat, Core, Effects, Enemies, Items, Level, Player, Story, UI
  Scripts/Editor  LastMemory.Editor.asmdef: ArtSetup, GeneratedArt, ContentBuilder, UIBuilder, LevelBuilder, MemoryStageBuilder, GameBuildPipeline
  Tests/PlayMode  LastMemory.Tests.asmdef: GameplayTests, ControlsAndLadderTests, StoryTests, FlashbackScreenshots (explicit)
  Tiles/          Tile assets generated from the extruded sheets
  UI/             title text material
```

All C# code is in the `LastMemory` namespace (editor tools: `LastMemory.EditorTools`).

Key runtime systems: `GameInput` (input), `Layers` (physics layers and collision matrix, enforced at startup), `GameSession` (gems, deaths, time, keys), `LevelManager` (per-scene flow), `SceneLoader` (fades), `AudioManager`, `CameraController`, `Fx`, `HUD`, `GameMenus`, `DragonBoss`.

Story systems (`Scripts/Runtime/Story`): `Story` (cycle flags, once-only and in-order beats, saved notch count), `StoryScript` (all text, layout-digit → beat table), `DialogueBox`, `StoryPoint`, `MemoryShard`, `TallyMarks`, `Glint`, `StoryDirector` (opening, ending, loop), `MemoryFlashback` + `MemoryActor` (the flashback). `CinematicOverlay` (UI): letterbox, fades and caption.

**Physics layers:** Ground, OneWay, Ladder, Player, Enemy, Hazards, Interactables, PlayerProjectile, EnemyProjectile. Player projectiles collide only with Ground and Enemy. Enemy projectiles collide only with Ground and Player. Pickups, portals and gates are on Interactables, which only the player touches.

### Rebuilding / editing levels
- **Tools → The Last Memory → Rebuild Game** regenerates art import settings, prefabs, UI and all four scenes from the layouts. It **overwrites** the generated scenes and prefabs, so change layouts in the `.txt` files and everything else in the builder code.
- **Tools → The Last Memory → Validate Scenes** checks for missing scripts and references, empty collider geometry, the build list, story points, and the ending's flashback wiring.
- **Tools → The Last Memory → Capture Preview Screenshots** writes `Docs/Screenshots/*.png`.
- **Tools → The Last Memory → Build Windows Player** builds to `Builds/TheLastMemory_Windows/The Last Memory.exe`.
- Batch mode: `Unity.exe -batchmode -projectPath <project> -executeMethod LastMemory.EditorTools.GameBuildPipeline.BuildAllBatch` (the Editor must be closed).

**Layout legend** (`y` = 0 is the bottom row; the header line sets the theme and back-wall rule):

| Char | Meaning | Char | Meaning |
|---|---|---|---|
| `#` | solid ground (auto-tiled) | `s` | skeleton (patrol) |
| `=` | one-way wooden ledge | `m` | slime (stationary) |
| `H` | ladder | `o` | ghost (shooter) |
| `I` | ladder + ledge top | `b` | bat (flyer) |
| `^` | spikes | `p` | spider (hangs from the ceiling/ledge above) |
| `P` | player start | `B` | boss start (it flies in from above) |
| `C` | checkpoint | `T` | final treasure spot |
| `g` | gem | `X` | boss-fight trigger |
| `h` | heart | `Z` | arena door (closes behind the player) |
| `k` / `K` | gold key / treasure key | `t` | torch with light |
| `L` / `D` | gold gate / treasure door (2×2, bottom-left anchor) | `w` | castle window |
| `c` / `$` | iron / silver chest | `,` `*` `\|` `A` | grass/bush, flower, column, ruined gateway |
| `E` | exit portal | `.` | empty |
| `1`–`9` | story marker (meaning per level: `StoryScript.BeatForDigit`) | | |
