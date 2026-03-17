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
    /// Simple AI player that gathers resources with workers and attacks enemies when ready.
    /// </summary>
    public class AIPlayer : Player
    {
        private readonly GameMap _map;
        private readonly Random _rng;

        public AIPlayer(int id, string name, GameMap map, int? seed = null)
            : base(id, name, isAI: true)
        {
            _map = map;
            _rng = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        /// <summary>
        /// Executes one AI turn: gather resources, build if affordable, attack if possible.
        /// </summary>
        public void TakeTurn(IReadOnlyList<Player> allPlayers)
        {
            RemoveDefeatedEntities();
            CollectPassiveIncome();

            GatherResources();
            TryBuild();
            TryAttack(allPlayers);
        }

        private void GatherResources()
        {
            foreach (var unit in Units)
            {
                if (!(unit is Worker worker)) continue;

                if (worker.CarriedAmount >= worker.CarryCapacity)
                {
                    var baseBuilding = Buildings.FirstOrDefault(b => b.Type == BuildingType.MainBase && b.IsAlive);
                    if (baseBuilding != null)
                    {
                        worker.MoveTo(_map, baseBuilding.X, baseBuilding.Y);
                        worker.Deposit(Resources);
                    }
                    continue;
                }

                var target = FindNearestResourceTile(worker.X, worker.Y);
                if (target != null)
                {
                    MoveToward(worker, target.X, target.Y);
                    worker.Gather(_map);
                }
            }
        }

        private void TryBuild()
        {
            if (!Buildings.Any(b => b.Type == BuildingType.Farm))
                TryBuildAt(BuildingType.Farm);

            if (Buildings.Any(b => b.Type == BuildingType.Farm && b.IsAlive)
                && !Buildings.Any(b => b.Type == BuildingType.Barracks))
                TryBuildAt(BuildingType.Barracks);
        }

        private void TryBuildAt(BuildingType type)
        {
            var mainBase = Buildings.FirstOrDefault(b => b.Type == BuildingType.MainBase && b.IsAlive);
            if (mainBase == null) return;

            for (int dx = -3; dx <= 3; dx++)
            {
                for (int dy = -3; dy <= 3; dy++)
                {
                    int tx = mainBase.X + dx;
                    int ty = mainBase.Y + dy;
                    if (!_map.IsInBounds(tx, ty)) continue;
                    if (!_map.GetTile(tx, ty).IsPassable) continue;

                    var building = Build(type, tx, ty, _map);
                    if (building != null) return;
                }
            }
        }

        private void TryAttack(IReadOnlyList<Player> allPlayers)
        {
            var enemies = allPlayers
                .Where(p => p.Id != Id && !p.IsDefeated)
                .ToList();

            if (!enemies.Any()) return;

            foreach (var unit in Units.Where(u => u.Type != UnitType.Worker && u.IsAlive))
            {
                Entity nearestTarget = null;
                int nearestDist = int.MaxValue;

                foreach (var enemy in enemies)
                {
                    foreach (var eu in enemy.Units.Where(u => u.IsAlive))
                    {
                        int d = Math.Abs(eu.X - unit.X) + Math.Abs(eu.Y - unit.Y);
                        if (d < nearestDist) { nearestDist = d; nearestTarget = eu; }
                    }
                    foreach (var eb in enemy.Buildings.Where(b => b.IsAlive))
                    {
                        int d = Math.Abs(eb.X - unit.X) + Math.Abs(eb.Y - unit.Y);
                        if (d < nearestDist) { nearestDist = d; nearestTarget = eb; }
                    }
                }

                if (nearestTarget == null) continue;

                if (nearestDist <= unit.AttackRange)
                    unit.Attack(nearestTarget);
                else
                    MoveToward(unit, nearestTarget.X, nearestTarget.Y);
            }
        }

        private MapTile FindNearestResourceTile(int x, int y)
        {
            MapTile best = null;
            int bestDist = int.MaxValue;

            foreach (var tile in _map.AllTiles())
            {
                if (tile.ResourceAmount <= 0) continue;
                int d = Math.Abs(tile.X - x) + Math.Abs(tile.Y - y);
                if (d < bestDist) { bestDist = d; best = tile; }
            }

            return best;
        }

        private void MoveToward(Unit unit, int targetX, int targetY)
        {
            int dx = Math.Sign(targetX - unit.X);
            int dy = Math.Sign(targetY - unit.Y);

            if (dx != 0 && unit.MoveTo(_map, unit.X + dx, unit.Y)) return;
            if (dy != 0) unit.MoveTo(_map, unit.X, unit.Y + dy);
        }
    }
}
