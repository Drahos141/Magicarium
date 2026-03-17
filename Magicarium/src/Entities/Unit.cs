using Magicarium.Map;

namespace Magicarium.Entities;

/// <summary>
/// A movable, attackable game unit.
/// </summary>
public class Unit : Entity
{
    public UnitType Type { get; }

    /// <summary>Base movement speed in tiles per turn.</summary>
    public float BaseSpeed { get; }

    /// <summary>Attack damage dealt per strike.</summary>
    public int AttackDamage { get; }

    /// <summary>Melee attack range in tiles (1 = adjacent only).</summary>
    public int AttackRange { get; }

    public Unit(int ownerId, int x, int y, UnitType type)
        : base(ownerId, x, y, GetMaxHealth(type))
    {
        Type = type;
        BaseSpeed = GetBaseSpeed(type);
        AttackDamage = GetAttackDamage(type);
        AttackRange = GetAttackRange(type);
    }

    /// <summary>
    /// Moves the unit to the specified tile.
    /// Returns false if the tile is not passable.
    /// </summary>
    public bool MoveTo(GameMap map, int targetX, int targetY)
    {
        if (!map.IsInBounds(targetX, targetY)) return false;
        var tile = map.GetTile(targetX, targetY);
        if (!tile.IsPassable) return false;

        X = targetX;
        Y = targetY;
        return true;
    }

    /// <summary>
    /// Returns the effective speed of this unit considering road bonuses on the current tile.
    /// </summary>
    public float GetEffectiveSpeed(GameMap map)
    {
        if (!map.IsInBounds(X, Y)) return BaseSpeed;
        var tile = map.GetTile(X, Y);
        return BaseSpeed * tile.SpeedMultiplier;
    }

    /// <summary>
    /// Attacks a target entity. Returns true if the target died.
    /// </summary>
    public bool Attack(Entity target)
    {
        int dx = Math.Abs(target.X - X);
        int dy = Math.Abs(target.Y - Y);
        int distance = dx + dy; // Manhattan distance

        if (distance > AttackRange) return false;

        return target.TakeDamage(AttackDamage);
    }

    // ──────────────────────────────────────────────────────────────
    // Static stat tables
    // ──────────────────────────────────────────────────────────────

    private static int GetMaxHealth(UnitType type) => type switch
    {
        UnitType.Worker  => 60,
        UnitType.Soldier => 100,
        UnitType.Archer  => 80,
        UnitType.Knight  => 150,
        _ => 80
    };

    private static float GetBaseSpeed(UnitType type) => type switch
    {
        UnitType.Worker  => 2.0f,
        UnitType.Soldier => 2.0f,
        UnitType.Archer  => 2.5f,
        UnitType.Knight  => 1.5f,
        _ => 2.0f
    };

    private static int GetAttackDamage(UnitType type) => type switch
    {
        UnitType.Worker  => 5,
        UnitType.Soldier => 15,
        UnitType.Archer  => 12,
        UnitType.Knight  => 25,
        _ => 10
    };

    private static int GetAttackRange(UnitType type) => type switch
    {
        UnitType.Archer  => 3,
        _ => 1
    };
}
