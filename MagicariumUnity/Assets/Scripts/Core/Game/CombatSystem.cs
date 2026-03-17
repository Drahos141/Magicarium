using Magicarium.Entities;

namespace Magicarium.Game
{
    /// <summary>
    /// Handles combat between units and buildings.
    /// </summary>
    public static class CombatSystem
    {
        /// <summary>
        /// Orders <paramref name="attacker"/> to attack <paramref name="target"/>.
        /// Returns true if the target was destroyed as a result.
        /// </summary>
        public static bool Attack(Unit attacker, Entity target)
        {
            if (!attacker.IsAlive) return false;
            if (!target.IsAlive) return false;
            if (attacker.OwnerId == target.OwnerId) return false;

            return attacker.Attack(target);
        }

        /// <summary>
        /// Checks whether <paramref name="attacker"/> is in range to attack <paramref name="target"/>.
        /// </summary>
        public static bool IsInRange(Unit attacker, Entity target)
        {
            int dx = System.Math.Abs(target.X - attacker.X);
            int dy = System.Math.Abs(target.Y - attacker.Y);
            return (dx + dy) <= attacker.AttackRange;
        }
    }
}
