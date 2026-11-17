using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace TileVania.EditorTools
{
    /// <summary>
    /// Builds the set for the ending's flashback (<see cref="MemoryFlashback"/>) into the Treasure Chamber scene:
    /// a strip of forest to the right of the level with the cottage, the path into the trees and the castle on
    /// the horizon, plus the two actors (the hero as they were, and Lia). Memory 4 is staged in the chamber itself.
    /// Everything here is scenery: no colliders, no gameplay.
    /// </summary>
    public static class MemoryStageBuilder
    {
        static readonly Color NightGround = new Color(0.6f, 0.68f, 0.8f);

        static Sprite F(string n) => ArtSetup.ForestTile(n);
        static Sprite Generated(string file) => ArtSetup.SingleSprite("Generated/" + file);

        public static MemoryFlashback Build(StoryDirector director, DragonBoss keeper, TreasureChest chest,
            Light2D globalLight, GameObject levelBackdrop)
        {
            var root = new GameObject("MemoryStage");
            root.transform.position = new Vector3(MemoryFlashback.StageX, 0f, 0f);
            var flashback = root.AddComponent<MemoryFlashback>();
            flashback.globalLight = globalLight;
            flashback.levelBackdrop = levelBackdrop;
            flashback.keeper = keeper;
            flashback.player = director.player;
            flashback.chest = chest;

            GameObject Sprite(string name, Sprite s, Vector2 local, string layer, int order, Color tint, bool unlit = false, Transform parent = null)
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent != null ? parent : root.transform, false);
                go.transform.localPosition = local;
                ContentBuilder.AddSprite(go, s, layer, order, unlit).color = tint;
                return go;
            }
            Sprite pixel = Generated("Pixel.png");
            GameObject Quad(string name, Vector2 centre, Vector2 size, string layer, int order, Color color, bool unlit = true)
            {
                var go = Sprite(name, pixel, centre, layer, order, color, unlit);
                go.transform.localScale = new Vector3(size.x * 16f, size.y * 16f, 1f); // the pixel sprite is 1/16 unit
                return go;
            }
            Sprite Bg(string n) => ArtSetup.SingleSprite($"Environment/Forest/Forest_{n}.png");

            // --- Sky, stars, moon, hills and the castle on the horizon
            flashback.sky = Quad("Sky", new Vector2(22f, 8f), new Vector2(80f, 34f), "Background", -110, new Color(0.07f, 0.08f, 0.18f))
                .GetComponent<SpriteRenderer>();
            var stars = Sprite("Stars", Bg("stars"), new Vector2(22f, 4f), "Background", -105, new Color(0.8f, 0.85f, 1f, 0.7f), unlit: true);
            var starsSr = stars.GetComponent<SpriteRenderer>();
            starsSr.drawMode = SpriteDrawMode.Tiled;
            starsSr.size = new Vector2(80f, 16f);
            flashback.stars = starsSr;
            var hills = Sprite("Hills", Bg("mountain_blue"), new Vector2(22f, -0.6f), "Background", -100, new Color(0.3f, 0.36f, 0.5f));
            var hillsSr = hills.GetComponent<SpriteRenderer>();
            hillsSr.drawMode = SpriteDrawMode.Tiled;
            hillsSr.size = new Vector2(80f, hillsSr.sprite.bounds.size.y);
            Sprite("Castle", Generated("Castle_Silhouette.png"), new Vector2(34.5f, 0.6f), "Background", -98, Color.white)
                .transform.localScale = new Vector3(1.2f, 1.2f, 1f);

            // --- Ground (a tilemap without colliders), grass, trees and a bush by the cottage
            var grid = new GameObject("Grid").AddComponent<Grid>();
            grid.transform.SetParent(root.transform, false);
            var groundGo = new GameObject("Ground");
            groundGo.transform.SetParent(grid.transform, false);
            var ground = groundGo.AddComponent<Tilemap>();
            var groundRenderer = groundGo.AddComponent<TilemapRenderer>();
            groundRenderer.sortingLayerName = "Platforms";
            groundRenderer.sharedMaterial = ContentBuilder.Lit;
            ground.color = NightGround; // the bright grass tiles would look like daylight
            var rng = new System.Random(42);
            for (int x = -16; x < 60; x++)
                for (int y = -6; y < 0; y++)
                    ground.SetTile(new Vector3Int(x, y, 0), LevelBuilder.TileFor(y == -1 ? F("Top_Mid") : F("Fill_" + rng.Next(5))));
            for (int x = -12; x < 56; x++)
                if (rng.Next(3) == 0)
                    Sprite("Grass", F("Grass_" + rng.Next(2)), new Vector2(x + 0.5f, 0.5f), "Platforms", 2, NightGround);

            var trunk = ArtSetup.GetSprite("Environment/Forest/Forest_tree.png", "Tree_Trunk");
            var clump = ArtSetup.GetSprite("Environment/Forest/Forest_leaves.png", "LeafClump");
            var treeTint = new Color(0.55f, 0.6f, 0.6f);
            foreach (float x in new[] { -6f, 15.5f, 20.5f, 27f, 32.5f, 39f, 45f })
            {
                Sprite("TreeTrunk", trunk, new Vector2(x, -0.2f), "Background", 20, treeTint);
                foreach (var o in new[] { new Vector2(-1.4f, 4.4f), new Vector2(1.6f, 4.6f), new Vector2(0.1f, 5.6f), new Vector2(-0.2f, 4.1f) })
                    Sprite("TreeLeaves", clump, new Vector2(x + o.x, o.y), "Background", 21, treeTint);
            }
            Sprite("Bush", ArtSetup.GetSprite("Environment/Forest/Forest_leaves.png", "Bush_Small"), new Vector2(MemoryFlashback.HouseX - 3.6f, -0.1f), "Background", 45, Color.white);

            // --- The cottage: walls with see-through window panes, the room behind them, the lantern and the doorway
            float house = MemoryFlashback.HouseX;
            Sprite("Cottage", Generated("Cottage.png"), new Vector2(house, 0f), "Background", 30, Color.white);
            Vector2 PixelCentre(RectInt r) => new Vector2(house + (r.center.x - GeneratedArt.CottageWidth / 2f) / 16f, r.center.y / 16f);
            Vector2 PixelSize(RectInt r) => new Vector2(r.width / 16f, r.height / 16f);
            Vector2 window = PixelCentre(GeneratedArt.CottageWindow), door = PixelCentre(GeneratedArt.CottageDoor);
            flashback.windowX = MemoryFlashback.StageX + window.x;
            flashback.doorX = MemoryFlashback.StageX + door.x;

            flashback.room = Quad("Room", window, PixelSize(GeneratedArt.CottageWindow) + new Vector2(0.3f, 0.3f), "Background", 26,
                new Color(1f, 0.78f, 0.42f)).GetComponent<SpriteRenderer>();

            var windowLantern = Sprite("WindowLantern", Generated("Lantern_Lit.png"), window + new Vector2(0.15f, -0.25f), "Background", 27, Color.white, unlit: true);
            var windowLight = ContentBuilder.AddPointLight(new GameObject("Light"), new Color(1f, 0.72f, 0.38f), 3.6f, 1.1f);
            windowLight.transform.SetParent(windowLantern.transform, false);
            windowLight.gameObject.AddComponent<LightFlicker>();
            flashback.windowLantern = windowLantern;

            var doorway = Quad("Doorway", door, PixelSize(GeneratedArt.CottageDoor), "Background", 31, new Color(0.95f, 0.72f, 0.4f));
            ContentBuilder.AddPointLight(new GameObject("Light"), new Color(1f, 0.75f, 0.45f), 4f, 0.9f).transform.SetParent(doorway.transform, false);
            doorway.transform.GetChild(0).localScale = new Vector3(1f / doorway.transform.localScale.x, 1f / doorway.transform.localScale.y, 1f);
            flashback.doorway = doorway;

            // --- The actors
            flashback.hero = Actor(root.transform, "Hero (memory)",
                ArtSetup.Frames("Player/Hero_Idle.png", "Hero_Idle", 4), ArtSetup.Frames("Player/Hero_Run.png", "Hero_Run", 8), 0);
            var sack = Sprite("GoldSack", Generated("GoldSack.png"), new Vector2(-0.45f, 0.8f), "Player", 1, Color.white, parent: flashback.hero.transform);
            flashback.goldSack = sack;

            var liaIdle = Generated("Lia_0.png");
            var liaWalk = Generated("Lia_1.png");
            flashback.lia = Actor(root.transform, "Lia (memory)", new[] { liaIdle }, new[] { liaWalk, liaIdle }, -1);
            flashback.lia.walkFps = 8f;
            var lantern = Sprite("Lantern", Generated("Lantern_Lit.png"), new Vector2(0.45f, 0.5f), "Player", 1, Color.white, unlit: true,
                parent: flashback.lia.transform);
            var lanternLight = ContentBuilder.AddPointLight(new GameObject("Light"), new Color(1f, 0.7f, 0.35f), 3.4f, 1.2f);
            lanternLight.transform.SetParent(lantern.transform, false);
            lanternLight.gameObject.AddComponent<LightFlicker>();
            flashback.liaLantern = lantern;

            director.flashback = flashback;
            director.keeper = keeper;
            return flashback;
        }

        static MemoryActor Actor(Transform parent, string name, Sprite[] idle, Sprite[] walk, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var body = ContentBuilder.AddSprite(go, idle[0], "Player", order);
            var actor = go.AddComponent<MemoryActor>();
            actor.body = body;
            actor.idleFrames = idle;
            actor.walkFrames = walk;
            return actor;
        }
    }
}
