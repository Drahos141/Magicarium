using Magicarium.Entities;
using Magicarium.Map;
using Magicarium.Resources;
using Xunit;

namespace Magicarium.Tests;

public class UnitTests
{
    private static GameMap CreateFlatMap(int w = 10, int h = 10) => new GameMap(w, h);

    [Fact]
    public void Unit_starts_at_full_health()
    {
        var unit = new Unit(1, 0, 0, UnitType.Soldier);
        Assert.Equal(unit.MaxHealth, unit.Health);
        Assert.True(unit.IsAlive);
    }

    [Fact]
    public void TakeDamage_reduces_health()
    {
        var unit = new Unit(1, 0, 0, UnitType.Soldier);
        unit.TakeDamage(30);
        Assert.Equal(unit.MaxHealth - 30, unit.Health);
    }

    [Fact]
    public void TakeDamage_kills_unit_when_damage_equals_max_health()
    {
        var unit = new Unit(1, 0, 0, UnitType.Soldier);
        bool died = unit.TakeDamage(unit.MaxHealth);
        Assert.True(died);
        Assert.False(unit.IsAlive);
        Assert.Equal(0, unit.Health);
    }

    [Fact]
    public void Health_does_not_go_below_zero()
    {
        var unit = new Unit(1, 0, 0, UnitType.Worker);
        unit.TakeDamage(9999);
        Assert.Equal(0, unit.Health);
    }

    [Fact]
    public void MoveTo_moves_unit_to_passable_tile()
    {
        var map = CreateFlatMap();
        var unit = new Unit(1, 0, 0, UnitType.Soldier);
        bool moved = unit.MoveTo(map, 3, 4);
        Assert.True(moved);
        Assert.Equal(3, unit.X);
        Assert.Equal(4, unit.Y);
    }

    [Fact]
    public void MoveTo_fails_on_water_tile()
    {
        var map = CreateFlatMap();
        map.GetTile(2, 2).Terrain = TerrainType.Water;
        var unit = new Unit(1, 0, 0, UnitType.Soldier);
        bool moved = unit.MoveTo(map, 2, 2);
        Assert.False(moved);
        Assert.Equal(0, unit.X);
        Assert.Equal(0, unit.Y);
    }

    [Fact]
    public void MoveTo_fails_on_out_of_bounds()
    {
        var map = CreateFlatMap(5, 5);
        var unit = new Unit(1, 0, 0, UnitType.Soldier);
        bool moved = unit.MoveTo(map, 10, 10);
        Assert.False(moved);
    }

    [Fact]
    public void Attack_deals_damage_to_target_in_range()
    {
        var attacker = new Unit(1, 0, 0, UnitType.Soldier);
        var target = new Unit(2, 1, 0, UnitType.Soldier);
        bool died = attacker.Attack(target);
        Assert.False(died); // soldier won't die in one hit
        Assert.True(target.Health < target.MaxHealth);
    }

    [Fact]
    public void Attack_returns_false_when_target_out_of_range()
    {
        var attacker = new Unit(1, 0, 0, UnitType.Soldier);
        var target = new Unit(2, 5, 5, UnitType.Soldier);
        int healthBefore = target.Health;
        bool hit = attacker.Attack(target);
        Assert.False(hit);
        Assert.Equal(healthBefore, target.Health);
    }

    [Fact]
    public void Archer_has_attack_range_of_3()
    {
        var archer = new Unit(1, 0, 0, UnitType.Archer);
        Assert.Equal(3, archer.AttackRange);
    }

    [Fact]
    public void Worker_on_road_tile_has_higher_effective_speed()
    {
        var map = CreateFlatMap();
        map.GetTile(2, 2).Terrain = TerrainType.Road;
        var worker = new Unit(1, 2, 2, UnitType.Worker);
        float effectiveSpeed = worker.GetEffectiveSpeed(map);
        Assert.True(effectiveSpeed > worker.BaseSpeed);
    }
}

public class WorkerTests
{
    [Fact]
    public void Worker_can_gather_from_mine_tile()
    {
        var map = new GameMap(5, 5);
        var tile = map.GetTile(1, 1);
        tile.Terrain = TerrainType.Mine;
        tile.ResourceYield = ResourceType.Gold;
        tile.ResourceAmount = 100;

        var worker = new Worker(1, 1, 1);
        int gathered = worker.Gather(map);
        Assert.True(gathered > 0);
        Assert.Equal(gathered, worker.CarriedAmount);
        Assert.Equal(ResourceType.Gold, worker.CarriedResource);
    }

    [Fact]
    public void Worker_can_deposit_resources()
    {
        var map = new GameMap(5, 5);
        var tile = map.GetTile(0, 0);
        tile.Terrain = TerrainType.Mine;
        tile.ResourceYield = ResourceType.Gold;
        tile.ResourceAmount = 100;

        var worker = new Worker(1, 0, 0);
        worker.Gather(map);

        var inventory = new ResourceInventory();
        var result = worker.Deposit(inventory);

        Assert.NotNull(result);
        Assert.Equal(ResourceType.Gold, result.Value.type);
        Assert.True(result.Value.amount > 0);
        Assert.Equal(0, worker.CarriedAmount);
        Assert.Equal(result.Value.amount, inventory.Get(ResourceType.Gold));
    }

    [Fact]
    public void Worker_does_not_gather_when_carry_is_full()
    {
        var map = new GameMap(5, 5);
        var tile = map.GetTile(0, 0);
        tile.Terrain = TerrainType.Mine;
        tile.ResourceYield = ResourceType.Gold;
        tile.ResourceAmount = 1000;

        var worker = new Worker(1, 0, 0);
        // Fill carry
        while (worker.CarriedAmount < worker.CarryCapacity)
            worker.Gather(map);

        int tileAmountBefore = tile.ResourceAmount;
        int gatheredExtra = worker.Gather(map);
        Assert.Equal(0, gatheredExtra);
        Assert.Equal(tileAmountBefore, tile.ResourceAmount);
    }

    [Fact]
    public void Worker_can_gather_wood_from_forest_tile()
    {
        var map = new GameMap(5, 5);
        var tile = map.GetTile(2, 2);
        tile.Terrain = TerrainType.Forest;
        tile.ResourceYield = ResourceType.Wood;
        tile.ResourceAmount = 200;

        var worker = new Worker(1, 2, 2);
        int gathered = worker.Gather(map);
        Assert.True(gathered > 0);
        Assert.Equal(ResourceType.Wood, worker.CarriedResource);
    }

    [Fact]
    public void Worker_does_not_gather_from_empty_tile()
    {
        var map = new GameMap(5, 5);
        var worker = new Worker(1, 0, 0); // tile is plain grass, no resources
        int gathered = worker.Gather(map);
        Assert.Equal(0, gathered);
    }
}
