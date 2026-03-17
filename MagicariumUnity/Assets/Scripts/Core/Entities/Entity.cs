using System;

namespace Magicarium.Entities
{
    /// <summary>
    /// Base class for all game entities (units and buildings) that have health and a position.
    /// </summary>
    public abstract class Entity
    {
        private int _health;

        public int X { get; protected set; }
        public int Y { get; protected set; }

        public int MaxHealth { get; }

        public int Health
        {
            get => _health;
            protected set => _health = Math.Max(0, value);
        }

        public bool IsAlive => Health > 0;

        /// <summary>Identifier of the player that owns this entity (1-based; 0 = neutral).</summary>
        public int OwnerId { get; }

        protected Entity(int ownerId, int x, int y, int maxHealth)
        {
            if (maxHealth <= 0) throw new ArgumentOutOfRangeException("maxHealth");

            OwnerId = ownerId;
            X = x;
            Y = y;
            MaxHealth = maxHealth;
            Health = maxHealth;
        }

        /// <summary>
        /// Applies damage to this entity. Returns true if the entity died as a result.
        /// </summary>
        public bool TakeDamage(int damage)
        {
            if (damage < 0) throw new ArgumentOutOfRangeException("damage");
            Health -= damage;
            return !IsAlive;
        }
    }
}
