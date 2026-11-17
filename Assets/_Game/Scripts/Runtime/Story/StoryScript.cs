using System;
using System.Collections.Generic;

namespace TileVania
{
    public enum LineStyle { Speech, Thought, Note, Voice, Memory }

    [Serializable]
    public struct DialogueLine
    {
        public string speaker;
        public string text;
        public LineStyle style;
        /// <summary>The line is interrupted: it closes by itself as soon as it has been typed.</summary>
        public bool cutOff;

        public DialogueLine(string speaker, string text, LineStyle style = LineStyle.Speech, bool cutOff = false)
        {
            this.speaker = speaker; this.text = text; this.style = style; this.cutOff = cutOff;
        }
    }

    /// <summary>How a story point in a level is triggered.</summary>
    public enum StoryPointKind
    {
        /// <summary>Walking into the zone plays a blocking dialogue (once per cycle).</summary>
        AutoDialogue,
        /// <summary>Walking into the zone shows a short thought that does not stop play.</summary>
        AutoBark,
        /// <summary>Shows an "Examine" prompt; the interact key plays the dialogue (can be re-read).</summary>
        Readable,
        /// <summary>A glowing memory shard pickup.</summary>
        MemoryShard,
    }

    /// <summary>The scenery the level builder creates for a story point.</summary>
    public enum StoryProp { None, NotchedTree, RustedLantern, FaintGhost, DoorScrawl, MemoryShard }

    public class StoryBeat
    {
        public string id;
        public StoryPointKind kind;
        public StoryProp prop;
        public string requires;
        public string prompt = "Examine";
        public DialogueLine[] lines;
    }

    /// <summary>
    /// Every line of story text in the game, plus the story points that the ASCII layouts place
    /// with the digits 1-9 (see <see cref="BeatForDigit"/>). Docs/STORY_SPEC.md explains the story.
    /// </summary>
    public static class StoryScript
    {
        public const string Hero = "???";
        public const string Keeper = "The Keeper";
        public const string Lia = "Lia";
        public const string Distant = "A distant voice";
        public const string Small = "A small voice";

        static DialogueLine Say(string who, string text) => new DialogueLine(who, text);
        static DialogueLine Think(string text) => new DialogueLine(Hero, text, LineStyle.Thought);
        static DialogueLine Voice(string who, string text) => new DialogueLine(who, text, LineStyle.Voice);
        static DialogueLine Note(string text) => new DialogueLine("Note", text, LineStyle.Note);
        static DialogueLine Memory(string text) => new DialogueLine("Memory", text, LineStyle.Memory);
        /// <summary>An interrupted line: it closes by itself as soon as it has been typed.</summary>
        static DialogueLine CutOff(string who, string text) => new DialogueLine(who, text, LineStyle.Speech, cutOff: true);

        // ------------------------------------------------------------------ forest: the awakening
        public static readonly DialogueLine[] OpeningVoice = { Voice(Distant, "...wait for me...") };

        public static readonly DialogueLine[] OpeningWake =
        {
            Say(Hero, "Where am I?"),
            Say(Hero, "I can't remember anything. Not even my name."),
        };

        public static readonly DialogueLine[] FirstGem =
        {
            Think("(It's warm. For a moment I heard someone laughing.)"),
            Think("(As if it holds a piece of something I've lost.)"),
        };

        /// <summary>The loop: the same tree, one more notch, and the hero no longer knows what the note means.</summary>
        public static readonly DialogueLine[] LoopNote =
        {
            Note("This time, don't open the chest."),
            Say(Hero, "What chest?"),
        };

        public const string FinalLine = "Somewhere, a lantern is still burning.";

        // ------------------------------------------------------------------ treasure chamber: the Keeper
        public static readonly DialogueLine[] BossIntro =
        {
            Say(Keeper, "So. You've come back."),
            Say(Hero, "You know me?"),
            Say(Keeper, "Better than you know yourself. Long ago, you gave me something to keep."),
            Say(Hero, "I don't remember."),
            Say(Keeper, "You weren't meant to. And you made me promise to stop you if you ever came back for it."),
            Say(Keeper, "I keep my promises."),
        };

        public static readonly DialogueLine[] BossRetry = { Say(Keeper, "Again, then.") };

        public static readonly DialogueLine[] BossDefeat =
        {
            Say(Keeper, "Enough. You've won. You always do."),
            Say(Hero, "What's inside that chest?"),
            Say(Keeper, "The memory you gave me. The one you begged me never to give back."),
            Say(Keeper, "Open it, and you'll remember why."),
        };

