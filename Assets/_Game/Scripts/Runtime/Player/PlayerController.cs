using UnityEngine;

namespace LastMemory
{
    /// <summary>
    /// Platformer movement: acceleration-based running, buffered/coyote jumps with variable height,
    /// ladder climbing, one-way platform drop-through, and projectile shooting.
    /// Input is read in Update, physics is applied in FixedUpdate.
    /// The transform pivot is at the feet; the sprite lives on the child "Visual".
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Run")]
        public float moveSpeed = 7.5f;
        public float groundAcceleration = 90f;
        public float groundDeceleration = 100f;
        public float airAcceleration = 55f;
        public float airDeceleration = 40f;

        [Header("Jump")]
        public float jumpVelocity = 16.5f;
        public float gravityScale = 4f;
        public float fallGravityMultiplier = 1.6f;
        public float jumpCutMultiplier = 0.45f;
        public float maxFallSpeed = 22f;
        public float coyoteTime = 0.1f;
        public float jumpBufferTime = 0.12f;
        [Tooltip("Up-arrow jumps wait this long so Up + X can fire upward without jumping.")]
        public float upJumpGrace = 0.09f;

        [Header("Climb")]
        public float climbSpeed = 4.5f;

        [Header("Shoot")]
        public Projectile projectilePrefab;
        public float shootCooldown = 0.3f;
        public Vector2 muzzleOffset = new Vector2(0.6f, 0.62f);

        [Header("References")]
        public Transform visual;
        public Animator animator;

        static readonly int IdleState = Animator.StringToHash("Idle");
        static readonly int RunState = Animator.StringToHash("Run");
        static readonly int JumpState = Animator.StringToHash("Jump");
        static readonly int ApexState = Animator.StringToHash("Apex");
        static readonly int FallState = Animator.StringToHash("Fall");
        static readonly int ClimbState = Animator.StringToHash("Climb");
        static readonly int AttackState = Animator.StringToHash("Attack");
        static readonly int HurtState = Animator.StringToHash("Hurt");

        Rigidbody2D rb;
        Vector2 input;
        float lastGroundedTime = -10f, lastJumpPressedTime = -10f, jumpOriginTime = -10f, pendingUpJump = -1f;
        float shootTimer, attackAnimTimer, knockbackTimer, dropThroughTimer, hurtAnimTimer;
        bool jumpHeld, wasGrounded;
        int currentAnim;
        float prevVelocityY;
        Vector2 squash = Vector2.one;

        public bool IsGrounded { get; private set; }
        public bool IsClimbing { get; private set; }
        public int Facing { get; private set; } = 1;
        public bool ControlEnabled { get; private set; } = true;
        public Rigidbody2D Body => rb;

        /// <summary>Cutscenes walk the hero with this (-1..1) while player control is off.</summary>
        public float ScriptedMoveX { get; set; }

