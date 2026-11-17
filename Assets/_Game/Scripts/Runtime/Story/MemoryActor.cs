using System.Collections;
using UnityEngine;

namespace TileVania
{
    /// <summary>
    /// A character in the flashback (the hero as they were, and Lia). It is only scenery: no physics and no input.
    /// It can stand, walk to a point, turn around and fade. The pivot is at the feet; the sprites face right.
    /// </summary>
    public class MemoryActor : MonoBehaviour
    {
        public SpriteRenderer body;
        public Sprite[] idleFrames;
        public Sprite[] walkFrames;
        public float idleFps = 4f;
        public float walkFps = 10f;
        [Tooltip("Walking speed in units per second (before the cutscene speed-up used by tests).")]
        public float walkSpeed = 2.2f;

        bool walking;
        float stepRate = 1f; // running animates the legs faster
        float frameTimer;
        int frame;

        public int Facing { get; private set; } = 1;

        void Update()
        {
            var frames = walking && walkFrames != null && walkFrames.Length > 0 ? walkFrames : idleFrames;
            if (body == null || frames == null || frames.Length == 0) return;
            frameTimer += Time.unscaledDeltaTime * StoryDirector.CutsceneSpeed;
            float fps = walking ? walkFps * stepRate : idleFps;
            if (frameTimer >= 1f / fps)
            {
                frameTimer = 0f;
                frame = (frame + 1) % frames.Length;
            }
            body.sprite = frames[frame % frames.Length];
        }

        /// <summary>Faces left (-1) or right (+1). Children such as a carried lantern turn with the actor.</summary>
        public void Face(int direction)
        {
            if (direction == 0) return;
            Facing = direction > 0 ? 1 : -1;
            var s = transform.localScale;
            transform.localScale = new Vector3(Mathf.Abs(s.x) * Facing, s.y, s.z);
        }

        public void Place(float x, float y, int facing)
        {
            StopWalk();
            transform.position = new Vector3(x, y, transform.position.z);
            Face(facing);
        }

        /// <summary>Starts walking to <paramref name="x"/> (speed 1 = walk, 2 = run). Yield on the result to wait.</summary>
        public Coroutine StartWalk(float x, float speedMultiplier = 1f)
        {
            StopWalk();
            walkRoutine = StartCoroutine(Walk(x, speedMultiplier));
            return walkRoutine;
        }

        /// <summary>Walks to <paramref name="x"/> and waits until the actor gets there.</summary>
        public IEnumerator WalkTo(float x, float speedMultiplier = 1f)
        {
            yield return StartWalk(x, speedMultiplier);
        }

        public void StopWalk()
        {
            if (walkRoutine != null) StopCoroutine(walkRoutine);
            walkRoutine = null;
            walking = false;
        }

        Coroutine walkRoutine;

        IEnumerator Walk(float x, float speedMultiplier)
        {
            Face((int)Mathf.Sign(x - transform.position.x));
            walking = true;
            stepRate = speedMultiplier;
            while (Mathf.Abs(transform.position.x - x) > 0.02f)
            {
                float step = walkSpeed * speedMultiplier * Time.unscaledDeltaTime * StoryDirector.CutsceneSpeed;
                var p = transform.position;
                p.x = Mathf.MoveTowards(p.x, x, step);
                transform.position = p;
                yield return null;
            }
            walking = false;
            walkRoutine = null;
        }

        /// <summary>Fades every sprite of the actor (including what it carries) to the given alpha.</summary>
        public IEnumerator FadeTo(float alpha, float time)
        {
            var renderers = GetComponentsInChildren<SpriteRenderer>(true);
            var from = new float[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) from[i] = renderers[i].color.a;
            for (float t = 0; t < time; t += Time.unscaledDeltaTime * StoryDirector.CutsceneSpeed)
            {
                for (int i = 0; i < renderers.Length; i++) SetAlpha(renderers[i], Mathf.Lerp(from[i], alpha, t / time));
                yield return null;
            }
            foreach (var r in renderers) SetAlpha(r, alpha);
        }

        static void SetAlpha(SpriteRenderer r, float a)
        {
            var c = r.color;
            c.a = a;
            r.color = c;
        }
    }
}
