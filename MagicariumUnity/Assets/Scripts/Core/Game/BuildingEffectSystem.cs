using Magicarium.Buildings;
using Magicarium.Map;
using Magicarium.Players;

namespace Magicarium.Game
{
    /// <summary>
    /// Applies passive building effects to the map and players each turn.
    /// </summary>
    public static class BuildingEffectSystem
    {
        public static void Apply(Player player, GameMap map)
        {
            player.CollectPassiveIncome();
            EnsureRoadTerrain(player, map);
        }

        private static void EnsureRoadTerrain(Player player, GameMap map)
        {
            foreach (var building in player.Buildings)
            {
                if (building.Type != BuildingType.Road) continue;
                if (!map.IsInBounds(building.X, building.Y)) continue;

                var tile = map.GetTile(building.X, building.Y);
                if (tile.Terrain != TerrainType.Road)
                    tile.Terrain = TerrainType.Road;
            }
        }
    }
}
