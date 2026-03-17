using System;
using Magicarium.Resources;

namespace Magicarium.Map
{
    /// <summary>
    /// Generates random game maps for Magicarium.
    /// Maps include various terrains (grass, forest, mountain, water), gold/magic-ore mines and forests.
    /// </summary>
    public static class MapGenerator
    {
        private const int GoldMineResourceAmount = 2000;
        private const int MagicOreMineResourceAmount = 1000;
        private const int ForestWoodAmount = 500;

        /// <summary>
        /// Creates a randomly generated map of the given dimensions.
        /// </summary>
        public static GameMap Generate(int width, int height, int? seed = null)
        {
            var rng = seed.HasValue ? new Random(seed.Value) : new Random();
            var map = new GameMap(width, height);

            PlaceTerrain(map, rng, width, height);
            PlaceMines(map, rng, width, height);
            PlaceForests(map, rng, width, height);

            return map;
        }

        private static void PlaceTerrain(GameMap map, Random rng, int width, int height)
        {
            int lakeCount = Math.Max(1, (width * height) / 80);
            for (int i = 0; i < lakeCount; i++)
            {
                int cx = rng.Next(1, width - 1);
                int cy = rng.Next(1, height - 1);
                int radius = rng.Next(1, 3);
                FillCircle(map, cx, cy, radius, TerrainType.Water);
            }

            int mountainCount = Math.Max(1, (width * height) / 100);
            for (int i = 0; i < mountainCount; i++)
            {
                int cx = rng.Next(1, width - 1);
                int cy = rng.Next(1, height - 1);
                int radius = rng.Next(1, 3);
                FillCircle(map, cx, cy, radius, TerrainType.Mountain);
            }
        }

        private static void PlaceMines(GameMap map, Random rng, int width, int height)
        {
            int mineCount = Math.Max(2, (width * height) / 60);

            for (int i = 0; i < mineCount; i++)
            {
                var resourceType = (i % 2 == 0) ? ResourceType.Gold : ResourceType.MagicOre;
                int resourceAmount = resourceType == ResourceType.Gold
                    ? GoldMineResourceAmount
                    : MagicOreMineResourceAmount;

                TryPlaceTile(map, rng, width, height, TerrainType.Mine, tile =>
                {
                    tile.ResourceYield = resourceType;
                    tile.ResourceAmount = resourceAmount;
                });
            }
        }

        private static void PlaceForests(GameMap map, Random rng, int width, int height)
        {
            int forestClusterCount = Math.Max(2, (width * height) / 50);

            for (int i = 0; i < forestClusterCount; i++)
            {
                int cx = rng.Next(0, width);
                int cy = rng.Next(0, height);
                int radius = rng.Next(1, 4);

                FillCircle(map, cx, cy, radius, TerrainType.Forest, tile =>
                {
                    // FillCircle has already set Terrain = Forest; only assign
                    // resources if none are present (preserves mines / earlier placements).
                    if (tile.ResourceAmount > 0) return;
                    tile.ResourceYield = ResourceType.Wood;
                    tile.ResourceAmount = ForestWoodAmount;
                });
            }
        }

        private static void TryPlaceTile(
            GameMap map, Random rng, int width, int height,
            TerrainType terrain, Action<MapTile> configure = null,
            int maxAttempts = 20)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                int x = rng.Next(0, width);
                int y = rng.Next(0, height);
                var tile = map.GetTile(x, y);

                if (tile.Terrain == TerrainType.Grass)
                {
                    tile.Terrain = terrain;
                    configure?.Invoke(tile);
                    return;
                }
            }
        }

        private static void FillCircle(
            GameMap map, int cx, int cy, int radius,
            TerrainType terrain, Action<MapTile> onSet = null)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (dx * dx + dy * dy > radius * radius) continue;
                    int nx = cx + dx;
                    int ny = cy + dy;
                    if (!map.IsInBounds(nx, ny)) continue;

                    var tile = map.GetTile(nx, ny);
                    tile.Terrain = terrain;
                    onSet?.Invoke(tile);
                }
            }
        }
    }
}