        // ------------------------------------------------------------------ the ending: the chest opens
        public static readonly DialogueLine[] EndingCall =
        {
            Voice(Small, "You said you'd be home by morning."),
            Say(Hero, "I know that voice..."),
        };

        // The flashback. MemoryFlashback plays these between its staging steps.

        /// <summary>Memory 1, leaving home: a poor cottage at night, a promise, and the lantern in the window.</summary>
        public static readonly DialogueLine[] MemoryLeaving =
        {
            Say(Lia, "Do you really have to go?"),
            Say(Hero, "There's no food left, Lia. The dragon's gold could feed us for years."),
            Say(Lia, "Will you be home by morning?"),
            Say(Hero, "I promise. I'll be back by morning, with enough gold for both of us."),
            Say(Lia, "Then I'll keep the lantern in the window, so you can find your way home."),
            Say(Hero, "Lock the door, and get some sleep."),
        };

        /// <summary>Memory 2, the last encounter: Lia follows with her lantern, and the hero sends her away.</summary>
        public static readonly DialogueLine[] MemoryFollowCall = { Say(Lia, "Wait! Wait for me!") };

        public static readonly DialogueLine[] MemoryFollowHarsh =
        {
            Say(Hero, "Lia? I told you to stay home!"),
            CutOff(Lia, "I just wanted to—"),
            Say(Hero, "Go home! I don't want you following me!"),
        };

        public static readonly DialogueLine[] MemoryFollowLast = { Think("(That was the last time I ever saw her.)") };

        /// <summary>Memory 3, returning home: dawn, an empty house, and a search that finds nothing.</summary>
        public static readonly DialogueLine[] MemoryReturnArrive = { Say(Hero, "Lia! I'm back! Look, we're rich!") };
        public static readonly DialogueLine[] MemoryReturnEmpty = { Say(Hero, "Lia?") };
        public static readonly DialogueLine[] MemoryReturnShout = { Say(Hero, "LIA!") };

        public static readonly DialogueLine[] MemoryReturnSearch =
        {
            Think("(I searched the forest for days. I called her name until my voice was gone.)"),
            Think("(She never came home.)"),
        };

        /// <summary>Memory 4, the bargain: the hero asks the Keeper to take the memory, and makes it promise.</summary>
        public static readonly DialogueLine[] MemoryBargain =
        {
            Say(Hero, "I can't stop hearing her voice."),
            Say(Keeper, "Your sister?"),
            Say(Hero, "I told her I didn't want her. Those were the last words she ever heard from me."),
            Say(Hero, "I've searched everywhere. I can't live with it. Take it away."),
            Say(Keeper, "I can take the memory, but not the guilt alone. You would forget her, and everything she was part of."),
            Say(Hero, "Then take everything."),
            Say(Keeper, "Part of you will come looking for it one day. What should I do then?"),
            Say(Hero, "Stop me. Promise you'll never give it back, no matter how I beg."),
            Say(Keeper, "I promise."),
        };

        // Back in the present: the hero remembers, and chooses to forget again.

        public static readonly DialogueLine[] EndingRemember =
        {
            Say(Hero, "Lia..."),
            Say(Hero, "I remember now. All of it."),
            Say(Keeper, "Then you know why you came to me."),
            Say(Keeper, "You could keep it this time. It would hurt. But she would be remembered."),
        };

        /// <summary>With every memory shard found, the hero hesitates: the cycle could end. They still choose to forget.</summary>
        public static readonly DialogueLine[] EndingHesitate =
        {
            Say(Hero, "If I keep it... maybe I could still find her."),
            Say(Keeper, "Maybe."),
        };

        /// <summary>The choice. The hero asks for the memory to be taken again; nothing forces them.</summary>
        public static readonly DialogueLine[] EndingChoice =
        {
            Say(Hero, "...I can't."),
            Say(Hero, "Take it back. Take all of it."),
        };

        public static readonly DialogueLine[] EndingKeeperAgrees = { Say(Keeper, "Then I will keep it for you. As I promised.") };
        public static readonly DialogueLine[] EndingApology = { Say(Hero, "I'm sorry, Lia.") };
        public static readonly DialogueLine[] EndingForget = { Think("(And again, I chose to forget.)") };
        public static readonly DialogueLine[] EndingWhisper = { Think("(Don't... open... the chest...)") };

        /// <summary>The lines after the flashback, up to the choice. The hesitation only plays when every shard was found.</summary>
        public static DialogueLine[] EndingDecision(bool allMemories)
        {
            var lines = new List<DialogueLine>(EndingRemember);
            if (allMemories) lines.AddRange(EndingHesitate);
            lines.AddRange(EndingChoice);
            return lines.ToArray();
        }

