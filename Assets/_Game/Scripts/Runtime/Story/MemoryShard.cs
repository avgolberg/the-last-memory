using UnityEngine;

namespace TileVania
{
    /// <summary>
    /// An optional memory fragment: a pale glowing shard. Picking it up is worth a few gems and briefly
    /// freezes the world to show one remembered line. Found shards stay found for the rest of the cycle.
    /// </summary>
    public class MemoryShard : Pickup
    {
        public string memoryId = "memory_1";
        public int gemValue = 3;

        protected override void Start()
        {
            base.Start();
            // Restarting a level must not bring back a memory that was already recovered this cycle.
            if (Story.Has(memoryId)) Destroy(gameObject);
        }

        protected override void OnCollected(PlayerController player)
        {
            GameSession.Instance.AddGems(gemValue);
            if (!Story.TryBegin(memoryId)) return;
            Fx.Burst(transform.position, new Color(0.9f, 0.85f, 1f), 24, 4f, 0.9f, -1f, 2f);
            CinematicOverlay.Flash(new Color(0.85f, 0.8f, 1f), 0.35f, 0.45f);
            var beat = StoryScript.Beat(memoryId);
            if (beat != null) DialogueBox.Play(beat.lines, DialogueMode.Blocking);
        }
    }
}
