using System.IO;
using UnityEditor;
using UnityEngine;

namespace TileVania.EditorTools
{
    /// <summary>
    /// Tiny original pixel-art pieces that the approved packs do not provide
    /// (heart icons, a ladder tile, a 1-pixel sprite, and the story props: the old notched tree, the pinned note,
    /// and the lantern, lit and rusted). Drawn from code so they are clearly project-made.
    /// </summary>
    public static class GeneratedArt
    {
        public const string Folder = ArtSetup.Art + "/Generated";

        static readonly string[] Heart =
        {
            ".XX...XX.",
            "XRRX.XRRX",
            "XRWRXRRRX",
            "XRRRRRRRX",
            "XRRRRRRRX",
            ".XRRRRRX.",
            "..XRRRX..",
            "...XRX...",
            "....X....",
        };

        public static void CreateAll()
        {
            Directory.CreateDirectory(Folder);
            Write("Heart_Full.png", FromPattern(Heart, c => c == 'X' ? Hex("2b0f17") : c == 'R' ? Hex("e43b44") : c == 'W' ? Hex("ffd6d6") : Color.clear));
            Write("Heart_Empty.png", FromPattern(Heart, c => c == 'X' ? Hex("2b0f17") : c == '.' ? Color.clear : Hex("4a3a4d")));
            Write("Ladder.png", LadderTexture());
            Write("OldTree.png", OldTreeTexture());
            Write("Note.png", FromPattern(NotePattern, c => c == 'P' ? Hex("e9dcb6") : c == 'L' ? Hex("8a7458") : c == 'S' ? Hex("c9b68c")
                : c == 'R' ? Hex("b03838") : Color.clear));
            Write("Lantern_Lit.png", FromPattern(LanternPattern, c => c == 'X' ? Hex("2a2420") : c == 'Y' ? Hex("7a6440")
                : c == 'W' ? Hex("ffe9a0") : c == 'O' ? Hex("ffb040") : Color.clear));
            Write("Lantern_Rusted.png", FromPattern(LanternPattern, c => c == 'X' ? Hex("4a2c1c") : c == 'Y' ? Hex("8a5530")
                : c == 'W' ? Hex("38363e") : c == 'O' ? Hex("4a4048") : Color.clear));
            var px = new Texture2D(4, 4);
            px.SetPixels(new Color[16].Populate(Color.white));
            Write("Pixel.png", px);
            AssetDatabase.Refresh();

            ArtSetup.Single("Generated/Heart_Full.png", 12, new Vector2(0.5f, 0.5f));
            ArtSetup.Single("Generated/Heart_Empty.png", 12, new Vector2(0.5f, 0.5f));
            ArtSetup.Single("Generated/Ladder.png", 16, new Vector2(0.5f, 0.5f));
            ArtSetup.Single("Generated/Pixel.png", 64, new Vector2(0.5f, 0.5f), fullRect: true);
            ArtSetup.Single("Generated/OldTree.png", 16, new Vector2(0.5f, 0f));
            ArtSetup.Single("Generated/Note.png", 16, new Vector2(0.5f, 0.5f));
            ArtSetup.Single("Generated/Lantern_Lit.png", 16, new Vector2(0.5f, 0.5f));
            ArtSetup.Single("Generated/Lantern_Rusted.png", 16, new Vector2(0.5f, 0.5f));
        }

        static readonly string[] NotePattern =
        {
            "...R...",
            ".PPPPP.",
            ".PLLLP.",
            ".PPPPP.",
            ".PLLPP.",
            ".PPPPP.",
            ".PLLLP.",
            ".PPPPS.",
            "..PPS..",
        };

        static readonly string[] LanternPattern =
        {
            "...XXX...",
            "..X...X..",
            "...XXX...",
            "..XXXXX..",
            ".XYWWWYX.",
            ".XYWOWYX.",
            ".XYOOOYX.",
            ".XYWOWYX.",
            ".XYWWWYX.",
            "..XXXXX..",
            ".XXXXXXX.",
        };

