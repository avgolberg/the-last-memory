using UnityEngine;

namespace LastMemory
{
    /// <summary>
    /// Physics layer indices and the 2D collision matrix the game relies on.
    /// The matrix is enforced at startup so projectiles can never hit the player
    /// that fired them, trigger level exits, or collide with pickups.
    /// </summary>
    public static class Layers
    {
        public const int Default = 0;
        public const int Ground = 6;
        public const int Ladder = 7;
        public const int Player = 8;
        public const int Background = 9;
        public const int OneWay = 10;
        public const int Enemy = 11;
        public const int Hazards = 12;
        public const int PlayerProjectile = 13;
        public const int Interactables = 14;
        public const int EnemyProjectile = 15;

        public static readonly int GroundMask = (1 << Ground) | (1 << OneWay);
        public static readonly int SolidMask = 1 << Ground;
        public static readonly int LadderMask = 1 << Ladder;
        public static readonly int PlayerMask = 1 << Player;
        public static readonly int EnemyMask = 1 << Enemy;

        static readonly int[] GameLayers =
            { Ground, Ladder, Player, Background, OneWay, Enemy, Hazards, PlayerProjectile, Interactables, EnemyProjectile };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ApplyCollisionMatrix()
        {
            foreach (int a in GameLayers)
                foreach (int b in GameLayers)
                    Physics2D.IgnoreLayerCollision(a, b, !ShouldCollide(a, b));
        }

        /// <summary>The only pairs of game layers that touch each other. Every other pair is ignored.</summary>
        static readonly int[,] CollidingPairs =
        {
            { Player, Ground }, { Player, OneWay }, { Player, Ladder }, { Player, Enemy },
            { Player, Hazards }, { Player, Interactables }, { Player, EnemyProjectile },
            { PlayerProjectile, Ground }, { PlayerProjectile, Enemy },   // the hero's shots hit walls and enemies only
            { EnemyProjectile, Ground },                                 // enemy shots hit walls (and the player, above)
            { Enemy, Ground },
        };

        public static bool ShouldCollide(int a, int b)
        {
            for (int i = 0; i < CollidingPairs.GetLength(0); i++)
            {
                int x = CollidingPairs[i, 0], y = CollidingPairs[i, 1];
                if ((a == x && b == y) || (a == y && b == x)) return true; // the order of the two layers doesn't matter
            }
            return false;
        }
    }
}
