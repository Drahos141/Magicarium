using Magicarium.Resources;

namespace Magicarium.Map
{
    /// <summary>
    /// Represents a single tile on the game map.
    /// </summary>
    public class MapTile
    {
        public int X { get; }
        public int Y { get; }
        public TerrainType Terrain { get; set; }

        /// <summary>Amount of resources available on this tile (for Mine/Forest tiles).</summary>
        public int ResourceAmount { get; set; }

        /// <summary>The type of resource this tile yields (only relevant for Mine and Forest tiles).</summary>
        public ResourceType? ResourceYield { get; set; }

        /// <summary>Whether a unit or building can be placed/move through this tile.</summary>
        public bool IsPassable => Terrain != TerrainType.Water && Terrain != TerrainType.Mountain;

        /// <summary>
        /// Speed multiplier applied to units moving through or from this tile.
        /// Roads provide a 50% movement bonus; water/mountain are impassable.
        /// </summary>
        public float SpeedMultiplier => Terrain == TerrainType.Road ? 1.5f : 1.0f;

        public MapTile(int x, int y, TerrainType terrain = TerrainType.Grass)
        {
            X = x;
            Y = y;
            Terrain = terrain;
        }
    }
}
