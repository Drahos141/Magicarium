using System;
using Magicarium.Map;

namespace Magicarium.Entities
{
    /// <summary>
    /// A movable, attackable game unit.
    /// </summary>
    public class Unit : Entity
    {
        public UnitType Type { get; }

        /// <summary>Base movement speed in tiles per turn.</summary>
        public float BaseSpeed { get; }

        /// <summary>Attack damage dealt per strike.</summary>
        public int AttackDamage { get; }

        /// <summary>Melee attack range in tiles (1 = adjacent only).</summary>
        public int AttackRange { get; }

        public Unit(int ownerId, int x, int y, UnitType type)
            : base(ownerId, x, y, GetMaxHealth(type))
        {
            Type = type;
            BaseSpeed = GetBaseSpeed(type);
            AttackDamage = GetAttackDamage(type);
            AttackRange = GetAttackRange(type);
        }

        /// <summary>
        /// Moves the unit to the specified tile.
        /// Returns false if the tile is not passable.
        /// </summary>
        public bool MoveTo(GameMap map, int targetX, int targetY)
        {
            if (!map.IsInBounds(targetX, targetY)) return false;
            var tile = map.GetTile(targetX, targetY);
            if (!tile.IsPassable) return false;

            X = targetX;
            Y = targetY;
            return true;
        }

        /// <summary>
        /// Returns the effective speed of this unit considering road bonuses on the current tile.
        /// </summary>
        public float GetEffectiveSpeed(GameMap map)
        {
            if (!map.IsInBounds(X, Y)) return BaseSpeed;
            var tile = map.GetTile(X, Y);
            return BaseSpeed * tile.SpeedMultiplier;
        }

        /// <summary>
        /// Attacks a target entity. Returns true if the target died.
        /// </summary>
        public bool Attack(Entity target)
        {
            int dx = Math.Abs(target.X - X);
            int dy = Math.Abs(target.Y - Y);
            int distance = dx + dy;

            if (distance > AttackRange) return false;

            return target.TakeDamage(AttackDamage);
        }

        private static int GetMaxHealth(UnitType type)
        {
            switch (type)
            {
                case UnitType.Worker:  return 60;
                case UnitType.Soldier: return 100;
                case UnitType.Archer:  return 80;
                case UnitType.Knight:  return 150;
                default:               return 80;
            }
        }

        private static float GetBaseSpeed(UnitType type)
        {
            switch (type)
            {
                case UnitType.Worker:  return 2.0f;
                case UnitType.Soldier: return 2.0f;
                case UnitType.Archer:  return 2.5f;
                case UnitType.Knight:  return 1.5f;
                default:               return 2.0f;
            }
        }

        private static int GetAttackDamage(UnitType type)
        {
            switch (type)
            {
                case UnitType.Worker:  return 5;
                case UnitType.Soldier: return 15;
                case UnitType.Archer:  return 12;
                case UnitType.Knight:  return 25;
                default:               return 10;
            }
        }

        private static int GetAttackRange(UnitType type)
        {
            return type == UnitType.Archer ? 3 : 1;
        }
    }
}
