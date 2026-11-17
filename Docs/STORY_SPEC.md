# Story Specification: *The Last Memory*

This is the narrative of **The Last Memory**. It is inspired by the torment of Tantalus: something you want badly stays within reach, and you can never hold it. The myth itself is not retold. The game keeps its psychological core: **the hero cannot stop reaching for the memory, and cannot bear to hold it.**

Every line of dialogue lives in one file, `Assets/_Game/Scripts/Runtime/Story/StoryScript.cs`. This document explains what the lines mean and when they play.

---

## 1. What really happened

The player learns all of this by the end of the game: from the clues in the levels, and from the flashback when the chest opens.

1. **Before the game.** The hero lived with their little sister, **Lia**, in a small cottage at the edge of the forest. They were poor; there was no food left. An old castle nearby held a dragon's gold.
2. **The promise.** One night the hero set out to steal the gold. Lia asked if they would be home by morning. The hero promised: *"I'll be back by morning, with enough gold for both of us."* Lia said she would keep a lantern in the window so they could find their way home.
3. **The last encounter.** Lia was afraid for them. She took the lantern and followed them into the forest, calling *"Wait! Wait for me!"* The hero, impatient and worried about being followed, turned and shouted: *"Go home! I don't want you following me!"* Then they walked on toward the castle. **That was the last time the hero ever saw her.**
4. **The empty house.** At dawn the hero came home with the gold. The house was empty and the lantern was gone. The hero searched the forest for days and called her name until their voice was gone. Lia was never found. **What happened to her is never revealed.** She is not stated to be dead.
5. **The bargain.** The hero could not live with their last words to her. They went back to the treasure chamber and asked its creature, **the Keeper**, to take the memory away. The Keeper explained the cost: it can take a memory, but not the guilt alone; the hero would forget Lia and everything she was part of. The hero said: *"Then take everything."* The Keeper asked what it should do when part of them came back looking for it. The hero made it promise: *"Promise you'll never give it back, no matter how I beg."* The Keeper promised.
6. **Why everything else went too.** Lia was part of the hero's home, childhood and name. Taking her away unravelled almost everything. The lost memories became the scattered **gems**.
7. **The cycle.**
   - Forgetting does not remove the *shape* of what is missing. The hero feels a hole and is pulled back to the treasure chamber without knowing why.
   - The Keeper fights them every time, because it promised to.
   - When the hero wins and opens the chest, the memory returns, all of it.
   - The Keeper offers to let them keep it this time. The hero **chooses** to give it up again: *"Take it back. Take all of it."* There is no curse and no villain. **The hero is the one who erases the memory, every time.**
8. **The fading self.** As the hero's mind empties, their fading self stumbles back the way it came. It scratches *"I was so close."* and a tally beside the treasure door, carves one more notch into the old tree in the forest, and long ago it pinned a note to that tree: *"This time, don't open the chest."* Then the hero falls asleep under the tree and wakes up with nothing.
9. **The count.** There were **seven** cycles before the player's first game. Each completed game adds one more notch. The count is saved between play sessions.

### The rules the story obeys
- **Gems are fragments of memory.** When the memory is given up again, every fragment scatters across the forest and the castle. That is why gems are everywhere, and why the hero collects them again each cycle.
- **The Keeper is not evil.** It keeps the promise the hero asked for. It fights to stop them, offers them the choice when it has lost, and takes the memory back when they ask. It is tired of the cycle but will not break its word.
- **The cycle could end.** The hero would have to choose to keep the memory, grief included. The game never shows this. The final line leaves that hope open.
- **No narrator, no companion.** Every line belongs to the hero (`???`), Lia, the Keeper, a ghost, a written note, or a remembered voice.
- **Lia's fate stays unknown.** Nothing in the game says where she went or whether she is alive.

---

## 2. What the player experiences

