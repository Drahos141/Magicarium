using Magicarium.Map;
using Magicarium.Resources;
using Xunit;

namespace Magicarium.Tests;

public class GameMapTests
{
    [Fact]
    public void Map_tiles_default_to_grass()
    {
        var map = new GameMap(10, 10);
        Assert.Equal(TerrainType.Grass, map.GetTile(0, 0).Terrain);
        Assert.Equal(TerrainType.Grass, map.GetTile(9, 9).Terrain);
    }

    [Fact]
    public void IsInBounds_returns_correct_values()
    {
        var map = new GameMap(5, 5);
        Assert.True(map.IsInBounds(0, 0));
        Assert.True(map.IsInBounds(4, 4));
        Assert.False(map.IsInBounds(-1, 0));
        Assert.False(map.IsInBounds(5, 0));
        Assert.False(map.IsInBounds(0, 5));
    }

    [Fact]
    public void GetTile_throws_on_out_of_bounds()
    {
        var map = new GameMap(5, 5);
        Assert.Throws<ArgumentOutOfRangeException>(() => map.GetTile(-1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => map.GetTile(0, 5));
    }

    [Fact]
    public void AllTiles_returns_width_times_height_tiles()
    {
        var map = new GameMap(6, 4);
        Assert.Equal(24, map.AllTiles().Count());
    }

    [Fact]
    public void GetNeighbours_returns_up_to_4_neighbours()
    {
        var map = new GameMap(3, 3);
        // Corner tile has 2 neighbours
        Assert.Equal(2, map.GetNeighbours(0, 0).Count());
        // Centre tile has 4 neighbours
        Assert.Equal(4, map.GetNeighbours(1, 1).Count());
    }

    [Fact]
    public void Water_tile_is_not_passable()
    {
        var map = new GameMap(5, 5);
        map.GetTile(2, 2).Terrain = TerrainType.Water;
        Assert.False(map.GetTile(2, 2).IsPassable);
    }

    [Fact]
    public void Mountain_tile_is_not_passable()
    {
        var map = new GameMap(5, 5);
        map.GetTile(1, 1).Terrain = TerrainType.Mountain;
        Assert.False(map.GetTile(1, 1).IsPassable);
    }

    [Fact]
    public void Road_tile_has_speed_multiplier_of_1_5()
    {
        var map = new GameMap(5, 5);
        map.GetTile(2, 2).Terrain = TerrainType.Road;
        Assert.Equal(1.5f, map.GetTile(2, 2).SpeedMultiplier);
    }
}

public class MapGeneratorTests
{
    [Fact]
    public void Generate_creates_map_with_correct_dimensions()
    {
        var map = MapGenerator.Generate(20, 20, seed: 42);
        Assert.Equal(20, map.Width);
        Assert.Equal(20, map.Height);
    }

    [Fact]
    public void Generate_places_at_least_one_mine()
    {
        var map = MapGenerator.Generate(20, 20, seed: 42);
        Assert.Contains(map.AllTiles(), t => t.Terrain == TerrainType.Mine);
    }

    [Fact]
    public void Generate_places_at_least_one_forest()
    {
        var map = MapGenerator.Generate(20, 20, seed: 42);
        Assert.Contains(map.AllTiles(), t => t.Terrain == TerrainType.Forest);
    }

    [Fact]
    public void Generate_mine_tiles_have_resource_yield_and_amount()
    {
        var map = MapGenerator.Generate(30, 30, seed: 99);
        var mines = map.AllTiles().Where(t => t.Terrain == TerrainType.Mine).ToList();
        Assert.All(mines, m =>
        {
            Assert.NotNull(m.ResourceYield);
            Assert.True(m.ResourceAmount > 0);
            Assert.True(m.ResourceYield == ResourceType.Gold || m.ResourceYield == ResourceType.MagicOre);
        });
    }

    [Fact]
    public void Generate_is_deterministic_with_same_seed()
    {
        var map1 = MapGenerator.Generate(15, 15, seed: 123);
        var map2 = MapGenerator.Generate(15, 15, seed: 123);

        var tiles1 = map1.AllTiles().Select(t => t.Terrain).ToList();
        var tiles2 = map2.AllTiles().Select(t => t.Terrain).ToList();
        Assert.Equal(tiles1, tiles2);
    }
}