        /// <summary>
        /// Returns the player if <paramref name="other"/> belongs to the hero, otherwise null. Triggers use this to
        /// recognise the player by component instead of by tag. (The hero's colliders sit on the same object as its
        /// Rigidbody2D, so the rigidbody tells us who owns the collider.)
        /// </summary>
        public static PlayerController FromCollider(Collider2D other)
        {
            if (other.attachedRigidbody == null) return null;
            return other.attachedRigidbody.GetComponent<PlayerController>();
        }

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = gravityScale;
        }

        void OnDisable() => SetOneWayIgnored(false);

        public void SetControl(bool enabled)
        {
            ControlEnabled = enabled;
            ScriptedMoveX = 0f;
            if (!enabled)
            {
                input = Vector2.zero;
                if (IsClimbing) StopClimbing();
            }
        }

        public void Teleport(Vector2 position)
        {
            StopClimbing();
            rb.position = position;
            transform.position = position;
            rb.linearVelocity = Vector2.zero;
            knockbackTimer = 0f;
        }

        public void Knockback(Vector2 velocity, float duration)
        {
            if (IsClimbing) StopClimbing();
            rb.linearVelocity = velocity;
            knockbackTimer = duration;
            hurtAnimTimer = 0.35f;
        }

        public void SetFacing(int dir)
        {
            if (dir != 0) Facing = dir > 0 ? 1 : -1;
        }

        void Update()
        {
            if (Time.timeScale == 0f) return;
            float dt = Time.deltaTime;
            shootTimer -= dt; attackAnimTimer -= dt; dropThroughTimer -= dt; hurtAnimTimer -= dt;

            if (ControlEnabled)
            {
                input = GameInput.Move;
                jumpHeld = GameInput.JumpHeld;
                ReadActions();
                if (knockbackTimer <= 0f && Mathf.Abs(input.x) > 0.2f && !IsClimbing) SetFacing((int)Mathf.Sign(input.x));
                UpdateLadder();
            }
            else
            {
                input = new Vector2(ScriptedMoveX, 0f);
                jumpHeld = false;
                if (Mathf.Abs(ScriptedMoveX) > 0.01f) SetFacing((int)Mathf.Sign(ScriptedMoveX));
            }

            UpdateVisual(dt);
        }

        /// <summary>
        /// Resolves the context-sensitive keyboard actions:
        ///  - Up arrow on a ladder climbs instead of jumping.
        ///  - Up arrow elsewhere jumps after a tiny grace period; pressing X in that window
        ///    (Up + X) fires upward and cancels the jump.
        ///  - C always fires straight up and never changes movement (no timing needed).
        ///  - Z / Space always jump immediately (Down + Z / Space drops through one-way ledges).
        /// </summary>
        void ReadActions()
        {
            if (GameInput.UpJumpPressed)
            {
                if (IsClimbing) { } // Up just keeps climbing
                else if (FindLadderColumn(BodyProbe(), out _)) StartClimbing();
                else pendingUpJump = Time.time;
            }

            if (GameInput.ShootUpPressed)
            {
                TryShoot(aimUp: true);
            }
            else if (GameInput.ShootPressed)
            {
                bool aimUp = input.y > 0.5f;
                if (aimUp && pendingUpJump >= 0f) pendingUpJump = -1f; // Up + X: aim, do not jump
                TryShoot(aimUp);
            }

            if (pendingUpJump >= 0f && (Time.time - pendingUpJump >= upJumpGrace || input.y <= 0.5f))
            {
                RequestJump(pendingUpJump);
                pendingUpJump = -1f;
            }

            if (GameInput.JumpPressed) RequestJump(Time.time);
        }

        void RequestJump(float pressTime)
        {
            lastJumpPressedTime = Time.time;
            jumpOriginTime = pressTime;
        }

        void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            knockbackTimer -= dt;
            CheckGround();

            if (IsClimbing)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = new Vector2(0f, input.y * climbSpeed);
                if (Time.time - lastJumpPressedTime <= jumpBufferTime)
                {
                    lastJumpPressedTime = -10f;
                    StopClimbing();
                    rb.linearVelocity = new Vector2(input.x * moveSpeed, jumpVelocity * 0.75f);
                    OnJumped();
                }
                return;
            }

            var v = rb.linearVelocity;

            // Horizontal movement
            if (knockbackTimer <= 0f)
            {
                float target = input.x * moveSpeed;
                bool accelerating = Mathf.Abs(target) > 0.01f;
                // Speed up / slow down more sharply on the ground than in the air.
                float rate;
                if (IsGrounded) rate = accelerating ? groundAcceleration : groundDeceleration;
                else rate = accelerating ? airAcceleration : airDeceleration;
                v.x = Mathf.MoveTowards(v.x, target, rate * dt);
            }

            // Jump (buffered + coyote time); Down + Jump on a one-way platform drops through it
            // buffered: the key was pressed recently (it may have been just before landing).
            // canJump: the hero was on the ground when the key was pressed, or left it at most coyoteTime earlier.
            bool buffered = Time.time - lastJumpPressedTime <= jumpBufferTime;
            bool canJump = lastGroundedTime >= jumpOriginTime - coyoteTime;
            if (buffered && canJump && ControlEnabled)
            {
                lastJumpPressedTime = -10f;
                if (input.y < -0.5f && StandingOnOneWay())
                {
                    dropThroughTimer = 0.3f;
                    SetOneWayIgnored(true);
                    lastGroundedTime = -10f;
                }
                else
                {
                    v.y = jumpVelocity;
                    lastGroundedTime = -10f;
                    OnJumped();
                }
            }

            // Variable jump height and snappier falling
            float g = gravityScale;
            if (v.y < 0f) g *= fallGravityMultiplier;
            else if (v.y > 0f && !jumpHeld && knockbackTimer <= 0f) v.y *= Mathf.Pow(jumpCutMultiplier, dt * 10f);
            rb.gravityScale = g;
            v.y = Mathf.Max(v.y, -maxFallSpeed);

            if (dropThroughTimer <= 0f && !IsClimbing) SetOneWayIgnored(false);

            prevVelocityY = v.y;
            rb.linearVelocity = v;
        }

        void CheckGround()
        {
            int mask = dropThroughTimer > 0f ? Layers.SolidMask : Layers.GroundMask;
            bool hit = false;
            // Rising bodies are never grounded, so jumping up through one-way platforms doesn't re-ground mid-jump.
            if (rb.linearVelocity.y <= 0.1f)
            {
                var box = Physics2D.OverlapBox((Vector2)transform.position + new Vector2(0f, -0.03f), new Vector2(0.55f, 0.08f), 0f, mask);
                hit = box != null;
            }
            IsGrounded = hit;
            if (IsGrounded)
            {
                lastGroundedTime = Time.time;
                if (!wasGrounded && prevVelocityY < -4f) OnLanded(prevVelocityY);
            }
            wasGrounded = IsGrounded;
        }

        bool StandingOnOneWay()
        {
            var solid = Physics2D.OverlapBox((Vector2)transform.position + new Vector2(0f, -0.03f), new Vector2(0.55f, 0.08f), 0f, Layers.SolidMask);
            if (solid != null) return false;
            return Physics2D.OverlapBox((Vector2)transform.position + new Vector2(0f, -0.03f), new Vector2(0.55f, 0.08f), 0f, 1 << Layers.OneWay) != null;
        }

        const float LadderGrabTolerance = 0.3f; // how far the body centre may be from a ladder column

        Vector2 BodyProbe() => (Vector2)transform.position + new Vector2(0f, 0.7f);
        Vector2 FeetProbe() => (Vector2)transform.position + new Vector2(0f, -0.3f);
        const float FeetProbeHeight = 0.4f; // only looks below the feet

        /// <summary>Finds a ladder column overlapping the probe, allowing a little horizontal slack.</summary>
        static bool FindLadderColumn(Vector2 probe, out float columnX, float height = 1.0f)
        {
            foreach (float dx in new[] { 0f, -LadderGrabTolerance, LadderGrabTolerance })
            {
                // Ladders are one tile wide on an integer grid: test the candidate column at its centre.
                float cx = Mathf.Floor(probe.x + dx) + 0.5f;
                if (Physics2D.OverlapBox(new Vector2(cx, probe.y), new Vector2(0.05f, height), 0f, Layers.LadderMask) != null)
                {
                    columnX = cx;
                    return true;
                }
            }
            columnX = 0f;
            return false;
        }

        void UpdateLadder()
        {
            Vector2 pos = transform.position;

            if (!IsClimbing)
            {
                if (knockbackTimer > 0f) return;
                // Down: on the ground, look for a ladder below the feet (standing on top of it);
                // in the air, look for a ladder the body overlaps.
                bool down = false;
                if (input.y < -0.5f)
                {
                    if (IsGrounded) down = FindLadderColumn(FeetProbe(), out _, FeetProbeHeight);
                    else down = FindLadderColumn(BodyProbe(), out _);
                }
                // Up: holding Up while overlapping a ladder (e.g. after jumping onto it) grabs it.
                bool up = input.y > 0.5f && FindLadderColumn(BodyProbe(), out _);
                if (down || up) StartClimbing();
                return;
            }

            bool bodyOnLadder = Physics2D.OverlapBox(BodyProbe(), new Vector2(0.3f, 1.2f), 0f, Layers.LadderMask) != null;
            bool ladderBelow = Physics2D.OverlapPoint(FeetProbe(), Layers.LadderMask) != null;
            if (!bodyOnLadder && !ladderBelow) { StopClimbing(); return; }                           // climbed off the top
            if (Mathf.Abs(input.x) > 0.6f && Mathf.Abs(input.y) < 0.3f) { StopClimbing(); return; }  // step off sideways
            if (input.y < -0.5f)
            {
                // Reached the bottom: no more ladder under the feet and something to stand on
                // (solid ground or a one-way ledge, which must become solid again right away).
                bool ladderUnderFeet = Physics2D.OverlapPoint(pos + new Vector2(0f, -0.05f), Layers.LadderMask) != null;
                bool standable = Physics2D.OverlapBox(pos + new Vector2(0f, -0.03f), new Vector2(0.55f, 0.08f), 0f, Layers.GroundMask) != null;
                if (!ladderUnderFeet && standable)
                {
                    StopClimbing();
                    dropThroughTimer = 0f;
                    SetOneWayIgnored(false);
                }
            }
        }

        void StartClimbing()
        {
            if (!FindLadderColumn(BodyProbe(), out float column) && !FindLadderColumn(FeetProbe(), out column, FeetProbeHeight)) return;
            IsClimbing = true;
            pendingUpJump = -1f;
            lastJumpPressedTime = -10f;
            SetOneWayIgnored(true);
            var p = rb.position;
            p.x = column;
            rb.position = p;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }

        void StopClimbing()
        {
            if (!IsClimbing) return;
            IsClimbing = false;
            rb.gravityScale = gravityScale;
            dropThroughTimer = 0.05f;
        }

        void SetOneWayIgnored(bool ignored)
        {
            if (Physics2D.GetIgnoreLayerCollision(Layers.Player, Layers.OneWay) != ignored)
                Physics2D.IgnoreLayerCollision(Layers.Player, Layers.OneWay, ignored);
        }

        void TryShoot(bool aimUp)
        {
            if (shootTimer > 0f || IsClimbing || projectilePrefab == null) return;
            shootTimer = shootCooldown;
            attackAnimTimer = 0.25f;
            // origin: a point inside the hero's body; spawn: where the projectile appears.
            Vector2 direction, origin, spawn;
            if (aimUp)
            {
                // C (or Up + X): straight up from above the head (useful against bats and the boss).
                direction = Vector2.up;
                origin = (Vector2)transform.position + new Vector2(0f, 1.2f);
                spawn = origin + Vector2.up * 0.5f;
            }
            else
            {
                direction = new Vector2(Facing, 0f);
                origin = (Vector2)transform.position + new Vector2(0f, muzzleOffset.y);
                spawn = origin + new Vector2(muzzleOffset.x * Facing, 0f);
            }
            AudioManager.Play(Sfx.Shoot);

            // Never spawn inside a wall (or behind it): just show a spark when point-blank against one.
            if (Physics2D.Linecast(origin, spawn, Layers.SolidMask))
            {
                Fx.Burst(origin + direction * 0.3f, new Color(0.8f, 0.9f, 1f), 5, 2.5f, 0.2f, 0f, 1.5f);
                return;
            }
            var projectile = Instantiate(projectilePrefab, spawn, Quaternion.identity);
            projectile.Launch(direction);
        }

        void OnJumped()
        {
            AudioManager.Play(Sfx.Jump, 0.6f);
            Fx.Dust(transform.position, 5);
            squash = new Vector2(0.8f, 1.2f);
        }

        void OnLanded(float impactVelocity)
        {
            AudioManager.Play(Sfx.Land, Mathf.Clamp01(-impactVelocity / 20f));
            Fx.Dust(transform.position, 6);
            squash = new Vector2(1.2f, 0.8f);
        }

        void UpdateVisual(float dt)
        {
            if (visual == null) return;
            squash = Vector2.MoveTowards(squash, Vector2.one, dt * 4f);
            visual.localScale = new Vector3(Facing * squash.x, squash.y, 1f);
            if (animator == null) return;

            // Pick the animation from the hero's state, most important first.
            int state;
            var v = rb.linearVelocity;
            if (hurtAnimTimer > 0f) state = HurtState;
            else if (IsClimbing) state = ClimbState;
            else if (attackAnimTimer > 0f) state = AttackState;
            else if (!IsGrounded)
            {
                // In the air: rising fast, near the top of the jump, or falling.
                if (v.y > 3f) state = JumpState;
                else if (v.y > -3f) state = ApexState;
                else state = FallState;
            }
            else if (Mathf.Abs(v.x) > 0.5f) state = RunState;
            else state = IdleState;

            // The climbing animation pauses while the hero holds still on a ladder.
            bool holdingStillOnLadder = state == ClimbState && Mathf.Abs(v.y) <= 0.1f;
            animator.speed = holdingStillOnLadder ? 0f : 1f;
            if (state != currentAnim)
            {
                currentAnim = state;
                animator.Play(state, 0, 0f);
            }
        }
    }
}