| Level | Chapter subtitle | Purpose |
|---|---|---|
| Level 1: The Whispering Forest | **The Awakening** | Amnesia, the note, the notches, gems as memories |
| Level 2: The Abandoned Castle | **The Forgotten Past** | Déjà vu, a ghost who recognises the hero, proof they reached the door before |
| Level 3: The Treasure Chamber | **The Last Memory** | The Keeper, the chest, the flashback, the choice, the loop |

The hero's speaker name is **`???`** throughout, even inside the memories: their own name went with Lia.

### 2.1 Forest: The Awakening
- **Opening (cinematic).** Black screen with letterbox bars. A distant voice: *"...wait for me..."* (Lia's last words to the hero, though the player cannot know it yet). The view fades in on the hero lying under an old tree. They get up.
  - ???: *Where am I?*
  - ???: *I can't remember anything. Not even my name.*
  - The title card appears and the player has control.
- **The Notched Tree** (automatic, three steps to the right). A note is pinned below carved tally marks.
  - Note: *This time, don't open the chest.*
  - ???: *What chest? ...The handwriting looks familiar.*
  - ???: *Someone has carved notches into this tree. Seven of them.* (The number matches the real notch count.)
  - ???: *Most are old and grown over. The last one is fresh.*
- **First gem** (a thought that doesn't stop play): *(It's warm. For a moment I heard someone laughing.)* / *(As if it holds a piece of something I've lost.)*
- **The Rusted Lantern** (optional, **Enter** to examine, by the ruined gateway).
  - *A rusted lantern lies half-buried in the moss. Someone carried it a long way from home.*
  - ???: *Why does looking at it hurt?*
- **Memory shards:** two in the forest (§3).

### 2.2 Castle: The Forgotten Past
- **Déjà vu** (a thought): *(I know this hall. I know where every step goes before I take it.)*
- **The Faint Ghost** (automatic, harmless):
  - Faint Ghost: *Back again?*
  - ???: *You know me? Then tell me who I am.*
  - Faint Ghost: *I can't. You gave that away, down in the treasure chamber.*
  - Faint Ghost: *You always hurry past us, as if someone is waiting for you at home.*
  - Then it fades away.
- **The Treasure Door** (automatic, only after the ghost). Scratched into the wall: ***"I was so close."*** with a tally.
  - ???: *"I was so close." It's the same handwriting as the note.*
  - ???: *And the same marks as the tree. Seven of them.*
  - ???: *I've stood at this door seven times before. Why can't I remember what's behind it?*
- **Memory shards:** two in the castle.

### 2.3 Treasure Chamber: the Keeper
- **Before the fight** (the world is frozen while it speaks):
  - The Keeper: *So. You've come back.*
  - ???: *You know me?*
  - The Keeper: *Better than you know yourself. Long ago, you gave me something to keep.*
  - ???: *I don't remember.*
  - The Keeper: *You weren't meant to. And you made me promise to stop you if you ever came back for it.*
  - The Keeper: *I keep my promises.*
- **On a retry** (does not stop play): The Keeper: *Again, then.*
- **After its defeat** (frozen):
  - The Keeper: *Enough. You've won. You always do.*
  - ???: *What's inside that chest?*
  - The Keeper: *The memory you gave me. The one you begged me never to give back.*
  - The Keeper: *Open it, and you'll remember why.*
  - The Keeper sinks to the floor by the right wall, wounded and grey, and **stays there**, watching. The gold chest appears.

### 2.4 The ending: the chest, the flashback and the choice
**At the chest.** The music stops and the chest's light fills the hall. A small dark figure with a lit lantern stands in it.
- A small voice: *You said you'd be home by morning.*
- ???: *I know that voice...*

The screen flashes white and the **flashback** begins (`MemoryFlashback`). It shows four short memories, staged with small pixel-art scenes, the two characters, lanterns and lighting. It takes roughly 70–100 seconds, depending on reading speed. The HUD is hidden.

**Memory 1: Leaving home** (the cottage at night; the door is open and the room is lit)
- Lia: *Do you really have to go?*
- ???: *There's no food left, Lia. The dragon's gold could feed us for years.*
- Lia: *Will you be home by morning?*
- ???: *I promise. I'll be back by morning, with enough gold for both of us.*
- Lia: *Then I'll keep the lantern in the window, so you can find your way home.*
- The hero steps close to her for a moment, then walks off toward the castle. Lia watches, then goes inside, and the door closes.

**Memory 2: The last encounter**
- *(No words.)* Close on the cottage window: Lia waits beside the lantern. Then the lantern is taken from the window, the room goes dark, and Lia is gone from it.
- The forest path at night, the castle on the horizon. The hero walks toward it. Lia runs after them with the lantern:
  - Lia: *Wait! Wait for me!*
  - The hero stops and turns around.
  - ???: *Lia? I told you to stay home!*
  - Lia: *I just wanted to—* (cut off)
  - ???: *Go home! I don't want you following me!*
- The hero turns away and walks on, out of sight. Lia stays where she is and lowers her lantern. The camera drifts to her, alone in the dark forest.
  - ???: *(That was the last time I ever saw her.)*
- What happens to Lia next is **not shown**.

**Memory 3: Returning home** (the same cottage at dawn: pale sky, dark window, no lantern)
- The hero comes back with a sack of gold. ???: *Lia! I'm back! Look, we're rich!*
- They go inside, and come back out. ???: *Lia?*
- The sack drops; the gold spills, forgotten. They run to the edge of the forest. ???: *LIA!*
- ???: *(I searched the forest for days. I called her name until my voice was gone.)*
- ???: *(She never came home.)*

**Memory 4: The bargain** (this same hall, long ago, in warm faded light; no chest, the Keeper unhurt)
- The hero walks in slowly and stops before the Keeper, without raising a weapon.
- ???: *I can't stop hearing her voice.*
- The Keeper: *Your sister?*
- ???: *I told her I didn't want her. Those were the last words she ever heard from me.*
- ???: *I've searched everywhere. I can't live with it. Take it away.*
- The Keeper: *I can take the memory, but not the guilt alone. You would forget her, and everything she was part of.*
- ???: *Then take everything.*
- The Keeper: *Part of you will come looking for it one day. What should I do then?*
- ???: *Stop me. Promise you'll never give it back, no matter how I beg.*
- The Keeper: *I promise.*
- A pale burst of light leaves the hero. White-out.

**The present: the choice.** The light fades back to the hall. The hero stands before the chest; Lia's figure still stands in the light; the wounded Keeper watches.
- ???: *Lia...* (the first time the hero says her name)
- ???: *I remember now. All of it.*
- The Keeper: *Then you know why you came to me.*
- The Keeper: *You could keep it this time. It would hurt. But she would be remembered.*
- *If all four memory shards were found this cycle*, the hero hesitates:
  - ???: *If I keep it... maybe I could still find her.*
  - The Keeper: *Maybe.*
- ???: *...I can't.*
- ???: *Take it back. Take all of it.*
- **The hero turns away from Lia's figure to face the Keeper.** The Keeper rises from the floor, its aura brightening.
- The Keeper: *Then I will keep it for you. As I promised.*
- The hero turns back to the figure for one last look. ???: *I'm sorry, Lia.*
- Lia's figure dissolves. Every gem bursts out of the chest and scatters; the HUD gem counter drains to zero. White-out.
- ???: *(And again, I chose to forget.)*
- Black. One soft knock, like a knife cutting bark: one more notch.
- ???: *(Don't... open... the chest...)* The fading self's last thought, which becomes the note on the tree.

### 2.5 The loop
The game returns to the forest and **the opening plays again, word for word**. Then:
1. The hero walks to the tree. The newest notch (now **eight**) glows faintly.
2. Note: *This time, don't open the chest.*
3. ???: *What chest?*
4. Fade to black. Final line, centred: ***Somewhere, a lantern is still burning.***
5. The ending panel: **The Last Memory**, *The cycle begins again.* Stats: gems, memories glimpsed (x of 4), time, deaths, notches on the tree. Buttons: **Begin Again** (continue the new cycle from the forest) or **Main Menu**.

The hero doesn't know there used to be seven notches; the player does.

---

## 3. Memory shards (optional)

Pale, glowing shards placed off the main path. Picking one up briefly freezes the world and shows one remembered line plus the hero's reaction. Each is worth 3 gems. Each one is a piece of the flashback, so the ending "clicks" for players who found them.

| # | Where | Memory | Hero | Seen again in |
|---|---|---|---|---|
| 1 | Forest: floating ledge east of the ruins | *A small house at the edge of the forest. A lantern in the window.* | *Was that my home?* | Memory 1 and 2 |
| 2 | Forest: high surface east of the cave entrance | A distant voice: *Wait! Wait for me!* | *A child's voice. I know it. Why can't I see her face?* | Memory 2 |
| 3 | Castle: top of the watchtower | *"I'll be back by morning, with enough gold for both of us."* | *I said that. But to whom?* | Memory 1 |
| 4 | Castle: past the exit portal, east wall | *Calling a name through the trees until my voice was gone.* | *What name?* | Memory 3 |

Finding all four adds the hero's moment of hesitation before the choice.

## 4. How the ending recontextualises the clues

| Clue | First reading | After the ending |
|---|---|---|
| "...wait for me..." | A dream | Lia's call in the forest, just before the hero's last words to her |
| Gems | Treasure, "a piece of something lost" | The hero's own memories, scattered each time they give Lia up |
| The note | A warning from a stranger | Written by the hero's fading self |
| The notches | Someone keeps coming back | The hero, once per cycle; the player sees one added |
| The rusted lantern | Junk | Lia's lantern, carried "a long way from home" into the forest |
| The ghost: "You gave that away" | Atmosphere | The bargain with the Keeper |
| The ghost: "someone is waiting for you at home" | Atmosphere | Lia, and the lantern in the window |
| The Keeper: "you made me promise to stop you" | Villain talk | The hero's own request, shown in Memory 4 |
| "Somewhere, a lantern is still burning." | — | Hope: Lia's fate is unknown, and the memory still waits |

## 5. Narrative decisions
- **Lia's fate is never resolved.** The flashback stops on her standing alone with her lantern; Memory 3 shows only the empty house and the search.
- **The guilt is specific.** The hero was not cruel on purpose: they were impatient and worried about being followed. But those were their last words to her, and they never checked that she got home.
- **The Keeper is a promise-keeper, not a villain.** It warns the hero about the cost, asks what to do when they come back, keeps its promise by fighting, offers the choice when it loses, and agrees when the hero chooses to forget.
- **The decision is shown, not implied.** The hero says "Take it back", turns to the Keeper, and apologises to Lia. The final thought, *"And again, I chose to forget"*, removes any doubt that it was a choice.
- **The Keeper stays in the hall after its defeat** (it used to vanish), so it can take part in the ending. Its fight and mechanics are unchanged.
- **The notch count is saved between play sessions** in PlayerPrefs. It starts at 7, and each finished game adds one.
- **"Begin Again" continues the new cycle**, so the loop is literal.

## 6. Interaction and pacing rules
- Level dialogue is short (at most six lines). Blocking dialogue freezes the world, so nothing can hurt the hero while text is on screen.
- Thoughts that don't stop play never take control away.
- **Keys:** Z / Space / X / C / Enter advance dialogue (the first press finishes the line being typed, the next one advances). **Enter** examines objects. Esc is ignored during dialogue and cutscenes.
- In cutscenes, lines also move on by themselves after a reading pause, so the ending plays even without input. One key press moves on by at most one line; there is no "skip the whole scene" key, so a press can never skip a memory.
- Each story beat fires once per cycle, and some need earlier ones: the castle door needs the ghost, the Keeper's last words need its intro, the ending needs the defeat. Shards found this cycle don't come back. Story state resets only when a new cycle starts.
