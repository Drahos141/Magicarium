using System;
using System.Collections.Generic;

namespace Magicarium.Resources
{
    /// <summary>
    /// Tracks the amounts of each resource type owned by a player.
    /// </summary>
    public class ResourceInventory
    {
        private readonly Dictionary<ResourceType, int> _resources = new Dictionary<ResourceType, int>();

        public ResourceInventory()
        {
            // Unity-compatible: cast instead of Enum.GetValues<T>()
            foreach (ResourceType type in (ResourceType[])Enum.GetValues(typeof(ResourceType)))
                _resources[type] = 0;
        }

        public int Get(ResourceType type) => _resources[type];

        public void Add(ResourceType type, int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException("amount", "Amount must be non-negative.");
            _resources[type] += amount;
        }

        public bool TrySpend(ResourceType type, int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException("amount", "Amount must be non-negative.");
            if (_resources[type] < amount) return false;
            _resources[type] -= amount;
            return true;
        }

        public bool CanAfford(ResourceType type, int amount) => _resources[type] >= amount;

        public bool CanAfford(IReadOnlyDictionary<ResourceType, int> cost)
        {
            foreach (var kvp in cost)
                if (!CanAfford(kvp.Key, kvp.Value)) return false;
            return true;
        }

        public bool TrySpend(IReadOnlyDictionary<ResourceType, int> cost)
        {
            if (!CanAfford(cost)) return false;
            foreach (var kvp in cost)
                _resources[kvp.Key] -= kvp.Value;
            return true;
        }
    }
}
