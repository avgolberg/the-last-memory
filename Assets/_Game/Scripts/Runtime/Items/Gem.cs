namespace TileVania
{
    public class Gem : Pickup
    {
        public int value = 1;

        protected override void OnCollected(PlayerController player)
        {
            GameSession.Instance.AddGems(value);
            // Gems are fragments of memory: the first one of each cycle says so, without stopping play.
            if (Story.TryBegin("first_gem")) DialogueBox.Play(StoryScript.FirstGem, DialogueMode.Bark);
        }
    }
}
