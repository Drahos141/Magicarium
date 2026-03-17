using Magicarium.Resources;
using Xunit;

namespace Magicarium.Tests;

public class ResourceInventoryTests
{
    [Fact]
    public void New_inventory_starts_at_zero_for_all_types()
    {
        var inv = new ResourceInventory();
        foreach (ResourceType type in Enum.GetValues<ResourceType>())
            Assert.Equal(0, inv.Get(type));
    }

    [Fact]
    public void Add_increases_resource_amount()
    {
        var inv = new ResourceInventory();
        inv.Add(ResourceType.Gold, 100);
        Assert.Equal(100, inv.Get(ResourceType.Gold));
    }

    [Fact]
    public void TrySpend_returns_true_and_deducts_when_sufficient()
    {
        var inv = new ResourceInventory();
        inv.Add(ResourceType.Wood, 50);
        bool result = inv.TrySpend(ResourceType.Wood, 30);
        Assert.True(result);
        Assert.Equal(20, inv.Get(ResourceType.Wood));
    }

    [Fact]
    public void TrySpend_returns_false_when_insufficient()
    {
        var inv = new ResourceInventory();
        inv.Add(ResourceType.Gold, 10);
        bool result = inv.TrySpend(ResourceType.Gold, 20);
        Assert.False(result);
        Assert.Equal(10, inv.Get(ResourceType.Gold)); // unchanged
    }

    [Fact]
    public void CanAfford_dictionary_returns_true_when_all_resources_sufficient()
    {
        var inv = new ResourceInventory();
        inv.Add(ResourceType.Gold, 200);
        inv.Add(ResourceType.Wood, 100);
        var cost = new Dictionary<ResourceType, int>
        {
            [ResourceType.Gold] = 150,
            [ResourceType.Wood] = 80
        };
        Assert.True(inv.CanAfford(cost));
    }

    [Fact]
    public void CanAfford_dictionary_returns_false_when_any_resource_insufficient()
    {
        var inv = new ResourceInventory();
        inv.Add(ResourceType.Gold, 200);
        inv.Add(ResourceType.Wood, 50);
        var cost = new Dictionary<ResourceType, int>
        {
            [ResourceType.Gold] = 150,
            [ResourceType.Wood] = 80
        };
        Assert.False(inv.CanAfford(cost));
    }

    [Fact]
    public void TrySpend_dictionary_deducts_all_on_success()
    {
        var inv = new ResourceInventory();
        inv.Add(ResourceType.Gold, 200);
        inv.Add(ResourceType.Wood, 100);
        var cost = new Dictionary<ResourceType, int>
        {
            [ResourceType.Gold] = 150,
            [ResourceType.Wood] = 80
        };
        bool result = inv.TrySpend(cost);
        Assert.True(result);
        Assert.Equal(50, inv.Get(ResourceType.Gold));
        Assert.Equal(20, inv.Get(ResourceType.Wood));
    }

    [Fact]
    public void Add_throws_on_negative_amount()
    {
        var inv = new ResourceInventory();
        Assert.Throws<ArgumentOutOfRangeException>(() => inv.Add(ResourceType.Gold, -1));
    }
}
