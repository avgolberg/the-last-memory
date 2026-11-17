using System.Collections;
using UnityEngine;

namespace LastMemory
{
    /// <summary>
    /// Hovering ranged enemy: when the player is in range and visible it glows (telegraph),
    /// then fires a projectile aimed at the player.
    /// </summary>
    public class ShooterEnemy : MonoBehaviour
    {
        public Projectile projectilePrefab;
        public float range = 9f;
        public float cooldown = 2.4f;
        public float telegraphTime = 0.6f;
        public Vector2 muzzleOffset = new Vector2(0f, 0.7f);
        public SpriteRenderer spriteRenderer;
        public bool spriteFacesRight = false;
        public Color telegraphColor = new Color(1f, 0.55f, 1f);

        float timer;
        Transform player;

        void Start()
        {
            timer = cooldown * Random.Range(0.4f, 0.8f);
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) player = pc.transform;
        }

        /// <summary>Where the bolt starts.</summary>
        Vector2 Muzzle => (Vector2)transform.position + muzzleOffset;

        /// <summary>Where the bolt is aimed: the middle of the hero's body.</summary>
        Vector2 AimPoint => (Vector2)player.position + Vector2.up * 0.8f;

        void Update()
        {
            if (player == null) return;
            Vector2 muzzle = Muzzle;
            Vector2 target = AimPoint;
            FaceTowards(target.x);

            timer -= Time.deltaTime;
            if (timer > 0f) return;

            // Only shoot when the hero is in range and nothing solid is in the way.
            if (Vector2.Distance(muzzle, target) > range) return;
            bool wallInTheWay = Physics2D.Linecast(muzzle, target, Layers.SolidMask);
            if (wallInTheWay) return;

            timer = cooldown;
            StartCoroutine(Fire());
        }

        void FaceTowards(float x)
        {
            if (spriteRenderer == null) return;
            // The sprite is drawn facing one way; flipX turns it around.
            if (spriteFacesRight) spriteRenderer.flipX = x < Muzzle.x;
            else spriteRenderer.flipX = x > Muzzle.x;
        }

        IEnumerator Fire()
        {
            AudioManager.Play(Sfx.EnemyShoot, 0.5f);
            for (float t = 0; t < telegraphTime; t += Time.deltaTime)
            {
                if (spriteRenderer != null)
                    spriteRenderer.color = Color.Lerp(Color.white, telegraphColor, Mathf.PingPong(t * 8f, 1f));
                yield return null;
            }
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
            if (player == null || projectilePrefab == null) yield break;
            // Aim at where the hero is now, after the warning.
            var bolt = Instantiate(projectilePrefab, Muzzle, Quaternion.identity);
            bolt.Launch(AimPoint - Muzzle);
        }
    }
}
