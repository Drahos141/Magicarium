using System.Collections.Generic;
using Magicarium.Entities;
using Magicarium.Resources;

namespace Magicarium.Buildings
{
    /// <summary>
    /// A building placed on the map. Buildings have health and can be destroyed.
    /// </summary>
    public class Building : Entity
    {
        public BuildingType Type { get; }

        /// <summary>
        /// Passive benefit description.
        /// </summary>
        public string BenefitDescription { get; }

        /// <summary>Resource cost to construct this building.</summary>
        public static IReadOnlyDictionary<ResourceType, int> GetCost(BuildingType type)
        {
            switch (type)
            {
                case BuildingType.MainBase:
                    return new Dictionary<ResourceType, int> { [ResourceType.Gold] = 0 };
                case BuildingType.Farm:
                    return new Dictionary<ResourceType, int> { [ResourceType.Gold] = 50,  [ResourceType.Wood] = 30 };
                case BuildingType.Barracks:
                    return new Dictionary<ResourceType, int> { [ResourceType.Gold] = 150, [ResourceType.Wood] = 100 };
                case BuildingType.BlacksmithHall:
                    return new Dictionary<ResourceType, int> { [ResourceType.Gold] = 200, [ResourceType.Wood] = 150, [ResourceType.MagicOre] = 50 };
                case BuildingType.Mill:
                    return new Dictionary<ResourceType, int> { [ResourceType.Gold] = 75,  [ResourceType.Wood] = 50 };
                case BuildingType.Church:
                    return new Dictionary<ResourceType, int> { [ResourceType.Gold] = 100, [ResourceType.Wood] = 80,  [ResourceType.MagicOre] = 20 };
                case BuildingType.Road:
                    return new Dictionary<ResourceType, int> { [ResourceType.Gold] = 10,  [ResourceType.Wood] = 20 };
                case BuildingType.Well:
                    return new Dictionary<ResourceType, int> { [ResourceType.Gold] = 40,  [ResourceType.Wood] = 20 };
                case BuildingType.Shrine:
                    return new Dictionary<ResourceType, int> { [ResourceType.Gold] = 80,  [ResourceType.MagicOre] = 30 };
                case BuildingType.Field:
                    return new Dictionary<ResourceType, int> { [ResourceType.Gold] = 30,  [ResourceType.Wood] = 10 };
                default:
                    return new Dictionary<ResourceType, int>();
            }
        }

        public Building(int ownerId, int x, int y, BuildingType type)
            : base(ownerId, x, y, GetMaxHealth(type))
        {
            Type = type;
            BenefitDescription = GetBenefitDescription(type);
        }

        private static int GetMaxHealth(BuildingType type)
        {
            switch (type)
            {
                case BuildingType.MainBase:       return 2000;
                case BuildingType.Barracks:       return 800;
                case BuildingType.BlacksmithHall: return 700;
                case BuildingType.Farm:           return 400;
                case BuildingType.Mill:           return 400;
                case BuildingType.Church:         return 600;
                case BuildingType.Road:           return 100;
                case BuildingType.Well:           return 300;
                case BuildingType.Shrine:         return 350;
                case BuildingType.Field:          return 200;
                default:                          return 300;
            }
        }

        private static string GetBenefitDescription(BuildingType type)
        {
            switch (type)
            {
                case BuildingType.MainBase:       return "Central hub; workers deposit resources here.";
                case BuildingType.Farm:           return "Increases food production and population cap.";
                case BuildingType.Barracks:       return "Allows training of soldiers and archers.";
                case BuildingType.BlacksmithHall: return "Enables weapon upgrades for military units.";
                case BuildingType.Mill:           return "Increases gold income from nearby farms.";
                case BuildingType.Church:         return "Boosts unit morale and healing rate.";
                case BuildingType.Road:           return "Increases movement speed of friendly units on this tile by 50%.";
                case BuildingType.Well:           return "Reduces fire damage spread; improves worker health regeneration.";
                case BuildingType.Shrine:         return "Generates passive magic ore income over time.";
                case BuildingType.Field:          return "Provides food for population growth.";
                default:                          return string.Empty;
            }
        }
    }
}
