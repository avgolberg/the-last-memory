# The Last Memory: Game Design (as implemented)

A short 2D pixel-art action-platformer with a story, **The Last Memory**. A hero wakes in a forest with no memory, crosses ancient ruins and an abandoned castle, and defeats the dragon that guards a chest. The chest holds the one memory they need. It slips away just as they reach it, and they wake in the forest again. The full story and every line of dialogue are in **`STORY_SPEC.md`**.

**Flow:** Main Menu → Level 1 (Forest, opening cutscene) → Level 2 (Castle) → Treasure Chamber (Boss) → ending cutscene → **back to Level 1** (the opening repeats, with one more notch on the tree) → ending panel → *Begin Again* (keep playing the new cycle) or Main Menu.
Level progression uses explicit scene names, not build-index arithmetic. The only loop is the deliberate story loop from the chest back to the forest.

---

## 1. Controls (keyboard-only, no mouse needed; gamepad supported)

| Action | Keyboard | Gamepad |
|---|---|---|
| Move | ← / → | Left stick / D-pad |
| Jump | **↑**, or **Z** / **Space** | South button |
| Climb a ladder | **↑** near a ladder climbs (instead of jumping); **↓** climbs down, including from the top of a ladder | Stick / D-pad up / down |
| Shoot forward | **X** | West button |
| Shoot upward | **↑ + X**, pressed together; it does not jump | Stick up + West |
| Drop through a wooden ledge | **↓ + Z** (or ↓ + Space) | Down + South |
| Jump off a ladder | Z / Space (← / → steps off) | South |
| Examine (note, lantern) | **C** or **Enter** when the "C Examine" prompt shows | North |
| Advance dialogue | **Z / Space / X / C / Enter**. The first press completes the line, the next one advances | South / East / West |
| Pause | Esc (ignored during dialogue and cutscenes) | Start |
| Menus | Arrow keys + Enter (mouse optional) | Stick + South |

**How Up is resolved.** On a ladder, ↑ climbs. Elsewhere, ↑ jumps after a 0.09 s grace window. If X is pressed inside that window, the jump is cancelled and the shot goes upward. Z and Space jump instantly, and holding any jump key (including ↑) gives the full jump height. Jump buffering and coyote time apply to every jump key; coyote time is measured from the moment the key was pressed, so the grace window never eats it. Input is defined in code (`GameInput.cs`) on the new Input System.

**Ladders.** You can grab a ladder when the hero's centre is within 0.3 tiles of its column; the hero then snaps onto it. You can stop anywhere on the ladder. Reaching the bottom (solid ground or a wooden ledge) steps you off. Climbing past the top puts you on the ledge above.

## 2. Player

| Mechanic | Implementation / tuning |
|---|---|
| Run | Acceleration-based: max 7.5 u/s, ground accel 90, decel 100, air accel 55 |
| Jump | 16.5 u/s launch, gravity scale 4 → apex ≈ **3.5 tiles**. 0.1 s coyote time, 0.12 s jump buffer, variable height (release early to cut), 1.6× fall gravity, max fall 22 u/s |
| Ladders | ↑ near a ladder or ↓ above it grabs it (0.3-tile tolerance), and the hero snaps to the ladder's centre. Climb speed is 4.5 u/s with gravity off; the hero stops when you release. Z/Space leaps off, ← / → steps off. Ladders pass through wooden ledges (`I` tiles). |
| One-way ledges | Jump up through them; ↓ + Z drops through. |
| Shooting | X fires a sword-energy slash projectile (16 u/s, ≈14-tile range, 0.3 s cooldown). It spawns in front of the hero at waist height (or above the head with ↑ + X) and never inside a wall: point-blank against a wall it shows only a spark. |
| Facing | Driven by horizontal input only. The sprite flips on the child `Visual` transform, so colliders are never mirrored. |
| Health | 3 hearts. A hit costs 1 heart, knocks the hero back, and gives 1.2 s of blinking invulnerability. |
| Death | Brief freeze, then a hop and spinning fall with a red fade, then the **"You Fell"** panel: *Retry from Checkpoint* (full hearts) or *Main Menu*. |
| Pits | Falling out of the level costs 1 heart and fades you back to the last checkpoint. |
| Feedback | Jump/land dust, squash and stretch, landing sound scaled by impact, hit sparks, screen shake, sounds for every important action. |

Animations (Animator states played from code): Idle, Run, Jump, Apex, Fall, Climb, Attack, Hurt.
The hero pack has no climb, hurt or death frames. Climb loops the jump frames, Hurt reuses an apex frame, and death is procedural (see §8).

---

## 3. Enemies and hazards

