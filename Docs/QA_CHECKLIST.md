# QA Checklist: *The Last Memory* (manual play-through)

Automated coverage: Play Mode tests in `Assets/_Game/Tests/PlayMode/` (`GameplayTests.cs`, `ControlsAndLadderTests.cs`, `StoryTests.cs`). Run them via **Window → General → Test Runner → PlayMode → Run All** (`FlashbackScreenshots` is opt-in and only captures images). The items below are the things automation cannot judge: feel, readability, audio and visuals. Tick them while playing at 1920×1080 (Game view "Full HD"), then at one other resolution.

## Setup
- [ ] Open `Assets/_Game/Scenes/MainMenu.unity` and press Play (or run `Builds/TheLastMemory_Windows/The Last Memory.exe`).
- [ ] The window title and the menu title read **The Last Memory**.
- [ ] The Console shows no errors or warnings on entering Play Mode.

## Main menu
- [ ] Music starts (*Perilous Dungeon*). The title *The Last Memory*, the tagline *"Every gem is something you have forgotten."*, the diorama, the control hint and four buttons are visible.
- [ ] Arrow keys move between buttons, with a highlight and a select sound; Enter activates.
- [ ] The mouse hover highlight works, and clicking works.
- [ ] **Settings:** both sliders change volume immediately, and the values persist after restarting Play Mode.
- [ ] **Credits:** the text fits inside the panel and Back returns to the menu.
- [ ] **Quit** stops Play Mode (in a build, the app closes).
- [ ] **Play** fades to black, then into the forest's opening cutscene (see *Story* below).

## Controls (keyboard only, no mouse)
- [ ] ← / → move; ↑, Z and Space each jump; X shoots; Esc pauses. The mouse does nothing in gameplay.
- [ ] **C fires straight up** every time: standing, running, during a jump, and right after landing. It never makes the hero jump.
- [ ] Shooting upward at bats and at the Keeper during Fire Rain feels reliable (the main reason for C).
- [ ] ↑ + X (pressed together) still fires upward without jumping.
- [ ] ↑ next to a ladder climbs it instead of jumping. ↑ away from ladders jumps.
- [ ] ↓ + Z on a wooden ledge drops through it. ↓ alone does not.
- [ ] Gamepad (if available): stick moves and climbs, South jumps, West shoots, right shoulder shoots up, North examines.

## Ladders (both directions)
- [ ] Level 1: standing on top of the shaft into the underground (x≈78), ↓ climbs down into the cave, even when not perfectly centred.
- [ ] Stopping midway holds position; ↑ climbs back out at the top onto the ground.
- [ ] Level 1 ruins ladder (x≈45): up onto the key ledge and back down.
- [ ] Level 2 watchtower: climbing **down** the upper ladder stops on the wooden floor at its foot (no falling through).
- [ ] ↓ next to (not on) a ladder foot does not snap you onto it.

## Player feel (Level 1 meadow)
- [ ] Running starts and stops crisply, with no sliding or sticking to walls.
- [ ] A tap-jump is short and a held jump is high (about 3.5 tiles). You can still jump just after leaving a ledge (coyote time), and a jump pressed just before landing still fires (buffer).
- [ ] Landing makes dust, a squash and a thud sound. Jumping makes dust and a stretch.
- [ ] Facing always matches movement. Shots leave from the side you face, never from inside the hero.
- [ ] Shooting point-blank into a wall makes a spark, and no projectile passes through.
- [ ] The camera follows smoothly with look-ahead, never shows outside the level, and pixels stay crisp.
- [ ] **No thin lines between terrain tiles** while running and jumping, at 1920×1080, 1366×768 and a small window (e.g. 960×540). Look especially at large dirt areas in the forest and the castle floors.

