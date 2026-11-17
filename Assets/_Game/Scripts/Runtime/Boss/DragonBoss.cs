using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LastMemory
{
    /// <summary>
    /// The Keeper: a dragon with three telegraphed attacks. It guards the last memory because the hero once
    /// made it promise to (Docs/STORY_SPEC.md); it speaks before the fight and after its defeat.
    ///  - Fire Volley: glows and shakes, then fires a fan of fireballs at the player.
    ///  - Dive Slam: marks the floor under the player, then slams down (vulnerable afterwards).
    ///  - Fire Rain: marks several floor columns, then fire falls on them.
    /// Below half health it becomes enraged: shorter pauses, more projectiles, slam shockwaves.
    /// </summary>
    public class DragonBoss : MonoBehaviour, IDamageable
    {
        public string displayName = "The Keeper";
        public int maxHealth = 30;
        public AudioClip bossMusic;

        [Header("References")]
        public Transform visual;
        public SpriteRenderer spriteRenderer;
        public bool spriteFacesRight = false;
        public ContactDamage contactDamage;
        public Collider2D hurtbox;
        public Projectile fireballPrefab;
        public Projectile fallingFirePrefab;
        public Projectile shockwavePrefab;
        public TreasureChest treasure;
        public Material warningMaterial;

        [Header("Arena (world space)")]
        public float arenaLeft = 2f;
        public float arenaRight = 30f;
        public float floorY = 2f;
        public float ceilingY = 15f;

        int health;
        bool fighting, dead, enraged;
        Vector3 startPosition;
        Transform player;
        Color baseColor = Color.white;
        int lastAttack = -1;
        Coroutine fightRoutine;
        readonly List<GameObject> warnings = new List<GameObject>();

        public bool IsFighting => fighting;
        public bool IsDead => dead;
        public int Health => health;

        void Awake()
        {
            health = maxHealth;
            startPosition = transform.position;
            if (treasure != null) treasure.gameObject.SetActive(false);
            SetVulnerable(false);
        }

        void Start()
        {
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) player = pc.transform;
        }

        void SetVulnerable(bool on)
        {
            if (hurtbox != null) hurtbox.enabled = on;
            if (contactDamage != null) contactDamage.active = on;
        }

        public void StartFight()
        {
            if (fighting || dead) return;
            fighting = true;
            health = maxHealth;
            enraged = false;
            baseColor = Color.white;
            HUD.ShowBossBar(displayName);
            if (bossMusic != null) AudioManager.PlayMusic(bossMusic, 0.5f);
            fightRoutine = StartCoroutine(FightLoop());
        }

        public void ResetFight()
        {
            if (dead) return;
            StopAllCoroutines();
            ClearWarnings();
            fighting = false;
            health = maxHealth;
            enraged = false;
            baseColor = Color.white;
            if (spriteRenderer) spriteRenderer.color = baseColor;
            transform.position = startPosition;
            SetVulnerable(false);
            HUD.HideBossBar();
            foreach (var p in FindObjectsByType<Projectile>(FindObjectsSortMode.None))
                if (p.gameObject.layer == Layers.EnemyProjectile) Destroy(p.gameObject);
        }

        IEnumerator FightLoop()
        {
            // Intro: swoop in, then speak (once per cycle; the world is frozen while it talks)
            yield return MoveTo(new Vector3((arenaLeft + arenaRight) * 0.5f, floorY + 8f), 1.4f);
            if (Story.TryBegin("boss_intro")) yield return DialogueBox.Run(StoryScript.BossIntro, DialogueMode.Blocking);
            else DialogueBox.Play(StoryScript.BossRetry, DialogueMode.Bark);
            SetVulnerable(true);
            AudioManager.Play(Sfx.BossWarning);
            CameraController.Shake(0.35f, 0.8f);
            yield return Flash(new Color(1f, 0.5f, 0.3f), 0.8f);

            while (fighting && !dead)
            {
                int attack;
                do attack = Random.Range(0, 3); while (attack == lastAttack);
                lastAttack = attack;
                switch (attack)
                {
                    case 0: yield return FireVolley(); break;
                    case 1: yield return DiveSlam(); break;
                    default: yield return FireRain(); break;
                }
                yield return Hover(enraged ? 0.7f : 1.3f);
            }
        }

        IEnumerator FireVolley()
        {
            float side = player != null && player.position.x > (arenaLeft + arenaRight) * 0.5f ? arenaLeft + 2.5f : arenaRight - 2.5f;
            // level with the side platforms so the player can hit it with straight shots
            yield return MoveTo(new Vector3(side, floorY + 3.2f), 0.9f);
            int volleys = enraged ? 2 : 1;
            for (int v = 0; v < volleys; v++)
            {
                AudioManager.Play(Sfx.BossWarning, 0.6f);
                yield return Flash(new Color(1f, 0.55f, 0.2f), 0.7f, true);
                if (player == null) yield break;
                Vector2 mouth = Mouth();
                Vector2 dir = ((Vector2)player.position + Vector2.up * 0.7f - mouth).normalized;
                int count = enraged ? 5 : 3;
                float spread = 14f;
                for (int i = 0; i < count; i++)
                {
                    float angle = (i - (count - 1) * 0.5f) * spread;
                    var p = Instantiate(fireballPrefab, mouth, Quaternion.identity);
                    p.Launch(Quaternion.Euler(0, 0, angle) * dir);
                }
                AudioManager.Play(Sfx.EnemyShoot);
                yield return new WaitForSeconds(0.5f);
            }
        }

        IEnumerator DiveSlam()
        {
            float targetX = player != null ? player.position.x : transform.position.x;
            targetX = Mathf.Clamp(targetX, arenaLeft + 3f, arenaRight - 3f);
            yield return MoveTo(new Vector3(targetX, floorY + 6.5f), 0.7f);

            var marker = MakeWarning(new Vector2(targetX, floorY + 0.15f), new Vector2(5f, 0.3f));
            AudioManager.Play(Sfx.BossWarning, 0.7f);
            yield return Flash(new Color(1f, 0.3f, 0.3f), 0.9f, true);
            RemoveWarning(marker);

            // Slam
            Vector3 from = transform.position, to = new Vector3(targetX, floorY);
            for (float t = 0; t < 1f; t += Time.deltaTime / 0.25f)
            {
                transform.position = Vector3.Lerp(from, to, t * t);
                yield return null;
            }
            transform.position = to;
            CameraController.Shake(0.45f, 0.4f);
            AudioManager.Play(Sfx.RockImpact);
            Fx.Burst(new Vector2(targetX, floorY + 0.2f), new Color(0.7f, 0.6f, 0.75f), 24, 6f, 0.6f);
            if (enraged && shockwavePrefab != null)
            {
                foreach (int d in new[] { -1, 1 })
                {
                    var w = Instantiate(shockwavePrefab, new Vector2(targetX + d * 2.5f, floorY + 0.45f), Quaternion.identity);
                    w.Launch(new Vector2(d, 0f));
                }
            }
            yield return new WaitForSeconds(enraged ? 0.6f : 0.9f);
            yield return MoveTo(new Vector3(targetX, floorY + 8f), 0.8f);
        }

        IEnumerator FireRain()
        {
            // level with the top platform so the player can punish the attack
            yield return MoveTo(new Vector3((arenaLeft + arenaRight) * 0.5f, floorY + 8.5f), 0.9f);
            AudioManager.Play(Sfx.BossWarning);
            CameraController.Shake(0.2f, 0.6f);

            int count = enraged ? 7 : 5;
            var xs = new List<float>();
            float width = arenaRight - arenaLeft - 2f;
            float slot = width / count;
            for (int i = 0; i < count; i++)
                xs.Add(arenaLeft + 1f + slot * (i + Random.Range(0.25f, 0.75f)));
            // always leave one random slot open so the attack is avoidable
            xs.RemoveAt(Random.Range(0, xs.Count));

            var marks = new List<GameObject>();
            foreach (float x in xs) marks.Add(MakeWarning(new Vector2(x, floorY + 0.15f), new Vector2(1.6f, 0.3f)));
            yield return Flash(new Color(1f, 0.5f, 0.2f), 1.1f, true);
            foreach (var m in marks) RemoveWarning(m);

            foreach (float x in xs)
            {
                var p = Instantiate(fallingFirePrefab, new Vector2(x, ceilingY - 0.5f), Quaternion.identity);
                p.Launch(Vector2.down);
                yield return new WaitForSeconds(0.08f);
            }
            yield return new WaitForSeconds(0.6f);
        }

        IEnumerator Hover(float time)
        {
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                FacePlayer();
                yield return null;
            }
        }

        IEnumerator MoveTo(Vector3 target, float duration)
        {
            target.z = 0f;
            Vector3 from = transform.position;
            for (float t = 0; t < 1f; t += Time.deltaTime / duration)
            {
                transform.position = Vector3.Lerp(from, target, Mathf.SmoothStep(0f, 1f, t));
                FacePlayer();
                yield return null;
            }
            transform.position = target;
        }

        IEnumerator Flash(Color c, float time, bool shake = false)
        {
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                if (spriteRenderer) spriteRenderer.color = Color.Lerp(baseColor, c, Mathf.PingPong(t * 6f, 1f));
                if (shake && visual) visual.localPosition = (Vector3)(Random.insideUnitCircle * 0.06f);
                FacePlayer();
                yield return null;
            }
            if (spriteRenderer) spriteRenderer.color = baseColor;
            if (visual) visual.localPosition = Vector3.zero;
        }

        void FacePlayer()
        {
            if (player == null || spriteRenderer == null) return;
            bool playerRight = player.position.x > transform.position.x;
            spriteRenderer.flipX = spriteFacesRight ? !playerRight : playerRight;
        }

        Vector2 Mouth()
        {
            bool facingRight = spriteRenderer != null && (spriteFacesRight ? !spriteRenderer.flipX : spriteRenderer.flipX);
            return (Vector2)transform.position + new Vector2(facingRight ? 2.3f : -2.3f, 2.4f);
        }

        GameObject MakeWarning(Vector2 pos, Vector2 size)
        {
            var go = new GameObject("BossWarning");
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x * 16f, size.y * 16f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Fx.PixelSprite;
            if (warningMaterial) sr.sharedMaterial = warningMaterial;
            sr.sortingLayerName = "FX";
            sr.color = new Color(1f, 0.15f, 0.1f, 0.8f);
            go.AddComponent<WarningPulse>();
            warnings.Add(go);
            return go;
        }

        void RemoveWarning(GameObject go)
        {
            warnings.Remove(go);
            if (go) Destroy(go);
        }

        void ClearWarnings()
        {
            foreach (var w in warnings) if (w) Destroy(w);
            warnings.Clear();
        }

        public bool TakeDamage(DamageInfo info)
        {
            if (!fighting || dead) return false;
            health -= info.amount;
            HUD.SetBossHealth((float)health / maxHealth);
            AudioManager.Play(Sfx.BossHit, 0.7f);
            StartCoroutine(HitFlash());
            if (!enraged && health <= maxHealth / 2)
            {
                enraged = true;
                baseColor = new Color(1f, 0.75f, 0.75f);
                HUD.ShowMessage("The Keeper is enraged!");
                CameraController.Shake(0.3f, 0.5f);
            }
            if (health <= 0) StartCoroutine(Die());
            return true;
        }

        IEnumerator HitFlash()
        {
            if (spriteRenderer == null) yield break;
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.4f);
            yield return new WaitForSeconds(0.06f);
            if (!dead) spriteRenderer.color = baseColor;
        }

        IEnumerator Die()
        {
            if (dead) yield break;
            dead = true;
            fighting = false;
            SetVulnerable(false);
            ClearWarnings();
            if (fightRoutine != null) StopCoroutine(fightRoutine);
            if (visual) visual.localPosition = Vector3.zero;
            HUD.HideBossBar();
            AudioManager.PlayMusic(null, 1.5f);
            AudioManager.Play(Sfx.BossDie);
            var bounds = spriteRenderer != null ? spriteRenderer.bounds : new Bounds(transform.position, Vector3.one * 3f);
            for (int i = 0; i < 12; i++)
            {
                var p = new Vector2(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y));
                Fx.Burst(p, i % 2 == 0 ? new Color(1f, 0.6f, 0.2f) : new Color(1f, 0.95f, 0.6f), 14, 6f, 0.6f);
                CameraController.Shake(0.3f, 0.2f);
                AudioManager.Play(Sfx.EnemyDie, 0.6f);
                if (spriteRenderer) spriteRenderer.color = i % 2 == 0 ? Color.white : new Color(1f, 0.4f, 0.4f);
                yield return new WaitForSeconds(0.15f);
            }
            if (spriteRenderer) spriteRenderer.color = new Color(1f, 0.85f, 0.85f);
            // Its last words (only if it spoke before the fight: beats never play out of order)
            if (Story.TryBegin("boss_defeat", "boss_intro")) yield return DialogueBox.Run(StoryScript.BossDefeat, DialogueMode.Blocking);

            // It sinks to the floor at the side of the hall, wounded, and stays: it is still the Keeper of the memory,
            // and the ending needs it (StoryDirector).
            Vector3 from = transform.position;
            Color fromColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
            for (float t = 0; t < 1f; t += Time.deltaTime / 1.4f)
            {
                transform.position = Vector3.Lerp(from, RestPosition, Mathf.SmoothStep(0f, 1f, t));
                if (spriteRenderer) spriteRenderer.color = Color.Lerp(fromColor, WoundedTint, t);
                yield return null;
            }
            transform.position = RestPosition;
            if (spriteRenderer) spriteRenderer.color = WoundedTint;
            var wings = visual != null ? visual.GetComponent<SpriteFrameAnimator>() : null;
            if (wings != null) wings.fps = 1f; // tired, slow wingbeats
            AudioManager.Play(Sfx.RockImpact, 0.5f);
            CameraController.Shake(0.2f, 0.3f);
            if (treasure != null)
            {
                FaceTowards(treasure.transform.position.x);
                treasure.gameObject.SetActive(true);
                Fx.Burst(treasure.transform.position + Vector3.up * 0.6f, new Color(1f, 0.85f, 0.3f), 30, 6f, 0.8f);
                AudioManager.Play(Sfx.Key);
            }
        }

        /// <summary>Where the defeated Keeper rests: on the floor near the right wall of the hall.</summary>
        public Vector3 RestPosition => new Vector3(arenaRight - 3.5f, floorY, 0f);

        /// <summary>The Keeper's colour after its defeat (dim and grey).</summary>
        public static readonly Color WoundedTint = new Color(0.55f, 0.5f, 0.6f);

        public void FaceTowards(float x)
        {
            if (spriteRenderer == null) return;
            bool right = x > transform.position.x;
            spriteRenderer.flipX = spriteFacesRight ? !right : right;
        }
    }
}
