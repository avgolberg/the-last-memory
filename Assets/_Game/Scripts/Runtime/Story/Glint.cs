using UnityEngine;

namespace TileVania
{
    /// <summary>A small periodic sparkle that draws the eye to something worth examining.</summary>
    public class Glint : MonoBehaviour
    {
        public Color color = new Color(1f, 0.9f, 0.6f);
        public float interval = 2.6f;
        public Vector2 offset;

        float timer;

        void Start() => timer = Random.Range(0.3f, interval);

        void Update()
        {
            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer = interval;
            Fx.Burst((Vector2)transform.position + offset, color, 4, 1.2f, 0.55f, 0f, 1.5f);
        }
    }
}