## Combat and hazards
- [ ] The slime takes 2 hits, flashes red on the first, and bursts with a sound on death.
- [ ] The skeleton patrols and turns at ledges and walls. It dies to 3 hits **from either side**.
- [ ] The ghost flashes violet before firing, and its bolt travels toward you.
- [ ] Touching an enemy or spikes costs **one** heart, knocks you back, and makes you blink. You can't be hit again while blinking.
- [ ] Falling into the chasm (Level 1, x≈63–66) costs a heart and fades you back to the last checkpoint.
- [ ] Losing all hearts plays the death fall, then **You Fell** with Retry and Main Menu. Retry restores 3 hearts at the checkpoint, and defeated enemies stay defeated.
- [ ] Projectiles never hit pickups, ladders, gates' trigger zones or the portal (shoot the portal: the level must not change).

## Level 1 progression
- [ ] Checkpoint pillars light a flame with a glow, play a sound, show "Checkpoint", and refill hearts.
- [ ] The Gold Key pickup shows a toast and a HUD key icon.
- [ ] In the cave: the spider bobs on its thread; the iron chest opens with an animation, and gems fly to you.
- [ ] The gate without the key shows a "Locked" toast and sound. With the key it rises and fades, and the HUD key icon disappears.
- [ ] The portal plays its opening animation as you approach. Entering it shrinks the hero, fades out, and loads the Castle.

## Level 2 — Castle
- [ ] The palette is darker; torches flicker and light the walls; the sky shows above the battlements.
- [ ] The spike-hall bridges and pillar jumps are all doable without taking damage.
- [ ] Bats fly predictably; the tower ladders reach the Gold Key.
- [ ] The Gold Gate opens; the spike gallery is crossable on the bridges.
- [ ] The Treasure Key ledge is reached by its ladder. The Treasure Door refuses the Gold Key and opens only with the Treasure Key.
- [ ] Music is *Spooky Dungeon*. The portal leads to the Treasure Chamber.

## Boss — Treasure Chamber
- [ ] Walking into the hall closes the portcullis behind you, starts the boss music, and brings up the health bar (**The Keeper**). The dragon speaks before attacking.
- [ ] Every attack is clearly telegraphed (flash and shake, red floor markers) before it can hurt you.
- [ ] Fire Volley: fireballs fan out toward you, and the dragon can be hit with straight shots from the side platforms.
- [ ] Dive Slam: the marker appears under you, the slam shakes the screen, and the dragon can be punished on the floor.
- [ ] Fire Rain: the markers are readable and one lane is always safe.
- [ ] Below half health: an enraged tint and message, faster pacing, and shockwaves after slams.
- [ ] Dying mid-fight then pressing Retry fully resets the boss (full bar), reopens the door and restores the pre-fight music.
- [ ] Defeat: explosions, the dragon falls and fades, music fades out, and the gold chest appears.
- [ ] Opening the chest starts the ending (see *Story* above) rather than a victory screen.
- [ ] Difficulty feels fair for a short game. Note the number of attempts and the time taken.

## Story: The Last Memory (see `Docs/STORY_SPEC.md`)
Play once from **Play** in the main menu, without skipping. Note anything that feels too long, unclear or badly timed.

**Opening and forest**
- [ ] The opening starts black with letterbox bars. "...wait for me..." appears, then it fades in on the hero lying under the old tree, who stands up and speaks two lines. The title card *The Whispering Forest: The Awakening* follows, and control returns.
- [ ] Esc does nothing during the opening or any dialogue. Z / Space / X / C / Enter finish a line, then advance. The key that closes a dialogue doesn't also jump or shoot.
- [ ] Walking right, the tree dialogue plays once: the note "This time, don't open the chest.", then the notches. The number spoken matches the tally marks on the trunk, and the marks are readable at normal zoom.
- [ ] The first gem shows a short thought at the bottom without stopping play.
- [ ] The rusted lantern by the ruined gateway glints. Standing next to it shows "Enter  Examine", and Enter plays its lines. C near it shoots upward instead.
- [ ] Both forest memory shards can be reached (the ledge east of the ruins; the high ledge on the surface east of the cave entrance). Picking one up flashes the screen briefly and shows a memory. Restarting the level doesn't bring a found shard back.

