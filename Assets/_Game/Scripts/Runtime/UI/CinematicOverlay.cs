using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LastMemory
{
    /// <summary>
    /// Screen-space effects for the story scenes: letterbox bars, a full-screen colour fade (black or white)
    /// and a centred caption. All timing is in real time. Lives on the in-game canvas, under the dialogue box.
    /// </summary>
    public class CinematicOverlay : MonoBehaviour
    {
        static CinematicOverlay instance;

        public RectTransform letterboxTop, letterboxBottom;
        public float letterboxHeight = 120f;
        public Image fade;
        public CanvasGroup captionGroup;
        public TMP_Text captionText;

        float letterbox; // 0..1
        Coroutine flashRoutine;

        void Awake()
        {
            instance = this;
            SetLetterboxInternal(0f);
            if (fade != null) fade.color = Color.clear;
            if (captionGroup != null) captionGroup.alpha = 0f;
        }

        void OnDestroy() { if (instance == this) instance = null; }

        public static float FadeAlpha => instance != null && instance.fade != null ? instance.fade.color.a : 0f;
        public static float Letterbox => instance != null ? instance.letterbox : 0f;
        public static float CaptionAlpha => instance != null && instance.captionGroup != null ? instance.captionGroup.alpha : 0f;
        public static string CaptionText => instance != null && instance.captionText != null ? instance.captionText.text : "";

        public static void SetFade(Color color, float alpha)
        {
            if (instance == null || instance.fade == null) return;
            color.a = alpha;
            instance.fade.color = color;
        }

        /// <summary>Fades to <paramref name="color"/> at <paramref name="alpha"/>, blending the colour on the way.</summary>
        public static IEnumerator FadeTo(Color color, float alpha, float time)
        {
            if (instance == null || instance.fade == null) yield break;
            var img = instance.fade;
            Color from = img.color;
            if (from.a <= 0.001f) from = new Color(color.r, color.g, color.b, 0f);
            Color to = new Color(color.r, color.g, color.b, alpha);
            for (float t = 0; t < time; t += Time.unscaledDeltaTime * StoryDirector.CutsceneSpeed)
            {
                if (img == null) yield break;
                img.color = Color.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / time));
                yield return null;
            }
            if (img != null) img.color = to;
        }

        /// <summary>A quick full-screen flash (memory shards).</summary>
        public static void Flash(Color color, float peak, float time)
        {
            if (instance == null || instance.fade == null) return;
            if (instance.fade.color.a > peak) return; // don't fight a fade that is already darker
            if (instance.flashRoutine != null) instance.StopCoroutine(instance.flashRoutine);
            instance.flashRoutine = instance.StartCoroutine(instance.FlashRoutine(color, peak, time));
        }

        IEnumerator FlashRoutine(Color color, float peak, float time)
        {
            for (float t = 0; t < time; t += Time.unscaledDeltaTime)
            {
                color.a = peak * (1f - t / time);
                fade.color = color;
                yield return null;
            }
            fade.color = Color.clear;
            flashRoutine = null;
        }

        public static void SetLetterbox(float amount)
        {
            if (instance != null) instance.SetLetterboxInternal(amount);
        }

        public static IEnumerator LetterboxTo(float amount, float time)
        {
            if (instance == null) yield break;
            float from = instance.letterbox;
            for (float t = 0; t < time; t += Time.unscaledDeltaTime * StoryDirector.CutsceneSpeed)
            {
                if (instance == null) yield break;
                instance.SetLetterboxInternal(Mathf.Lerp(from, amount, Mathf.SmoothStep(0f, 1f, t / time)));
                yield return null;
            }
            if (instance != null) instance.SetLetterboxInternal(amount);
        }

        void SetLetterboxInternal(float amount)
        {
            letterbox = amount;
            float h = letterboxHeight * amount;
            if (letterboxTop != null) letterboxTop.sizeDelta = new Vector2(letterboxTop.sizeDelta.x, h);
            if (letterboxBottom != null) letterboxBottom.sizeDelta = new Vector2(letterboxBottom.sizeDelta.x, h);
        }

        /// <summary>Fades a centred caption in, holds it, and fades it out.</summary>
        public static IEnumerator Caption(string text, float fadeIn, float hold, float fadeOut)
        {
            if (instance == null || instance.captionGroup == null) yield break;
            var g = instance.captionGroup;
            instance.captionText.text = text;
            float speed = StoryDirector.CutsceneSpeed;
            for (float t = 0; t < fadeIn; t += Time.unscaledDeltaTime * speed) { if (g == null) yield break; g.alpha = t / fadeIn; yield return null; }
            if (g == null) yield break;
            g.alpha = 1f;
            yield return new WaitForSecondsRealtime(hold / speed);
            for (float t = 0; t < fadeOut; t += Time.unscaledDeltaTime * speed) { if (g == null) yield break; g.alpha = 1f - t / fadeOut; yield return null; }
            if (g != null) g.alpha = 0f;
        }
    }
}
