using Magicarium.Entities;
using Magicarium.Map;
using Magicarium.Resources;

namespace Magicarium.Buildings;

/// <summary>
/// A building placed on the map.  Buildings have health and can be destroyed.
/// </summary>
public class Building : Entity
{
    public BuildingType Type { get; }

    /// <summary>
    /// Passive benefit description (informational; logic applied by <see cref="Magicarium.Game.BuildingEffectSystem"/>).
    /// </summary>
    public string BenefitDescription { get; }

    /// <summary>Resource cost to construct this building.</summary>
    public static IReadOnlyDictionary<ResourceType, int> GetCost(BuildingType type) =>
        type switch
        {
            BuildingType.MainBase       => new Dictionary<ResourceType, int> { [ResourceType.Gold] = 0 },
            BuildingType.Farm           => new Dictionary<ResourceType, int> { [ResourceType.Gold] = 50,  [ResourceType.Wood] = 30 },
            BuildingType.Barracks       => new Dictionary<ResourceType, int> { [ResourceType.Gold] = 150, [ResourceType.Wood] = 100 },
            BuildingType.BlacksmithHall => new Dictionary<ResourceType, int> { [ResourceType.Gold] = 200, [ResourceType.Wood] = 150, [ResourceType.MagicOre] = 50 },
            BuildingType.Mill           => new Dictionary<ResourceType, int> { [ResourceType.Gold] = 75,  [ResourceType.Wood] = 50 },
            BuildingType.Church         => new Dictionary<ResourceType, int> { [ResourceType.Gold] = 100, [ResourceType.Wood] = 80,  [ResourceType.MagicOre] = 20 },
            BuildingType.Road           => new Dictionary<ResourceType, int> { [ResourceType.Gold] = 10,  [ResourceType.Wood] = 20 },
            BuildingType.Well           => new Dictionary<ResourceType, int> { [ResourceType.Gold] = 40,  [ResourceType.Wood] = 20 },
            BuildingType.Shrine         => new Dictionary<ResourceType, int> { [ResourceType.Gold] = 80,  [ResourceType.MagicOre] = 30 },
            BuildingType.Field          => new Dictionary<ResourceType, int> { [ResourceType.Gold] = 30,  [ResourceType.Wood] = 10 },
            _ => new Dictionary<ResourceType, int>()
        };

    public Building(int ownerId, int x, int y, BuildingType type)
        : base(ownerId, x, y, GetMaxHealth(type))
    {
        Type = type;
        BenefitDescription = GetBenefitDescription(type);
    }

    // ──────────────────────────────────────────────────────────────
    // Static stat tables
    // ──────────────────────────────────────────────────────────────

    private static int GetMaxHealth(BuildingType type) => type switch
    {
        BuildingType.MainBase       => 2000,
        BuildingType.Barracks       => 800,
        BuildingType.BlacksmithHall => 700,
        BuildingType.Farm           => 400,
        BuildingType.Mill           => 400,
        BuildingType.Church         => 600,
        BuildingType.Road           => 100,
        BuildingType.Well           => 300,
        BuildingType.Shrine         => 350,
        BuildingType.Field          => 200,
        _ => 300
    };

    private static string GetBenefitDescription(BuildingType type) => type switch
    {
        BuildingType.MainBase       => "Central hub; workers deposit resources here.",
        BuildingType.Farm           => "Increases food production and population cap.",
        BuildingType.Barracks       => "Allows training of soldiers and archers.",
        BuildingType.BlacksmithHall => "Enables weapon upgrades for military units.",
        BuildingType.Mill           => "Increases gold income from nearby farms.",
        BuildingType.Church         => "Boosts unit morale and healing rate.",
        BuildingType.Road           => "Increases movement speed of friendly units on this tile by 50%.",
        BuildingType.Well           => "Reduces fire damage spread; improves worker health regeneration.",
        BuildingType.Shrine         => "Generates passive magic ore income over time.",
        BuildingType.Field          => "Provides food for population growth.",
        _ => string.Empty
    };
}
