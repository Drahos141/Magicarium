using Magicarium.Buildings;
using Magicarium.Entities;
using Magicarium.Map;
using Magicarium.Resources;
using Xunit;

namespace Magicarium.Tests;

public class BuildingTests
{
    [Fact]
    public void Building_starts_at_full_health()
    {
        var b = new Building(1, 0, 0, BuildingType.MainBase);
        Assert.Equal(b.MaxHealth, b.Health);
        Assert.True(b.IsAlive);
    }

    [Fact]
    public void MainBase_has_highest_health()
    {
        var mainBase = new Building(1, 0, 0, BuildingType.MainBase);
        var barracks = new Building(1, 0, 0, BuildingType.Barracks);
        Assert.True(mainBase.MaxHealth > barracks.MaxHealth);
    }

    [Fact]
    public void Building_can_be_destroyed()
    {
        var b = new Building(1, 0, 0, BuildingType.Farm);
        bool died = b.TakeDamage(b.MaxHealth);
        Assert.True(died);
        Assert.False(b.IsAlive);
    }

    [Fact]
    public void Road_building_has_road_terrain_cost()
    {
        var cost = Building.GetCost(BuildingType.Road);
        Assert.True(cost.ContainsKey(ResourceType.Gold));
        Assert.True(cost.ContainsKey(ResourceType.Wood));
    }

    [Fact]
    public void BlacksmithHall_cost_includes_magic_ore()
    {
        var cost = Building.GetCost(BuildingType.BlacksmithHall);
        Assert.True(cost.ContainsKey(ResourceType.MagicOre));
        Assert.True(cost[ResourceType.MagicOre] > 0);
    }

    [Fact]
    public void Road_benefit_description_mentions_speed()
    {
        var road = new Building(1, 0, 0, BuildingType.Road);
        Assert.Contains("speed", road.BenefitDescription, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(BuildingType.Farm)]
    [InlineData(BuildingType.Barracks)]
    [InlineData(BuildingType.BlacksmithHall)]
    [InlineData(BuildingType.Mill)]
    [InlineData(BuildingType.Church)]
    [InlineData(BuildingType.Well)]
    [InlineData(BuildingType.Shrine)]
    [InlineData(BuildingType.Field)]
    [InlineData(BuildingType.Road)]
    public void All_building_types_have_non_zero_cost(BuildingType type)
    {
        var cost = Building.GetCost(type);
        Assert.True(cost.Values.Sum() > 0);
    }

    [Theory]
    [InlineData(BuildingType.Farm)]
    [InlineData(BuildingType.Barracks)]
    [InlineData(BuildingType.BlacksmithHall)]
    [InlineData(BuildingType.Mill)]
    [InlineData(BuildingType.Church)]
    [InlineData(BuildingType.Well)]
    [InlineData(BuildingType.Shrine)]
    [InlineData(BuildingType.Field)]
    [InlineData(BuildingType.Road)]
    public void All_building_types_have_positive_max_health(BuildingType type)
    {
        var b = new Building(1, 0, 0, type);
        Assert.True(b.MaxHealth > 0);
    }
}
