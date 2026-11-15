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
        public const string Dragon = "Dragon";
        public const string Distant = "A distant voice";
        public const string Small = "A small voice";

        static DialogueLine Say(string who, string text) => new DialogueLine(who, text);
        static DialogueLine Think(string text) => new DialogueLine(Hero, text, LineStyle.Thought);
        static DialogueLine Voice(string who, string text) => new DialogueLine(who, text, LineStyle.Voice);
        static DialogueLine Note(string text) => new DialogueLine("Note", text, LineStyle.Note);
        static DialogueLine Memory(string text) => new DialogueLine("Memory", text, LineStyle.Memory);

        // ------------------------------------------------------------------ forest: the awakening
        public static readonly DialogueLine[] OpeningVoice = { Voice(Distant, "...wait for me...") };

        public static readonly DialogueLine[] OpeningWake =
        {
            Say(Hero, "Where... am I?"),
            Say(Hero, "I can't remember anything. Not even my name."),
        };

        public static readonly DialogueLine[] FirstGem =
        {
            Think("(It's warm. For a heartbeat I heard someone laughing.)"),
            Think("(As if it holds a piece of something I've lost.)"),
        };

        /// <summary>The loop: the same tree, one more notch, and the hero no longer knows what the note means.</summary>
        public static readonly DialogueLine[] LoopNote =
        {
            Note("This time, don't open the chest."),
            Say(Hero, "What chest?"),
        };

        public const string FinalLine = "Somewhere, a lantern is still burning.";

        // ------------------------------------------------------------------ treasure chamber: the last memory
        public static readonly DialogueLine[] BossIntro =
        {
            Say(Dragon, "So. You've come back."),
            Say(Hero, "You know me?"),
            Say(Dragon, "Better than you know yourself. You know how this ends."),
            Say(Hero, "Then why am I here?"),
            Say(Dragon, "Because you never remember the answer."),
            Say(Dragon, "I made you a promise once. I will keep it again."),
        };

        public static readonly DialogueLine[] BossRetry = { Say(Dragon, "Again, then.") };

        public static readonly DialogueLine[] BossDefeat =
        {
            Say(Dragon, "Enough... You have won. You always do."),
            Say(Hero, "What's inside that chest?"),
            Say(Dragon, "What you gave me to keep. What you begged me never to give back."),
            Say(Dragon, "Take it, if you can bear it. You never can."),
        };

        public static readonly DialogueLine[] EndingReunion =
        {
            Voice(Small, "You said you'd be home by morning."),
            Say(Hero, "I know you... I know you!"),
            Voice(Small, "I kept the lantern burning, so you could find your way home."),
        };

        /// <summary>The name never arrives. With every memory shard found, the hero gets one syllable further.</summary>
        public static DialogueLine[] EndingName(bool allMemories) => new[]
        {
            new DialogueLine(Hero, allMemories ? "Your name is... Li—" : "Your name is... your name is—", LineStyle.Speech, cutOff: true),
        };

        public static readonly DialogueLine[] EndingLoss = { Say(Hero, "No... not again...") };
        public static readonly DialogueLine[] EndingWhisper = { Think("(Don't... open... the chest...)") };

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
                    Say(Hero, "Some are old and grown over. The last one is fresh."),
                },
            });
            Add(new StoryBeat
            {
                id = "forest_lantern", kind = StoryPointKind.Readable, prop = StoryProp.RustedLantern, prompt = "Examine",
                lines = new[]
                {
                    new DialogueLine("", "A rusted lantern lies half-buried in the moss. Someone carried it a long way.", LineStyle.Thought),
                    Say(Hero, "Why does looking at it hurt?"),
                },
            });
            Add(new StoryBeat
            {
                id = "memory_1", kind = StoryPointKind.MemoryShard, prop = StoryProp.MemoryShard,
                lines = new[] { Memory("A small house at the edge of a forest. A lantern in the window."), Say(Hero, "Home? Was it mine?") },
            });
            Add(new StoryBeat
            {
                id = "memory_2", kind = StoryPointKind.MemoryShard, prop = StoryProp.MemoryShard,
                lines = new[]
                {
                    Voice(Distant, "Wait for me! You're walking too fast!"),
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
                    Say("Faint Ghost", "Nobody here remembers that anymore. Not even you."),
                    Say("Faint Ghost", "You always rush past us, as if someone is waiting for you."),
                },
            });
            Add(new StoryBeat
            {
                id = "castle_door", kind = StoryPointKind.AutoDialogue, prop = StoryProp.DoorScrawl, requires = "castle_ghost",
                lines = new[]
                {
                    Say(Hero, "\"I was so close.\" ...It's the same handwriting as the note."),
                    Say(Hero, "And the same marks as the tree. {Notches} of them."),
                    Say(Hero, "I've stood at this door before. {Notches} times."),
                },
            });
            Add(new StoryBeat
            {
                id = "memory_3", kind = StoryPointKind.MemoryShard, prop = StoryProp.MemoryShard,
                lines = new[] { Memory("\"I'll be back by morning. With enough gold for both of us.\""), Say(Hero, "I said that. But to whom?") },
            });
            Add(new StoryBeat
            {
                id = "memory_4", kind = StoryPointKind.MemoryShard, prop = StoryProp.MemoryShard,
                lines = new[] { Memory("Calling a name through the trees until my voice was gone."), Say(Hero, "...What name?") },
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
