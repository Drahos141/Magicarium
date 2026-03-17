namespace Magicarium.Resources;

/// <summary>
/// Tracks the amounts of each resource type owned by a player.
/// </summary>
public class ResourceInventory
{
    private readonly Dictionary<ResourceType, int> _resources = new();

    public ResourceInventory()
    {
        foreach (ResourceType type in Enum.GetValues<ResourceType>())
            _resources[type] = 0;
    }

    public int Get(ResourceType type) => _resources[type];

    public void Add(ResourceType type, int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative.");
        _resources[type] += amount;
    }

    public bool TrySpend(ResourceType type, int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative.");
        if (_resources[type] < amount) return false;
        _resources[type] -= amount;
        return true;
    }

    public bool CanAfford(ResourceType type, int amount) => _resources[type] >= amount;

    public bool CanAfford(IReadOnlyDictionary<ResourceType, int> cost)
    {
        foreach (var (type, amount) in cost)
            if (!CanAfford(type, amount)) return false;
        return true;
    }

    public bool TrySpend(IReadOnlyDictionary<ResourceType, int> cost)
    {
        if (!CanAfford(cost)) return false;
        foreach (var (type, amount) in cost)
            _resources[type] -= amount;
        return true;
    }
}
