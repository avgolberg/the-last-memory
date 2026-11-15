using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileVania
{
    /// <summary>
    /// Carved tally marks (groups of five, one group per row) showing how many cycles have passed.
    /// Built from a 1-pixel sprite so the count can change at runtime; the level builder bakes the
    /// first-play count into the scene so it is visible in the editor too.
    /// </summary>
    public class TallyMarks : MonoBehaviour
    {
        public Sprite markSprite;
        public Color color = new Color(0.88f, 0.76f, 0.58f);
        public Color glowColor = new Color(1f, 0.85f, 0.45f);
        public string sortingLayer = "Background";
        public int sortingOrder = 26;
        [Tooltip("Pixel size of one game pixel in world units (16 PPU).")]
        public float px = 1f / 16f;
        public bool buildOnStart = true;

        readonly List<SpriteRenderer> marks = new List<SpriteRenderer>();

        public int Count { get; private set; }

        void Start()
        {
            if (buildOnStart) Build(Story.NotchCount);
        }

        public void Build(int count)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var c = transform.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(c); else DestroyImmediate(c);
            }
            marks.Clear();
            Count = count;
            const int stroke = 6;     // pixels per vertical stroke
            const int gap = 3;        // pixels between strokes
            const int rowHeight = 9;  // pixels between rows
            for (int i = 0; i < count; i++)
            {
                int group = i / 5, idx = i % 5;
                float rowY = -group * rowHeight * px;
                var go = new GameObject("Notch" + (i + 1));
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = markSprite;
                sr.sortingLayerName = sortingLayer;
                sr.sortingOrder = sortingOrder;
                sr.color = color;
                if (idx < 4)
                {
                    go.transform.localPosition = new Vector3((idx * gap - 4.5f) * px, rowY, 0f);
                    go.transform.localScale = new Vector3(1f, stroke, 1f);
                }
                else
                {
                    // the fifth stroke crosses the other four
                    go.transform.localPosition = new Vector3(0f, rowY, 0f);
                    go.transform.localRotation = Quaternion.Euler(0f, 0f, -62f);
                    go.transform.localScale = new Vector3(1f, stroke * 2.2f, 1f);
                }
                marks.Add(sr);
            }
            // "The last one is fresh": the newest cut is paler than the weathered ones.
            if (marks.Count > 0) marks[marks.Count - 1].color = Color.Lerp(color, glowColor, 0.35f);
        }

        public Vector3 NewestPosition => marks.Count > 0 ? marks[marks.Count - 1].transform.position : transform.position;

        /// <summary>The newest notch glows, so an attentive player notices the count went up.</summary>
        public void PulseNewest(float duration = 3f)
        {
            if (marks.Count == 0) return;
            StartCoroutine(Pulse(marks[marks.Count - 1], duration));
        }

        IEnumerator Pulse(SpriteRenderer sr, float duration)
        {
            Fx.Burst(sr.transform.position, glowColor, 8, 1.2f, 0.7f, 0f, 1.5f);
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float k = Mathf.Sin(t / duration * Mathf.PI * 3f) * 0.5f + 0.5f;
                sr.color = Color.Lerp(color, glowColor, k);
                yield return null;
            }
            sr.color = Color.Lerp(color, glowColor, 0.35f); // stays a little fresher than the others
        }
    }
}
