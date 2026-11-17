using System.Collections.Generic;
using UnityEngine;

namespace LastMemory
{
    /// <summary>
    /// Narrative state for the current cycle (one play-through from waking in the forest to the chest).
    ///  - Story beats are flags that fire once per cycle; <see cref="TryBegin"/> also enforces prerequisites.
    ///  - The number of completed cycles is saved in PlayerPrefs; the notched tree shows
    ///    <see cref="NotchesBeforeFirstPlay"/> + that number.
    /// See Docs/STORY_SPEC.md for the story itself.
    /// </summary>
    public static class Story
    {
        public enum Opening { None, FirstWaking, Loop }

        /// <summary>What the player saw in the cycle that just ended (shown on the ending panel).</summary>
        public struct CycleSummary
        {
            public int gems, memories, deaths, notches;
            public float time;
        }

        public const int NotchesBeforeFirstPlay = 7;
        public const int TotalMemories = 4;
        const string CyclesKey = "story_completed_cycles";

        static readonly HashSet<string> flags = new HashSet<string>();

        /// <summary>Set by the main menu (first waking) or the ending (loop); consumed by the forest's StoryDirector.</summary>
        public static Opening PendingOpening;
        public static CycleSummary LastCycle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            flags.Clear();
            PendingOpening = Opening.None;
            LastCycle = default;
        }

        public static int CompletedCycles => PlayerPrefs.GetInt(CyclesKey, 0);

        /// <summary>Tests use this to restore the saved count after playing through the ending.</summary>
        public static void SetCompletedCycles(int value) => PlayerPrefs.SetInt(CyclesKey, Mathf.Max(0, value));

        public static int NotchCount => NotchesBeforeFirstPlay + CompletedCycles;

        public static int MemoriesFound
        {
            get
            {
                int n = 0;
                foreach (var f in flags) if (f.StartsWith("memory_")) n++;
                return n;
            }
        }

        public static bool Has(string flag) => flags.Contains(flag);

        /// <summary>Marks a flag; returns true the first time.</summary>
        public static bool Set(string flag) => flags.Add(flag);

        /// <summary>True if the beat has not fired yet this cycle and its prerequisite (if any) has.</summary>
        public static bool CanBegin(string beat, string requires = null) =>
            !flags.Contains(beat) && (string.IsNullOrEmpty(requires) || flags.Contains(requires));

        /// <summary>Fires a beat exactly once per cycle, and never before its prerequisite.</summary>
        public static bool TryBegin(string beat, string requires = null)
        {
            if (!CanBegin(beat, requires)) return false;
            flags.Add(beat);
            return true;
        }

        /// <summary>Starts a fresh cycle: all beats can fire again.</summary>
        public static void BeginNewCycle(Opening opening)
        {
            flags.Clear();
            PendingOpening = opening;
        }

        public static CycleSummary CaptureSummary()
        {
            var s = GameSession.Instance;
            return new CycleSummary
            {
                gems = s.Gems, memories = MemoriesFound, deaths = s.Deaths, time = s.PlayTime, notches = NotchCount,
            };
        }

        /// <summary>The chest was opened: one more notch, saved for good.</summary>
        public static void CompleteCycle(CycleSummary summary)
        {
            SetCompletedCycles(CompletedCycles + 1);
            PlayerPrefs.Save();
            summary.notches = NotchCount;
            LastCycle = summary;
        }

        /// <summary>Replaces {notches} / {Notches} with the current count in words.</summary>
        public static string Format(string text)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf('{') < 0) return text;
            string word = NumberWord(NotchCount);
            return text.Replace("{notches}", word)
                       .Replace("{Notches}", char.ToUpperInvariant(word[0]) + word.Substring(1));
        }

        static readonly string[] Words =
        {
            "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten",
            "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen", "twenty",
        };

        public static string NumberWord(int n) => n >= 0 && n < Words.Length ? Words[n] : n.ToString();
    }
}
