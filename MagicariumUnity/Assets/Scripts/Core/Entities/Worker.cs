using System;
using Magicarium.Map;
using Magicarium.Resources;

namespace Magicarium.Entities
{
    /// <summary>
    /// A worker unit that can gather resources from mine and forest tiles.
    /// </summary>
    public class Worker : Unit
    {
        /// <summary>Amount of resources the worker can carry before returning to base.</summary>
        public int CarryCapacity { get; } = 20;

        /// <summary>Resources currently being carried.</summary>
        public int CarriedAmount { get; private set; }
        public ResourceType? CarriedResource { get; private set; }

        public Worker(int ownerId, int x, int y)
            : base(ownerId, x, y, UnitType.Worker) { }

        /// <summary>
        /// Gathers resources from the tile at the worker's current position.
        /// Returns the amount gathered (0 if nothing to gather or carry is full).
        /// </summary>
        public int Gather(GameMap map)
        {
            var tile = map.GetTile(X, Y);
            if (tile.ResourceAmount <= 0 || tile.ResourceYield == null) return 0;
            if (CarriedAmount >= CarryCapacity) return 0;
            if (CarriedResource != null && CarriedResource != tile.ResourceYield) return 0;

            int space = CarryCapacity - CarriedAmount;
            int amount = Math.Min(space, Math.Min(tile.ResourceAmount, 5));

            tile.ResourceAmount -= amount;
            CarriedAmount += amount;
            CarriedResource = tile.ResourceYield;
            return amount;
        }

        /// <summary>
        /// Deposits all carried resources into the given inventory.
        /// Returns the type and amount deposited; null if nothing carried.
        /// </summary>
        public DepositResult? Deposit(ResourceInventory inventory)
        {
            if (CarriedAmount == 0 || CarriedResource == null) return null;

            var result = new DepositResult(CarriedResource.Value, CarriedAmount);
            inventory.Add(CarriedResource.Value, CarriedAmount);
            CarriedAmount = 0;
            CarriedResource = null;
            return result;
        }
    }

    /// <summary>Result of a worker deposit action.</summary>
    public struct DepositResult
    {
        public ResourceType Type { get; }
        public int Amount { get; }
        public DepositResult(ResourceType type, int amount) { Type = type; Amount = amount; }
    }
}