        /// <summary>
        /// A gnarled old tree, mostly bare, with a smooth patch of bark at eye level where the notches are carved.
        /// 40x76 px, pivot at the bottom centre.
        /// </summary>
        static Texture2D OldTreeTexture()
        {
            const int W = 40, H = 76;
            var t = new Texture2D(W, H);
            var px = new Color[W * H];
            var outline = Hex("1c110b"); var dark = Hex("3a2416"); var mid = Hex("553621"); var light = Hex("74502f");
            var leafDark = Hex("1f3d26"); var leaf = Hex("2f5a33"); var leafLight = Hex("4a7a40");
            int[,] mask = new int[W, H]; // 0 empty, 1 bark, 2 leaves

            // trunk with a slight lean and flared roots
            for (int y = 0; y < 56; y++)
            {
                float cx = 20f + Mathf.Sin(y * 0.07f) * 1.5f;
                float hw = 6.5f + (y < 6 ? (6 - y) * 1.6f : 0f) - (y > 40 ? (y - 40) * 0.12f : 0f);
                for (int x = 0; x < W; x++)
                    if (Mathf.Abs(x + 0.5f - cx) <= hw) mask[x, y] = 1;
            }
            void Branch(Vector2 a, Vector2 b, float r0, float r1)
            {
                for (float k = 0; k <= 1f; k += 0.02f)
                {
                    var p = Vector2.Lerp(a, b, k);
                    float r = Mathf.Lerp(r0, r1, k);
                    for (int x = Mathf.FloorToInt(p.x - r); x <= Mathf.CeilToInt(p.x + r); x++)
                        for (int y = Mathf.FloorToInt(p.y - r); y <= Mathf.CeilToInt(p.y + r); y++)
                            if (x >= 0 && y >= 0 && x < W && y < H && (new Vector2(x + 0.5f, y + 0.5f) - p).sqrMagnitude <= r * r)
                                mask[x, y] = 1;
                }
            }
            Branch(new Vector2(19f, 50f), new Vector2(7f, 68f), 3.2f, 1.2f);
            Branch(new Vector2(22f, 52f), new Vector2(33f, 66f), 3f, 1.1f);
            Branch(new Vector2(21f, 54f), new Vector2(23f, 73f), 2.4f, 1f);
            Branch(new Vector2(12f, 61f), new Vector2(4f, 62f), 1.3f, 0.8f);
            Branch(new Vector2(29f, 61f), new Vector2(37f, 59f), 1.2f, 0.8f);
            // a few stubborn leaves at the branch tips
            void Tuft(int cx, int cy, int r)
            {
                for (int x = cx - r; x <= cx + r; x++)
                    for (int y = cy - r; y <= cy + r; y++)
                        if (x >= 0 && y >= 0 && x < W && y < H && mask[x, y] == 0
                            && (x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r - ((x * 7 + y * 13) % 5 == 0 ? r : 0))
                            mask[x, y] = 2;
            }
            Tuft(6, 70, 4); Tuft(34, 68, 4); Tuft(23, 74, 2); Tuft(3, 63, 2); Tuft(38, 60, 2);

            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    int m = mask[x, y];
                    if (m == 0) continue;
                    bool edge = x == 0 || y == 0 || x == W - 1 || y == H - 1
                                || mask[x - 1, y] != m || mask[x + 1, y] != m || (y + 1 < H && mask[x, y + 1] != m);
                    Color c;
                    if (m == 2) c = edge ? leafDark : ((x + y) % 3 == 0 ? leafLight : leaf);
                    else if (edge) c = outline;
                    else
                    {
                        // vertical bark grain, lit from the right; a smooth dark patch where pale notches are cut
                        bool patch = y >= 24 && y <= 40 && Mathf.Abs(x - 20) <= 4;
                        int grain = (x * 31 + (y / 3) * 7) % 9;
                        bool lit = mask[Mathf.Min(W - 1, x + 2), y] != 1;
                        c = patch ? dark : lit ? light : grain < 2 ? dark : mid;
                    }
                    c.a = 1f;
                    px[y * W + x] = c;
                }
            // a knot hole above the patch
            foreach (var (x, y) in new[] { (17, 45), (18, 45), (17, 46), (18, 46), (18, 47) }) px[y * W + x] = outline;
            t.SetPixels(px);
            return t;
        }

        static Texture2D LadderTexture()
        {
            var t = new Texture2D(16, 16);
            var dark = Hex("2a160c"); var wood = Hex("7a4a28"); var light = Hex("a8703c");
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    Color c = Color.clear;
                    bool rail = x == 2 || x == 3 || x == 12 || x == 13;
                    bool railEdge = x == 1 || x == 4 || x == 11 || x == 14;
                    int ry = y % 8;
                    bool rung = (ry == 3 || ry == 4) && x > 3 && x < 12;
                    bool rungEdge = (ry == 2 || ry == 5) && x > 3 && x < 12;
                    if (rail) c = x == 2 || x == 12 ? light : wood;
                    else if (rung) c = ry == 4 ? wood : light;
                    else if (railEdge || rungEdge) c = dark;
                    t.SetPixel(x, y, c);
                }
            return t;
        }

        static Texture2D FromPattern(string[] rows, System.Func<char, Color> map)
        {
            int h = rows.Length, w = rows[0].Length;
            var t = new Texture2D(w, h);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    t.SetPixel(x, h - 1 - y, map(rows[y][x]));
            return t;
        }

        static void Write(string file, Texture2D t)
        {
            t.Apply();
            File.WriteAllBytes(Path.Combine(Folder, file), t.EncodeToPNG());
            Object.DestroyImmediate(t);
        }

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }

        static Color[] Populate(this Color[] arr, Color c)
        {
            for (int i = 0; i < arr.Length; i++) arr[i] = c;
            return arr;
        }
    }
}
