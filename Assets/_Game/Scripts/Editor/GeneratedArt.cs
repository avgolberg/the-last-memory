using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TileVania.EditorTools
{
    /// <summary>
    /// Tiny original pixel-art pieces that the approved packs do not provide
    /// (heart icons, a ladder tile, a 1-pixel sprite, the story props: the old notched tree, the pinned note,
    /// the lantern, lit and rusted; and for the flashback: Lia, the cottage, the distant castle and a sack of gold).
    /// Drawn from code so they are clearly project-made.
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

            // The flashback: Lia (standing and walking), the cottage, the castle on the horizon and the sack of gold
            Write("Lia_0.png", FromPattern(LiaPattern(walking: false), LiaColor));
            Write("Lia_1.png", FromPattern(LiaPattern(walking: true), LiaColor));
            Write("Cottage.png", CottageTexture());
            Write("Castle_Silhouette.png", CastleSilhouetteTexture());
            Write("GoldSack.png", FromPattern(SackPattern, c => c == 'E' ? Hex("2a1a10") : c == 'S' ? Hex("8a6440")
                : c == 'B' ? Hex("9a7448") : c == 'G' ? Hex("f0c050") : Color.clear));
            AssetDatabase.Refresh();

            ArtSetup.Single("Generated/Heart_Full.png", 12, new Vector2(0.5f, 0.5f));
            ArtSetup.Single("Generated/Heart_Empty.png", 12, new Vector2(0.5f, 0.5f));
            ArtSetup.Single("Generated/Ladder.png", 16, new Vector2(0.5f, 0.5f));
            ArtSetup.Single("Generated/Pixel.png", 64, new Vector2(0.5f, 0.5f), fullRect: true);
            ArtSetup.Single("Generated/OldTree.png", 16, new Vector2(0.5f, 0f));
            ArtSetup.Single("Generated/Note.png", 16, new Vector2(0.5f, 0.5f));
            ArtSetup.Single("Generated/Lantern_Lit.png", 16, new Vector2(0.5f, 0.5f));
            ArtSetup.Single("Generated/Lantern_Rusted.png", 16, new Vector2(0.5f, 0.5f));
            ArtSetup.Single("Generated/Lia_0.png", 16, new Vector2(0.5f, 0f));
            ArtSetup.Single("Generated/Lia_1.png", 16, new Vector2(0.5f, 0f));
            ArtSetup.Single("Generated/Cottage.png", 16, new Vector2(0.5f, 0f));
            ArtSetup.Single("Generated/Castle_Silhouette.png", 16, new Vector2(0.5f, 0f));
            ArtSetup.Single("Generated/GoldSack.png", 16, new Vector2(0.5f, 0.5f));
        }

        // ------------------------------------------------------------------ flashback art

        /// <summary>
        /// Lia, the hero's little sister: the same chunky style as the hero (blank face, big hair), about two thirds
        /// of the hero's height. Faces right. The walking frame only moves the legs.
        /// </summary>
        static string[] LiaPattern(bool walking)
        {
            var rows = new List<string>
            {
                "..............",
                ".....HHHHH....",
                "....HHHHHHH...",
                "...HHHhhhHHH..",
                "...HHhSSSShH..",
                "...HHSSSSSSH..",
                "...HHSSSSSSS..",
                "...HHSsSSSsS..",
                "...HHSSSSSSH..",
                "...HH.SSSSS...",
                "...H..WDDW....",
                ".....DDDDDD...",
                "....SDDDDDDS..",
                ".....dDDDDd...",
                "....DDDDDDDD..",
                "....dDDDDDDd..",
            };
            rows.AddRange(walking
                ? new[] { "....LL....LL..", "....BB....BBB." }
                : new[] { ".....LL..LL...", ".....BB..BBB.." });
            return rows.ToArray();
        }

        static Color LiaColor(char c) => c switch
        {
            'H' => Hex("583422"), 'h' => Hex("784a2e"),   // hair
            'S' => Hex("f3a787"), 's' => Hex("f2d3ac"),   // skin, rosy cheeks
            'D' => Hex("54609a"), 'd' => Hex("3c4470"),   // dress
            'W' => Hex("ded6c4"),                         // collar
            'L' => Hex("341c27"), 'B' => Hex("1e161a"),   // legs, shoes
            _ => Color.clear,
        };

        static readonly string[] SackPattern =
        {
            "...EE.....",
            "..ESSE....",
            ".EBBBBE...",
            "EBGBBBBE..",
            "EBBBBGBBE.",
            "EBBBBBBBE.",
            "EBBGBBBBE.",
            ".EBBBBBE..",
            "..EEEEE...",
            "..........",
        };

        /// <summary>Cottage size and the pixel rectangles (bottom-left origin) of its window panes and door.</summary>
        public const int CottageWidth = 88, CottageHeight = 72;
        public static readonly RectInt CottageWindow = new RectInt(17, 15, 17, 17);
        public static readonly RectInt CottageDoor = new RectInt(53, 4, 13, 30);

        /// <summary>
        /// A small wooden cottage with a gable roof, a chimney, a door and a window whose panes are transparent,
        /// so whatever stands behind the cottage (a lit room, Lia and her lantern) shows through. 88x72 px.
        /// </summary>
        static Texture2D CottageTexture()
        {
            var t = Blank(CottageWidth, CottageHeight);
            Color outline = Hex("1c120c"), plankA = Hex("6e4a2c"), plankB = Hex("5e3f25"), seam = Hex("3e2816"), post = Hex("4a2f1a");
            Color stoneA = Hex("6a6470"), stoneB = Hex("55505c"), roofA = Hex("7a3f2e"), roofB = Hex("5e2f22"), roofEdge = Hex("3a1a14");
            Color frame = Hex("3a2414"), sill = Hex("8a6440"), doorA = Hex("4e3220"), doorB = Hex("432a1a"), brass = Hex("d8a040");

            for (int x = 6; x < 82; x++)                                   // stone foundation
                for (int y = 0; y < 4; y++) t.SetPixel(x, y, ((x / 4) + (y / 2)) % 2 == 0 ? stoneA : stoneB);
            for (int y = 4; y < 41; y++)                                   // plank walls
                for (int x = 8; x < 80; x++)
                {
                    bool joint = y % 4 == 3 || (x * 7 + (y / 4) * 13) % 23 == 0;
                    t.SetPixel(x, y, joint ? seam : (y / 4) % 2 == 0 ? plankA : plankB);
                }
            Fill(t, 8, 4, 10, 40, post);
            Fill(t, 77, 4, 79, 40, post);

            for (int y = 39; y < 71; y++)                                  // gable roof with rows of shingles
            {
                int half = (int)(44 * (1f - (y - 39) / 31f)) + 1;
                for (int x = 44 - half; x < 44 + half; x++)
                {
                    bool edge = x <= 44 - half + 1 || x >= 44 + half - 2 || y == 39;
                    bool shingleLine = y % 4 == 0 || (x + (y / 4) * 3) % 6 == 0;
                    t.SetPixel(x, y, edge ? roofEdge : shingleLine ? roofB : roofA);
                }
            }
            Fill(t, 60, 52, 67, 68, stoneB);                               // chimney
            Fill(t, 61, 53, 66, 68, stoneA);
            Fill(t, 59, 68, 68, 70, outline);

            Fill(t, 15, 13, 35, 33, frame);                                // window frame, four clear panes, sill
            foreach (var pane in new[] { new RectInt(17, 15, 8, 8), new RectInt(26, 15, 8, 8), new RectInt(17, 24, 8, 8), new RectInt(26, 24, 8, 8) })
                Fill(t, pane.xMin, pane.yMin, pane.xMax - 1, pane.yMax - 1, Color.clear);
            Fill(t, 13, 11, 37, 12, sill);
            Fill(t, 13, 11, 37, 11, frame);

            Fill(t, 51, 4, 67, 35, frame);                                 // door
            for (int x = 53; x < 66; x++)
                for (int y = 4; y < 34; y++) t.SetPixel(x, y, (x - 53) % 4 == 0 ? doorB : doorA);
            Fill(t, 53, 18, 65, 18, frame);
            Fill(t, 53, 28, 65, 28, frame);
            Fill(t, 63, 16, 63, 17, brass);
            return t;
        }

        /// <summary>The castle on the horizon: a dark silhouette with three towers and a few lit windows. 64x56 px.</summary>
        static Texture2D CastleSilhouetteTexture()
        {
            var t = Blank(64, 56);
            var dark = Hex("201a33");
            Fill(t, 4, 0, 59, 24, dark);
            for (int x = 4; x < 60; x += 4) Fill(t, x, 25, x + 1, 26, dark);    // battlements
            foreach (var (x0, width, top) in new[] { (8, 10, 38), (46, 10, 34), (26, 12, 48) })
            {
                Fill(t, x0, 0, x0 + width - 1, top, dark);
                for (int x = x0; x < x0 + width; x += 3) Fill(t, x, top + 1, x + 1, top + 2, dark);
            }
            Fill(t, 30, 49, 33, 53, dark);                                      // spire
            Fill(t, 31, 54, 32, 55, dark);
            var lit = Hex("f0c060");
            t.SetPixel(31, 40, lit); t.SetPixel(12, 28, lit); t.SetPixel(50, 20, lit);
            return t;
        }

        static Texture2D Blank(int w, int h)
        {
            var t = new Texture2D(w, h);
            t.SetPixels(new Color[w * h].Populate(Color.clear));
            return t;
        }

        /// <summary>Fills an inclusive pixel rectangle (bottom-left origin).</summary>
        static void Fill(Texture2D t, int x0, int y0, int x1, int y1, Color c)
        {
            for (int x = x0; x <= x1; x++)
                for (int y = y0; y <= y1; y++)
                    if (x >= 0 && y >= 0 && x < t.width && y < t.height) t.SetPixel(x, y, c);
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
