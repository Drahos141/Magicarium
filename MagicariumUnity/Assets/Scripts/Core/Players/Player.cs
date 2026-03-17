using System;
using System.Collections.Generic;
using System.Linq;
using Magicarium.Buildings;
using Magicarium.Entities;
using Magicarium.Map;
using Magicarium.Resources;

namespace Magicarium.Players
{
    /// <summary>
    /// Represents a player (human or AI) in the game.
    /// Each player starts with one MainBase and three Workers.
    /// </summary>
    public class Player
    {
        public int Id { get; }
        public string Name { get; }
        public bool IsAI { get; }
        public bool IsDefeated =>
            !Buildings.Any(b => b.Type == BuildingType.MainBase && b.IsAlive)
            && !Units.Any(u => u.IsAlive);

        public ResourceInventory Resources { get; } = new ResourceInventory();

        private readonly List<Unit> _units = new List<Unit>();
        private readonly List<Building> _buildings = new List<Building>();

        public IReadOnlyList<Unit> Units => _units;
        public IReadOnlyList<Building> Buildings => _buildings;

        public Player(int id, string name, bool isAI = false)
        {
            Id = id;
            Name = name;
            IsAI = isAI;
        }

        // ── Initialisation ──────────────────────────────────────────

        /// <summary>
        /// Places the player's starting entities: one MainBase and three Workers.
        /// </summary>
        public void PlaceStartingEntities(int baseX, int baseY)
        {
            _buildings.Add(new Building(Id, baseX, baseY, BuildingType.MainBase));

            var workerOffsets = new[] { new int[] { 1, 0 }, new int[] { 2, 0 }, new int[] { 0, 1 } };
            foreach (var offset in workerOffsets)
                _units.Add(new Worker(Id, baseX + offset[0], baseY + offset[1]));
        }

        // ── Construction ────────────────────────────────────────────

        /// <summary>
        /// Attempts to construct a building at the given position.
        /// Deducts resources on success. Returns null if resources are insufficient.
        /// </summary>
        public Building Build(BuildingType type, int x, int y, GameMap map)
        {
            if (!map.IsInBounds(x, y)) return null;
            var tile = map.GetTile(x, y);
            if (!tile.IsPassable) return null;

            var cost = Building.GetCost(type);
            if (!Resources.TrySpend(cost)) return null;

            var building = new Building(Id, x, y, type);
            _buildings.Add(building);

            if (type == BuildingType.Road)
                tile.Terrain = TerrainType.Road;

            return building;
        }

        // ── Unit management ─────────────────────────────────────────

        public void AddUnit(Unit unit) => _units.Add(unit);

        public void RemoveDefeatedEntities()
        {
            _units.RemoveAll(u => !u.IsAlive);
            _buildings.RemoveAll(b => !b.IsAlive);
        }

        // ── Resource helpers ────────────────────────────────────────

        /// <summary>
        /// Adds resource income from shrines and mills each tick.
        /// </summary>
        public void CollectPassiveIncome()
        {
            int shrineCount = _buildings.Count(b => b.Type == BuildingType.Shrine && b.IsAlive);
            if (shrineCount > 0)
                Resources.Add(ResourceType.MagicOre, shrineCount * 5);

            int millCount = _buildings.Count(b => b.Type == BuildingType.Mill && b.IsAlive);
            int farmCount = _buildings.Count(b => b.Type == BuildingType.Farm && b.IsAlive);
            if (millCount > 0 && farmCount > 0)
                Resources.Add(ResourceType.Gold, millCount * farmCount * 2);
        }
    }
}
