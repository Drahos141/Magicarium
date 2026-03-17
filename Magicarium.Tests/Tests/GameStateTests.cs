using Magicarium.Buildings;
using Magicarium.Entities;
using Magicarium.Game;
using Magicarium.Map;
using Magicarium.Players;
using Magicarium.Resources;
using Xunit;

namespace Magicarium.Tests;

public class CombatSystemTests
{
    [Fact]
    public void Attack_deals_damage_to_enemy()
    {
        var attacker = new Unit(1, 0, 0, UnitType.Soldier);
        var target = new Unit(2, 1, 0, UnitType.Soldier);
        int healthBefore = target.Health;

        CombatSystem.Attack(attacker, target);
        Assert.True(target.Health < healthBefore);
    }

    [Fact]
    public void Attack_does_not_damage_friendly_unit()
    {
        var attacker = new Unit(1, 0, 0, UnitType.Soldier);
        var ally = new Unit(1, 1, 0, UnitType.Soldier); // same owner
        int healthBefore = ally.Health;

        CombatSystem.Attack(attacker, ally);
        Assert.Equal(healthBefore, ally.Health);
    }

    [Fact]
    public void Attack_returns_true_when_target_dies()
    {
        var attacker = new Unit(1, 0, 0, UnitType.Knight);
        // Use worker with low health as target
        var target = new Unit(2, 1, 0, UnitType.Worker);
        // Deal enough damage to kill
        target.TakeDamage(target.MaxHealth - 1); // leave 1 hp

        bool killed = CombatSystem.Attack(attacker, target);
        Assert.True(killed);
        Assert.False(target.IsAlive);
    }

    [Fact]
    public void Attack_returns_false_when_attacker_is_dead()
    {
        var attacker = new Unit(1, 0, 0, UnitType.Soldier);
        attacker.TakeDamage(attacker.MaxHealth); // kill attacker

        var target = new Unit(2, 1, 0, UnitType.Soldier);
        int healthBefore = target.Health;

        bool result = CombatSystem.Attack(attacker, target);
        Assert.False(result);
        Assert.Equal(healthBefore, target.Health);
    }

    [Fact]
    public void Attack_can_destroy_building()
    {
        var attacker = new Unit(1, 0, 0, UnitType.Knight);
        var building = new Building(2, 1, 0, BuildingType.Field);
        // Drain health so one hit kills it
        building.TakeDamage(building.MaxHealth - attacker.AttackDamage);

        bool destroyed = CombatSystem.Attack(attacker, building);
        Assert.True(destroyed);
        Assert.False(building.IsAlive);
    }

    [Fact]
    public void IsInRange_returns_true_for_adjacent_tile()
    {
        var attacker = new Unit(1, 0, 0, UnitType.Soldier);
        var target = new Unit(2, 1, 0, UnitType.Soldier);
        Assert.True(CombatSystem.IsInRange(attacker, target));
    }

    [Fact]
    public void IsInRange_returns_false_when_too_far()
    {
        var attacker = new Unit(1, 0, 0, UnitType.Soldier);
        var target = new Unit(2, 5, 5, UnitType.Soldier);
        Assert.False(CombatSystem.IsInRange(attacker, target));
    }
}

public class GameStateTests
{
    [Fact]
    public void CreateNew_generates_map_and_state()
    {
        var state = GameState.CreateNew(20, 20, seed: 7);
        Assert.NotNull(state.Map);
        Assert.Equal(20, state.Map.Width);
        Assert.Equal(20, state.Map.Height);
    }

    [Fact]
    public void AddPlayer_allows_up_to_8_players()
    {
        var state = GameState.CreateNew(30, 30, seed: 1);
        for (int i = 1; i <= 8; i++)
            state.AddPlayer(new Player(i, $"P{i}"));
        Assert.Equal(8, state.Players.Count);
    }

    [Fact]
    public void AddPlayer_throws_when_exceeding_8_players()
    {
        var state = GameState.CreateNew(30, 30, seed: 1);
        for (int i = 1; i <= 8; i++)
            state.AddPlayer(new Player(i, $"P{i}"));
        Assert.Throws<InvalidOperationException>(() => state.AddPlayer(new Player(9, "P9")));
    }

    [Fact]
    public void Tick_increments_turn()
    {
        var state = GameState.CreateNew(20, 20, seed: 2);
        state.AddPlayer(new Player(1, "Human"));
        state.Tick();
        Assert.Equal(1, state.CurrentTurn);
    }

    [Fact]
    public void IsGameOver_is_false_when_multiple_players_alive()
    {
        var state = GameState.CreateNew(20, 20, seed: 3);
        var p1 = new Player(1, "P1");
        var p2 = new Player(2, "P2");
        p1.PlaceStartingEntities(0, 0);
        p2.PlaceStartingEntities(15, 15);
        state.AddPlayer(p1);
        state.AddPlayer(p2);
        Assert.False(state.IsGameOver);
    }

    [Fact]
    public void IsGameOver_is_true_when_only_one_player_survives()
    {
        var state = GameState.CreateNew(20, 20, seed: 4);
        var p1 = new Player(1, "P1");
        var p2 = new Player(2, "P2");
        p1.PlaceStartingEntities(0, 0);
        p2.PlaceStartingEntities(15, 15);
        state.AddPlayer(p1);
        state.AddPlayer(p2);

        // Defeat player 2 completely
        foreach (var unit in p2.Units.ToList()) unit.TakeDamage(unit.MaxHealth);
        foreach (var bldg in p2.Buildings.ToList()) bldg.TakeDamage(bldg.MaxHealth);
        p2.RemoveDefeatedEntities();

        Assert.True(state.IsGameOver);
        Assert.Equal(p1, state.GetWinner());
    }

    [Fact]
    public void AI_player_takes_turn_during_tick()
    {
        var map = MapGenerator.Generate(20, 20, seed: 10);
        // Place some gold mine near AI start
        map.GetTile(6, 5).Terrain = TerrainType.Mine;
        map.GetTile(6, 5).ResourceYield = ResourceType.Gold;
        map.GetTile(6, 5).ResourceAmount = 500;

        var state = new GameState(map);
        var ai = new AIPlayer(1, "AI", map, seed: 10);
        ai.PlaceStartingEntities(5, 5);
        state.AddPlayer(ai);

        // Should not throw; AI takes turns
        for (int i = 0; i < 5; i++)
            state.Tick();

        Assert.True(state.CurrentTurn == 5);
    }
}