| # | Requirement | Implementation | Prefab | HP |
|---|---|---|---|---|
| 1 | Stationary, not animated | **Spikes** (contact damage) | `Hazard_Spikes` | — |
| 2 | Stationary, animated | **Slime** (wobbles/hops in place) and **Spider** (dangles on a silk thread, bobbing up and down) | `Enemy_Slime`, `Enemy_Spider` | 2 / 2 |
| 3 | Moving, animated, patrols | **Skeleton**: walks and turns at walls and ledges, detected by raycasts | `Enemy_Skeleton` | 3 |
| 4 | Shoots at the player | **Ghost**: hovers. When the player is within 9 tiles with clear line of sight, it flashes violet for 0.6 s (telegraph), then fires an aimed bolt | `Enemy_Ghost` | 2 |
| + | Flying (optional) | **Bat**: wavy back-and-forth flight | `Enemy_Bat` | 1 |

All enemies take damage through **one hurtbox trigger**, so hits register the same from any direction. Touching any enemy body costs a heart. A defeated enemy bursts into particles with a sound and a small shake. As in the original project, defeated enemies stay defeated when you retry from a checkpoint; reloading or restarting the level brings them back.

---

## 4. Collectibles, keys and chests

| Item | Purpose |
|---|---|
| **Gems** (green/blue/red/orange) | Main collectible: **fragments of the hero's memory** (the first one of each cycle says so in a short thought). Shown in the HUD and on the ending panel. 53 are placed by hand, plus chest rewards (≈86 in total). |
| **Memory shard** | Optional, pale and glowing, 2 in the forest and 2 in the castle. Worth 3 gems; picking one up briefly freezes the world for a remembered line. Found shards stay found for the rest of the cycle. |
| **Heart** | Restores 1 heart. It stays in the world if you're already at full health. |
| **Gold Key** | Opens the iron **gate**. Level 1: it guards the portal chamber in the ruins. Level 2: it guards the castle's east wing. Keys are per level and consumed on use. |
| **Treasure Key** | Level 2 only. Opens the **Treasure Chamber door** that leads to the boss. |
| **Iron / Silver chest** | Touch to open: an animated lid, then 3 or 6 gems fly out and home in on you, and 1 heart is restored. |
| **Gold chest** | The Keeper's chest. It appears when the boss dies; opening it starts the ending cutscene (no gems, no victory screen). |
| **Checkpoint** | A stone pillar that lights a flame (with a 2D light), sets the respawn point and refills hearts. |

HUD (top-left): hearts, gem count, and icons for held keys. The level title card appears at level start, and toasts (keys, locked gates, checkpoints) show at the bottom centre.

---

## 5. Levels

Layouts are ASCII files in `Assets/_Game/Levels/` that the editor builder turns into scenes (see §9).

### Level 1 — The Whispering Forest (`Level1_Forest`, 128×30 tiles)
1. **Meadow (tutorial):** run, jump, gems, a one-way ledge, a slime to shoot, and the first spike dip.
2. **Ruins:** checkpoint, a raised field with a patrolling skeleton, broken columns, a **ladder** up to a ledge holding the **Gold Key**, and a ghost overlooking the area.
3. **Chasm:** a bottomless 4-tile gap.
4. **Underground entrance:** checkpoint, then a ruined gateway with a ladder shaft down into a dark cave. Inside: a skeleton, a spike pair, a hanging spider, an iron chest, a heart on a ledge, and another skeleton.
5. **Portal chamber:** the **Gold Gate** blocks it. Behind the gate are a silver chest and the **exit portal**, which opens with an animation as you approach.
6. Optional: the surface above the cave holds more gems, a ghost, a skeleton and a heart.

### Level 2 — The Abandoned Castle (`Level2_Castle`, 140×34 tiles)
Darker purple/blue palette, torch-lit walls with flickering lights, battlement back-walls, night sky above.
1. **Entrance hall** with wooden ledges.
2. **Spike hall:** wooden bridges over two spike beds, with a bat in between.
3. **Broken bridge:** bottomless gap crossed on three pillars, with two bats and a ghost.
4. **Watchtower:** two ladders and wooden floors up into the night sky, past a spider and a ghost, to the **Gold Key**.
5. **Gold Gate** → **spike gallery:** a 17-tile spike floor crossed on three bridges, with a skeleton patrolling the middle bridge, a bat and a ghost.
6. **Treasure-key tower:** a ladder up to a ledge with a slime, a silver chest and the **Treasure Key**.
7. **Great hall:** two skeletons, a ghost, an iron chest and a heart; then the **Treasure Chamber door** and the portal to the boss.

### Treasure Chamber — Boss (`Level3_TreasureChamber`, 40×22 tiles)
An entry corridor with a checkpoint opens into a torch-lit hall with two side platforms, a central platform and two hearts. Crossing the threshold drops an iron portcullis behind you and wakes the Keeper.

### Story points in the levels
Story markers are the digits **1–9** in the layouts. Their meaning per level is in `StoryScript.BeatForDigit`.

