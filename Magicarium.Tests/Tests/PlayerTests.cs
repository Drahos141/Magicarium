using Magicarium.Buildings;
using Magicarium.Entities;
using Magicarium.Map;
using Magicarium.Players;
using Magicarium.Resources;
using Xunit;

namespace Magicarium.Tests;

public class PlayerTests
{
    private static GameMap CreateFlatMap() => new GameMap(20, 20);

    [Fact]
    public void PlaceStartingEntities_creates_one_main_base_and_three_workers()
    {
        var player = new Player(1, "Player 1");
        player.PlaceStartingEntities(5, 5);

        Assert.Single(player.Buildings, b => b.Type == BuildingType.MainBase);
        Assert.Equal(3, player.Units.Count);
        Assert.All(player.Units, u => Assert.IsType<Worker>(u));
    }

    [Fact]
    public void Player_is_not_defeated_when_has_alive_main_base()
    {
        var player = new Player(1, "Player 1");
        player.PlaceStartingEntities(5, 5);
        Assert.False(player.IsDefeated);
    }

    [Fact]
    public void Player_is_defeated_when_main_base_destroyed_and_no_units()
    {
        var player = new Player(1, "Player 1");
        player.PlaceStartingEntities(5, 5);

        // Destroy the main base
        var mainBase = player.Buildings.First(b => b.Type == BuildingType.MainBase);
        mainBase.TakeDamage(mainBase.MaxHealth);

        // Kill all workers
        foreach (var unit in player.Units)
            unit.TakeDamage(unit.MaxHealth);

        player.RemoveDefeatedEntities();
        Assert.True(player.IsDefeated);
    }

    [Fact]
    public void Build_deducts_resources_on_success()
    {
        var map = CreateFlatMap();
        var player = new Player(1, "Player 1");
        player.Resources.Add(ResourceType.Gold, 100);
        player.Resources.Add(ResourceType.Wood, 50);

        var building = player.Build(BuildingType.Farm, 3, 3, map);
        Assert.NotNull(building);
        Assert.True(player.Resources.Get(ResourceType.Gold) < 100);
    }

    [Fact]
    public void Build_returns_null_when_resources_insufficient()
    {
        var map = CreateFlatMap();
        var player = new Player(1, "Player 1"); // no resources

        var building = player.Build(BuildingType.Barracks, 3, 3, map);
        Assert.Null(building);
    }

    [Fact]
    public void Build_road_sets_tile_terrain_to_road()
    {
        var map = CreateFlatMap();
        var player = new Player(1, "Player 1");
        player.Resources.Add(ResourceType.Gold, 50);
        player.Resources.Add(ResourceType.Wood, 50);

        player.Build(BuildingType.Road, 4, 4, map);
        Assert.Equal(TerrainType.Road, map.GetTile(4, 4).Terrain);
    }

    [Fact]
    public void Build_fails_on_water_tile()
    {
        var map = CreateFlatMap();
        map.GetTile(5, 5).Terrain = TerrainType.Water;
        var player = new Player(1, "Player 1");
        player.Resources.Add(ResourceType.Gold, 100);
        player.Resources.Add(ResourceType.Wood, 100);

        var building = player.Build(BuildingType.Farm, 5, 5, map);
        Assert.Null(building);
    }

    [Fact]
    public void CollectPassiveIncome_adds_magic_ore_from_shrines()
    {
        var map = CreateFlatMap();
        var player = new Player(1, "Player 1");
        player.Resources.Add(ResourceType.Gold, 200);
        player.Resources.Add(ResourceType.MagicOre, 100);
        player.Build(BuildingType.Shrine, 3, 3, map);

        int magicOreBefore = player.Resources.Get(ResourceType.MagicOre);
        player.CollectPassiveIncome();
        Assert.True(player.Resources.Get(ResourceType.MagicOre) > magicOreBefore);
    }

    [Fact]
    public void CollectPassiveIncome_adds_gold_from_mill_and_farm_combo()
    {
        var map = CreateFlatMap();
        var player = new Player(1, "Player 1");
        player.Resources.Add(ResourceType.Gold, 500);
        player.Resources.Add(ResourceType.Wood, 200);
        player.Build(BuildingType.Farm, 2, 2, map);
        player.Build(BuildingType.Mill, 3, 3, map);

        int goldBefore = player.Resources.Get(ResourceType.Gold);
        player.CollectPassiveIncome();
        Assert.True(player.Resources.Get(ResourceType.Gold) > goldBefore);
    }

    [Fact]
    public void RemoveDefeatedEntities_removes_dead_units_and_buildings()
    {
        var player = new Player(1, "Player 1");
        player.PlaceStartingEntities(0, 0);

        // Kill all workers
        foreach (var unit in player.Units.ToList())
            unit.TakeDamage(unit.MaxHealth);

        // Destroy main base
        var mb = player.Buildings.First(b => b.Type == BuildingType.MainBase);
        mb.TakeDamage(mb.MaxHealth);

        player.RemoveDefeatedEntities();

        Assert.Empty(player.Units);
        Assert.Empty(player.Buildings);
    }
}