        // ------------------------------------------------------------------ story points placed by the layouts
        static readonly Dictionary<string, StoryBeat> beats = new Dictionary<string, StoryBeat>();

        static StoryBeat Add(StoryBeat b) { beats[b.id] = b; return b; }

        static StoryScript()
        {
            // Forest
            Add(new StoryBeat
            {
                id = "forest_tree", kind = StoryPointKind.AutoDialogue, prop = StoryProp.NotchedTree,
                lines = new[]
                {
                    Note("This time, don't open the chest."),
                    Say(Hero, "What chest? ...The handwriting looks familiar."),
                    Say(Hero, "Someone has carved notches into this tree. {Notches} of them."),
                    Say(Hero, "Most are old and grown over. The last one is fresh."),
                },
            });
            Add(new StoryBeat
            {
                id = "forest_lantern", kind = StoryPointKind.Readable, prop = StoryProp.RustedLantern, prompt = "Examine",
                lines = new[]
                {
                    new DialogueLine("", "A rusted lantern lies half-buried in the moss. Someone carried it a long way from home.", LineStyle.Thought),
                    Say(Hero, "Why does looking at it hurt?"),
                },
            });
            Add(new StoryBeat
            {
                id = "memory_1", kind = StoryPointKind.MemoryShard, prop = StoryProp.MemoryShard,
                lines = new[] { Memory("A small house at the edge of the forest. A lantern in the window."), Say(Hero, "Was that my home?") },
            });
            Add(new StoryBeat
            {
                id = "memory_2", kind = StoryPointKind.MemoryShard, prop = StoryProp.MemoryShard,
                lines = new[]
                {
                    Voice(Distant, "Wait! Wait for me!"),
                    Say(Hero, "A child's voice. I know it. Why can't I see her face?"),
                },
            });

            // Castle
            Add(new StoryBeat
            {
                id = "castle_dejavu", kind = StoryPointKind.AutoBark, prop = StoryProp.None,
                lines = new[] { Think("(I know this hall. I know where every step goes before I take it.)") },
            });
            Add(new StoryBeat
            {
                id = "castle_ghost", kind = StoryPointKind.AutoDialogue, prop = StoryProp.FaintGhost,
                lines = new[]
                {
                    Say("Faint Ghost", "Back again?"),
                    Say(Hero, "You know me? Then tell me who I am."),
                    Say("Faint Ghost", "I can't. You gave that away, down in the treasure chamber."),
                    Say("Faint Ghost", "You always hurry past us, as if someone is waiting for you at home."),
                },
            });
            Add(new StoryBeat
            {
                id = "castle_door", kind = StoryPointKind.AutoDialogue, prop = StoryProp.DoorScrawl, requires = "castle_ghost",
                lines = new[]
                {
                    Say(Hero, "\"I was so close.\" It's the same handwriting as the note."),
                    Say(Hero, "And the same marks as the tree. {Notches} of them."),
                    Say(Hero, "I've stood at this door {notches} times before. Why can't I remember what's behind it?"),
                },
            });
            Add(new StoryBeat
            {
                id = "memory_3", kind = StoryPointKind.MemoryShard, prop = StoryProp.MemoryShard,
                lines = new[] { Memory("\"I'll be back by morning, with enough gold for both of us.\""), Say(Hero, "I said that. But to whom?") },
            });
            Add(new StoryBeat
            {
                id = "memory_4", kind = StoryPointKind.MemoryShard, prop = StoryProp.MemoryShard,
                lines = new[] { Memory("Calling a name through the trees until my voice was gone."), Say(Hero, "What name?") },
            });
        }

        public static StoryBeat Beat(string id) => id != null && beats.TryGetValue(id, out var b) ? b : null;

        public static IEnumerable<StoryBeat> AllBeats => beats.Values;

        /// <summary>
        /// Story markers in the ASCII layouts: digits 1-9, numbered per level in the order the player meets them.
        /// </summary>
        public static StoryBeat BeatForDigit(string scene, char digit)
        {
            string id = null;
            if (scene == SceneLoader.Level1)
                id = digit switch { '1' => "forest_tree", '2' => "forest_lantern", '3' => "memory_1", '4' => "memory_2", _ => null };
            else if (scene == SceneLoader.Level2)
                id = digit switch
                {
                    '1' => "castle_dejavu", '2' => "castle_ghost", '3' => "castle_door", '4' => "memory_3", '5' => "memory_4", _ => null,
                };
            return Beat(id);
        }
    }
}