| Level | Digit | Beat | Trigger |
|---|---|---|---|
| Forest | 1 (x 6) | Notched tree + pinned note | automatic, required: 3 steps from the spawn |
| Forest | 2 (x 74) | Rusted lantern | optional, **C** to examine (glints) |
| Forest | 3, 4 | Memory shards 1 and 2 (ledge east of the ruins; surface ledge east of the cave entrance) | pickup |
| Castle | 1 (x 13) | Déjà vu thought | automatic, doesn't stop play |
| Castle | 2 (x 60) | Faint Ghost | automatic; fades away afterwards |
| Castle | 3 (x 123) | "I was so close." + tally marks by the treasure door | automatic, **requires the ghost** |
| Castle | 4, 5 | Memory shards 3 and 4 (watchtower top; past the exit portal) | pickup |
| Chamber | — | Dragon intro (before the fight), retry line, defeat (requires the intro), ending | scripted in `DragonBoss` / `StoryDirector` |

---

## 6. Boss — The Keeper

A green dragon (Glax's dragon sprite, scaled 2× with nearest-neighbour filtering) with **30 HP** and a health bar at the top of the screen. It is not evil: it guards the last memory because the hero once made it promise to (see `STORY_SPEC.md`). After it swoops in, it **speaks first while the world is frozen**, and the fight starts when the dialogue ends. On a retry it only says "Again, then." without stopping play.

| Attack | Telegraph | Behaviour | How to deal with it |
|---|---|---|---|
| **Fire Volley** | Flies to the wall opposite you, flashes orange and shakes, bell sound | A fan of 3 aimed fireballs (5 when enraged, and twice) | Jump between platforms. It's level with the side platforms, so straight shots hit it. |
| **Dive Slam** | Hovers over you; a **red marker** pulses on the floor for 0.9 s | Slams down, shaking the screen. When enraged, it sends fire shockwaves along the floor both ways. | Move off the marker, then punish it while it recovers on the floor. Jump over the shockwaves. |
| **Fire Rain** | Rises over the hall; **4–6 red floor markers** pulse for 1.1 s | Fire falls on the marked spots. One gap is always left open. | Stand on an unmarked spot and fire upward. |

- **Enraged** below 50% HP: a red tint, the message "The Keeper is enraged!", shorter pauses, more projectiles and shockwaves.
- Damage feedback: a flash on each hit plus a metallic sound; the health bar drains.
- **Defeat:** chained explosions and flashing, then its last words (frozen dialogue). The dragon falls and fades, the music stops, and the **gold chest** appears in a burst of light. Opening it starts the **ending** (§7a).
- **Retry:** if you die, the fight fully resets (HP, position, music) and the door reopens, and you respawn at the corridor checkpoint.

---

## 7a. Story cutscenes (no timeline: `StoryDirector` scripts the existing objects)

- **Opening (forest):**
  - Starts from black with letterbox bars, and a distant voice says "...wait for me...".
  - The view fades in on the hero lying down. They stand up and say two lines, then the title card appears and play begins (about 6 s plus reading).
  - It plays when a cycle begins: **Play** from the menu, or the loop. Loading the forest directly in the Editor skips it.
- **Ending (chamber):**
  1. The chest light swells and a small silhouette with a lit lantern appears.
  2. The reunion lines play. The hero steps forward, and the name is cut off.
  3. The silhouette dissolves and 36 gems burst out. The HUD gem counter drains to 0.
  4. White-out, "No... not again...", then black, a single knock, and the whisper "(Don't... open... the chest...)".
  5. The completed-cycle count is saved and the game loads the forest.
- **Loop (forest again):**
  1. The same opening, word for word.
  2. The hero walks to the tree, and the newest notch glows.
  3. Note: "This time, don't open the chest." The hero: "What chest?"
  4. Fade to black and the caption ***"Somewhere, a lantern is still burning."***
  5. Then the **ending panel**: *The Last Memory* with gems, memories x/4, time, deaths and notches, plus **Begin Again** and **Main Menu**.
- **Notches** = 7 + the number of completed cycles, saved in PlayerPrefs (`story_completed_cycles`). The castle door's tally shows the same count.
- **Dialogue box** (`DialogueBox`, bottom of the screen):
  - It shows a speaker plate, typewriter text, and a hint to continue.
  - Line styles: speech, thought (italic), note (parchment), voice (pale blue) and memory (gold).
  - Modes: **Blocking** freezes the world (time scale 0) and takes control. **Cinematic** is used in cutscenes. **Bark** is a short thought that never takes control.
  - Control comes back 0.12 s after the last line, so the closing key doesn't also jump or shoot.
  - Toasts move above the box while it is open.

## 7. UI and audio

- **Main menu:** a parallax forest diorama with the hero, a treasure chest and a portal. It also shows the tagline *"Every gem is something you have forgotten."* Buttons: Play, Settings (Music/SFX sliders), Credits, Quit. Fully keyboard-navigable.
- **Pause (Esc):** Resume, Restart Level, Music and SFX sliders, Main Menu.
- **Death panel** ("You Fell — Not yet. Not like this.") and the **ending panel** described in §7a; fade-to-black transitions between scenes.
- **Examine prompt** ("C  Examine") above the toast line when standing at a readable object.
- **Music:** *8-bit Perilous Dungeon* (menu and forest), *Spooky Dungeon* (castle and pre-fight), *8-bit Slay the Evil* (boss fight). Cross-faded.
- **SFX:** Kenney Impact & Interface packs, mapped through `Resources/SfxLibrary.asset` (edit there to swap sounds).
- **Volume:** independent Music and SFX volumes, saved in PlayerPrefs.

---

## 8. Visual design

- Pixel art with point filtering, no compression and no mipmaps. Tiles, hero and items use 16 px = 1 unit. Enemy sheets use 80 PPU to match the hero's size. The dragon uses 8 PPU (an exact 2× upscale).
- Camera: orthographic size 6.75, which is exactly 5× pixel scale at 1080p. It uses smoothed follow with look-ahead, clamps to the level bounds, adds screen shake, and snaps to the pixel grid.
- Forest: natural greens and browns under a dusk sky with stars, clouds and two parallax mountain ranges, background trees and bushes. The underground has darkened rock back-walls.
- Castle: a purple battlement back-wall, blue-grey brick floors, wooden bridges, barred windows, torches with flickering 2D point lights, a dim bluish global light, and a night sky with stars, moon and distant hills in parallax.
- Effects: pooled pixel-particle bursts (dust, sparkles, hits, explosions), squash and stretch, flashes, telegraph pulses.

---

## 9. Technical structure

```
Assets/_Game/
  Art/            imported, organised third-party art (+ Generated/ project-made pieces, Licenses/)
  Audio/          Music/, SFX/
  Levels/         ASCII layouts (source of truth for level geometry & placement)
  Scenes/         MainMenu, Level1_Forest, Level2_Castle, Level3_TreasureChamber (generated, editable)
  Prefabs/        Player, enemies, projectiles, pickups, chests, gates, checkpoint, portal, torch, boss, GameCanvas
  Animations/     Hero clips + Hero.controller
  Tiles/          Tile assets generated from the sliced sheets
  Resources/      SfxLibrary.asset
  Scripts/Runtime TileVania.Runtime.asmdef — Core, Player, Combat, Enemies, Items, Level, UI, Boss, Story
  Scripts/Editor  TileVania.Editor.asmdef — ArtSetup, GeneratedArt, ContentBuilder, UIBuilder, LevelBuilder, GameBuildPipeline
  Tests/PlayMode  TileVania.Tests.PlayMode.asmdef — GameplayTests, ControlsAndLadderTests, StoryTests
Assets/_Legacy/   original course project (not in build; contains course-restricted art)
```

Key runtime systems: `GameInput` (input), `Layers` (physics layers and collision matrix, enforced at startup), `GameSession` (gems, deaths, time, keys), `LevelManager` (per-scene flow), `SceneLoader` (fades), `AudioManager`, `CameraController`, `Fx`, `HUD`, `GameMenus`, `DragonBoss`.

Story systems (`Scripts/Runtime/Story`):
- `Story`: cycle flags, `TryBegin(beat, requires)` for once-only and in-order beats, and the saved notch count.
- `StoryScript`: all story text, plus the layout-digit → beat table.
- `DialogueBox`, `StoryPoint` (auto, bark and readable beats), `MemoryShard`, `TallyMarks`, `Glint`.
- `StoryDirector`: opening, ending and loop. `CinematicOverlay` (UI): letterbox, fades and caption.

**Physics layers:** Ground, OneWay, Ladder, Player, Enemy, Hazards, Interactables, PlayerProjectile, EnemyProjectile. Player projectiles collide only with Ground and Enemy. Enemy projectiles collide only with Ground and Player. Pickups, portals and gates are on Interactables, which only the player touches.

### Rebuilding / editing levels
- **Tools → TileVania → Rebuild Game** regenerates art import settings, prefabs, UI and all four scenes from the layouts. It **overwrites** the generated scenes and prefabs, so make layout changes in the `.txt` files (or stop using the builder and edit the scenes by hand).
- **Tools → TileVania → Validate Scenes** checks for missing scripts and references, empty collider geometry, and the build list.
- **Tools → TileVania → Capture Preview Screenshots** writes `Docs/Screenshots/*.png`.
- **Tools → TileVania → Build Windows Player** builds to `Builds/Windows/`.

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