**Castle**
- [ ] The déjà-vu thought appears near the start without stopping play.
- [ ] The Faint Ghost (after the broken bridge) speaks four lines and then fades away. It never hurts the hero.
- [ ] By the treasure door, "I was so close." and the tally marks are visible, and the hero says the matching count.
- [ ] Both castle shards (the top of the watchtower; past the exit portal at the far east wall) can be reached.

**Treasure chamber and ending**
- [ ] After the portcullis falls and the dragon swoops in, the world is frozen while it speaks. No attack hits during the conversation. The fight starts right after, and the boss bar reads **The Keeper**.
- [ ] Die and retry: the long conversation doesn't repeat; only "Again, then." appears.
- [ ] On defeat, the Keeper's four last lines play, then it sinks to the floor by the right wall, grey and wounded, and **stays there**. It can't hurt you. Then the gold chest appears.
- [ ] Opening the chest: the light swells, Lia's small dark figure with a lantern appears, *"You said you'd be home by morning."*, then a white flash.
- [ ] **Flashback** (read at your own pace; lines also move on by themselves). The HUD is hidden and the letterbox stays on.
  - Memory 1, the cottage at night: door open, window lit, Lia in the doorway. Five lines; the hero walks off toward the castle; Lia goes inside.
  - Memory 2: the window close-up (Lia with the lantern; then the lantern is gone and the room goes dark), then the forest path with the castle on the horizon. Lia runs after the hero; the harsh exchange; the hero walks away; Lia is left alone; *"(That was the last time I ever saw her.)"*
  - Memory 3, dawn: the hero returns with a sack, goes inside, comes out, drops the sack (gold spills), runs to the forest edge and calls her name.
  - Memory 4, the same hall in faded warm light: the Keeper unhurt, no chest; the bargain and *"I promise."*
  - Nothing shows what happened to Lia.
  - One key press never skips more than one line, and never skips a whole memory.
- [ ] Back in the present: *"Lia..."*, the Keeper's offer, *"Take it back. Take all of it."* The hero turns to the Keeper, which rises; *"I'm sorry, Lia."*; the figure dissolves, gems scatter and the counter drains to 0; white-out; *"(And again, I chose to forget.)"*; black, a knock, the whisper.
- [ ] Find all four shards in one cycle and finish it: the hero hesitates (*"If I keep it... maybe I could still find her."* / *"Maybe."*) before choosing.
- [ ] Do the story beats make sense to a first-time player? Is it clear why the hero feels guilty, what the bargain was, and that forgetting is the hero's own choice? Note anything unclear.
- [ ] Is the flashback's length comfortable (about 70–100 s)? Note any line that feels too long or too fast.
- [ ] The flashback looks right at 16:9 and at a narrower window (e.g. 1280×960): characters and the cottage stay in frame.
- [ ] The loop:
  - The forest opening repeats word for word.
  - The hero walks to the tree, and the newest notch glows. **There is one more notch than before.**
  - "What chest?", then the caption *"Somewhere, a lantern is still burning."*, then the panel **The Last Memory** with sensible stats.
- [ ] **Begin Again** gives control in the forest, with music and the title card. **Main Menu** works.
- [ ] Quit, restart the game and play to the tree: the notch count is still the higher number (it is saved).
- [ ] Overall pacing: the story never interrupts a jump or a fight, and no dialogue feels too long.

## Pause and UI
- [ ] Esc pauses (gameplay freezes) and Esc again resumes. Pause doesn't open over the death or ending panels, or during dialogue and cutscenes.
- [ ] Restart Level reloads the level and discards gems collected in it. Main Menu works from pause.
- [ ] The HUD stays readable at 1280×720 and at 2560×1440.

## Audio
- [ ] Every action has a sound: jump, land, shoot, gem, key, chest, gate, locked, hit, enemy death, hurt, death, checkpoint, portal, UI, boss warning, boss hit and boss death.
- [ ] No sound is too loud or clipping. Swap any you dislike in `Assets/_Game/Resources/SfxLibrary.asset`.
- [ ] Music cross-fades between scenes, and the Music slider affects music only.
