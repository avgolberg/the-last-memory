using UnityEngine;
using UnityEngine.InputSystem;

namespace LastMemory
{
    /// <summary>
    /// Gameplay input built in code on top of the Input System, so no scene wiring is needed.
    ///
    /// Keyboard (primary): arrows move, Up arrow = context jump (climbs when on a ladder,
    /// aims when combined with X), Z / Space = jump, X = shoot, C = shoot straight up, Enter = examine, Esc = pause.
    /// Dialogue advances with Z / Space / X / C / Enter.
    /// Gamepad: stick/D-pad move and climb, South = jump, West = shoot, Right shoulder = shoot up,
    /// North = examine, Start = pause.
    ///
    /// The Up arrow is reported separately (<see cref="UpJumpPressed"/>) so the player controller can
    /// decide between jumping, climbing and aiming. Tests drive the player through <see cref="Simulated"/>.
    /// </summary>
    public static class GameInput
    {
        public struct SimulatedInput
        {
            public bool Enabled;
            public Vector2 Move;
            /// <summary>Z / Space / South held.</summary>
            public bool JumpHeld;
            /// <summary>Z / Space / South pressed this frame.</summary>
            public bool JumpPressed;
            /// <summary>Up arrow pressed this frame (also set Move.y = 1 to hold it).</summary>
            public bool UpPressed;
            public bool ShootPressed;
            /// <summary>C / gamepad right shoulder pressed this frame (shoot straight up).</summary>
            public bool ShootUpPressed;
            /// <summary>Enter / gamepad North pressed this frame (examine).</summary>
            public bool InteractPressed;
            /// <summary>A dialogue-advance key pressed this frame.</summary>
            public bool AdvancePressed;
            public bool PausePressed;
        }

        public static SimulatedInput Simulated;

        static InputAction move, jump, upKey, shoot, shootUp, pause, interact, advance;
        static bool initialized;

        static void Init()
        {
            if (initialized) return;
            initialized = true;

            move = new InputAction("Move", InputActionType.Value);
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            move.AddBinding("<Gamepad>/leftStick");
            move.AddBinding("<Gamepad>/dpad");

            jump = new InputAction("Jump", InputActionType.Button);
            jump.AddBinding("<Keyboard>/z");
            jump.AddBinding("<Keyboard>/space");
            jump.AddBinding("<Gamepad>/buttonSouth");

            upKey = new InputAction("UpJump", InputActionType.Button);
            upKey.AddBinding("<Keyboard>/upArrow");

            shoot = new InputAction("Shoot", InputActionType.Button);
            shoot.AddBinding("<Keyboard>/x");
            shoot.AddBinding("<Gamepad>/buttonWest");

            // A dedicated key for upward shots: no timing with the Up arrow needed (Up + X still works too).
            shootUp = new InputAction("ShootUp", InputActionType.Button);
            shootUp.AddBinding("<Keyboard>/c");
            shootUp.AddBinding("<Gamepad>/rightShoulder");

            pause = new InputAction("Pause", InputActionType.Button);
            pause.AddBinding("<Keyboard>/escape");
            pause.AddBinding("<Gamepad>/start");

            interact = new InputAction("Interact", InputActionType.Button);
            interact.AddBinding("<Keyboard>/enter");
            interact.AddBinding("<Keyboard>/numpadEnter");
            interact.AddBinding("<Gamepad>/buttonNorth");

            advance = new InputAction("Advance", InputActionType.Button);
            foreach (var key in new[] { "z", "space", "x", "c", "enter", "numpadEnter" }) advance.AddBinding("<Keyboard>/" + key);
            advance.AddBinding("<Gamepad>/buttonSouth");
            advance.AddBinding("<Gamepad>/buttonEast");
            advance.AddBinding("<Gamepad>/buttonWest");

            move.Enable(); jump.Enable(); upKey.Enable(); shoot.Enable(); shootUp.Enable(); pause.Enable(); interact.Enable(); advance.Enable();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            initialized = false;
            Simulated = default;
        }

        public static Vector2 Move
        {
            get
            {
                if (Simulated.Enabled) return Simulated.Move;
                Init();
                return move.ReadValue<Vector2>();
            }
        }

        /// <summary>A dedicated jump button (Z, Space, gamepad South) was pressed.</summary>
        public static bool JumpPressed
        {
            get
            {
                if (Simulated.Enabled) { bool v = Simulated.JumpPressed; Simulated.JumpPressed = false; return v; }
                Init();
                return jump.WasPressedThisFrame();
            }
        }

        /// <summary>The keyboard Up arrow was pressed (jump, climb or aim depending on context).</summary>
        public static bool UpJumpPressed
        {
            get
            {
                if (Simulated.Enabled) { bool v = Simulated.UpPressed; Simulated.UpPressed = false; return v; }
                Init();
                return upKey.WasPressedThisFrame();
            }
        }

        /// <summary>Any jump input held (for variable jump height): jump buttons or the Up arrow.</summary>
        public static bool JumpHeld
        {
            get
            {
                if (Simulated.Enabled) return Simulated.JumpHeld || Simulated.Move.y > 0.5f;
                Init();
                return jump.IsPressed() || upKey.IsPressed();
            }
        }

        public static bool ShootPressed
        {
            get
            {
                if (Simulated.Enabled) { bool v = Simulated.ShootPressed; Simulated.ShootPressed = false; return v; }
                Init();
                return shoot.WasPressedThisFrame();
            }
        }

        /// <summary>Shoot straight up (C, gamepad right shoulder).</summary>
        public static bool ShootUpPressed
        {
            get
            {
                if (Simulated.Enabled) { bool v = Simulated.ShootUpPressed; Simulated.ShootUpPressed = false; return v; }
                Init();
                return shootUp.WasPressedThisFrame();
            }
        }

        public static bool PausePressed
        {
            get
            {
                if (Simulated.Enabled) { bool v = Simulated.PausePressed; Simulated.PausePressed = false; return v; }
                Init();
                return pause.WasPressedThisFrame();
            }
        }

        /// <summary>Examine something (Enter, gamepad North).</summary>
        public static bool InteractPressed
        {
            get
            {
                if (Simulated.Enabled) { bool v = Simulated.InteractPressed; Simulated.InteractPressed = false; return v; }
                Init();
                return interact.WasPressedThisFrame();
            }
        }

        /// <summary>Advance dialogue (Z, Space, X, C, Enter, or a gamepad face button).</summary>
        public static bool AdvancePressed
        {
            get
            {
                if (Simulated.Enabled) { bool v = Simulated.AdvancePressed; Simulated.AdvancePressed = false; return v; }
                Init();
                return advance.WasPressedThisFrame();
            }
        }
    }
}
