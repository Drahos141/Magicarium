using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Magicarium.Buildings;
using Magicarium.Entities;
using Magicarium.Game;
using Magicarium.Players;
using Magicarium.Resources;

namespace Magicarium.Unity
{
    /// <summary>
    /// Central singleton MonoBehaviour that owns the game state, drives the
    /// input state-machine, and coordinates the UI and map renderer.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // ── Public game-state accessors ──────────────────────────────
        public GameState GameState { get; private set; }
        public Player HumanPlayer { get; private set; }

        // ── Input state machine ──────────────────────────────────────
        public enum ActionMode { None, Move, Attack, Build }
        public ActionMode CurrentMode { get; private set; } = ActionMode.None;
        public Entity SelectedEntity { get; private set; }
        public BuildingType PendingBuildType { get; private set; }

        // ── Map size ─────────────────────────────────────────────────
        public const int MapSize = 20;

        private UIManager _ui;

        // ────────────────────────────────────────────────────────────
        // Unity lifecycle
        // ────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            InitialiseGame();

            // UIManager creates all Canvas elements; it must be added after game state exists
            var uiGo = new GameObject("UIManager");
            _ui = uiGo.AddComponent<UIManager>();
            _ui.Initialise();
        }

        // ────────────────────────────────────────────────────────────
        // Game setup
        // ────────────────────────────────────────────────────────────

        private void InitialiseGame()
        {
            GameState = GameState.CreateNew(MapSize, MapSize, seed: 42);

            // Human player — start with enough resources to build and train
            HumanPlayer = new Player(1, "Player");
            HumanPlayer.Resources.Add(ResourceType.Gold, 300);
            HumanPlayer.Resources.Add(ResourceType.Wood, 200);
            HumanPlayer.Resources.Add(ResourceType.MagicOre, 50);
            HumanPlayer.PlaceStartingEntities(2, 2);
            GameState.AddPlayer(HumanPlayer);

            // AI player
            var ai = new AIPlayer(2, "AI Enemy", GameState.Map, seed: 77);
            ai.Resources.Add(ResourceType.Gold, 200);
            ai.Resources.Add(ResourceType.Wood, 150);
            ai.PlaceStartingEntities(MapSize - 5, MapSize - 5);
            GameState.AddPlayer(ai);
        }

        // ────────────────────────────────────────────────────────────
        // Tile click — dispatched by TileClickHandler
        // ────────────────────────────────────────────────────────────

        public void OnTileClicked(int x, int y)
        {
            switch (CurrentMode)
            {
                case ActionMode.None:   SelectAt(x, y);      break;
                case ActionMode.Move:   TryMove(x, y);       break;
                case ActionMode.Attack: TryAttack(x, y);     break;
                case ActionMode.Build:  TryBuild(x, y);      break;
            }
        }

        // ── Selection ────────────────────────────────────────────────

        private void SelectAt(int x, int y)
        {
            Entity found = null;

            // Prefer the human player's own entities
            found = HumanPlayer.Units.FirstOrDefault(u => u.X == x && u.Y == y && u.IsAlive);
            if (found == null)
                found = HumanPlayer.Buildings.FirstOrDefault(b => b.X == x && b.Y == y && b.IsAlive);

            // Fall back to enemy entities (read-only; allows viewing info)
            if (found == null)
            {
                foreach (var p in GameState.Players.Where(p => p.Id != HumanPlayer.Id))
                {
                    found = p.Units.FirstOrDefault(u => u.X == x && u.Y == y && u.IsAlive);
                    if (found != null) break;
                    found = p.Buildings.FirstOrDefault(b => b.X == x && b.Y == y && b.IsAlive);
                    if (found != null) break;
                }
            }

            SelectedEntity = found;
            CurrentMode = ActionMode.None;
            Refresh();
        }

        // ── Move ─────────────────────────────────────────────────────

        private void TryMove(int x, int y)
        {
            if (SelectedEntity is Unit unit && unit.OwnerId == HumanPlayer.Id)
            {
                bool ok = unit.MoveTo(GameState.Map, x, y);
                _ui.ShowMessage(ok
                    ? $"{unit.Type} moved to ({x},{y})."
                    : "Cannot move there — tile is impassable or out of bounds.");
            }
            CurrentMode = ActionMode.None;
            Refresh();
        }

        // ── Attack ───────────────────────────────────────────────────

        private void TryAttack(int x, int y)
        {
            if (!(SelectedEntity is Unit attacker) || attacker.OwnerId != HumanPlayer.Id)
            {
                CurrentMode = ActionMode.None;
                Refresh();
                return;
            }

            Entity target = null;
            foreach (var player in GameState.Players.Where(p => p.Id != HumanPlayer.Id))
            {
                target = player.Units.FirstOrDefault(u => u.X == x && u.Y == y && u.IsAlive);
                if (target != null) break;
                target = player.Buildings.FirstOrDefault(b => b.X == x && b.Y == y && b.IsAlive);
                if (target != null) break;
            }

            if (target == null)
            {
                _ui.ShowMessage("No enemy at that tile.");
            }
            else
            {
                bool killed = CombatSystem.Attack(attacker, target);
                if (!CombatSystem.IsInRange(attacker, target))
                    _ui.ShowMessage($"Target is out of range (range = {attacker.AttackRange} tiles).");
                else
                    _ui.ShowMessage(killed
                        ? $"Enemy {GetEntityLabel(target)} destroyed!"
                        : $"Hit enemy {GetEntityLabel(target)} for {attacker.AttackDamage} damage.");
            }

            CurrentMode = ActionMode.None;
            CleanDeadEntities();
            Refresh();
        }

        // ── Build ────────────────────────────────────────────────────

        private void TryBuild(int x, int y)
        {
            var building = HumanPlayer.Build(PendingBuildType, x, y, GameState.Map);
            _ui.ShowMessage(building != null
                ? $"Built {PendingBuildType} at ({x},{y})."
                : "Cannot build there — check resources or tile passability.");
            CurrentMode = ActionMode.None;
            Refresh();
        }

        // ────────────────────────────────────────────────────────────
        // Action methods called from UI buttons
        // ────────────────────────────────────────────────────────────

        public void StartMove()
        {
            if (SelectedEntity is Unit u && u.OwnerId == HumanPlayer.Id)
            {
                CurrentMode = ActionMode.Move;
                _ui.ShowMessage($"Click a tile to move {u.Type}.");
            }
        }

        public void StartAttack()
        {
            if (SelectedEntity is Unit u && u.OwnerId == HumanPlayer.Id)
            {
                CurrentMode = ActionMode.Attack;
                _ui.ShowMessage($"Click an enemy tile to attack.");
            }
        }

        public void StartBuild(BuildingType type)
        {
            var cost = Building.GetCost(type);
            if (!HumanPlayer.Resources.CanAfford(cost))
            {
                _ui.ShowMessage($"Not enough resources to build {type}.");
                return;
            }
            PendingBuildType = type;
            CurrentMode = ActionMode.Build;
            _ui.ShowMessage($"Click a tile to place {type}.");
        }

        public void TrainUnit(UnitType type)
        {
            BuildingType required = type == UnitType.Worker
                ? BuildingType.MainBase
                : BuildingType.Barracks;

            var spawnBuilding = HumanPlayer.Buildings.FirstOrDefault(b => b.Type == required && b.IsAlive);
            if (spawnBuilding == null)
            {
                _ui.ShowMessage($"Need a {required} to train {type}.");
                return;
            }

            var cost = GetTrainingCost(type);
            if (!HumanPlayer.Resources.TrySpend(cost))
            {
                _ui.ShowMessage($"Not enough resources to train {type}.");
                return;
            }

            // Place new unit adjacent to the spawn building
            int nx = spawnBuilding.X + 1 < MapSize ? spawnBuilding.X + 1 : spawnBuilding.X - 1;
            Unit newUnit = type == UnitType.Worker
                ? new Worker(HumanPlayer.Id, nx, spawnBuilding.Y)
                : new Unit(HumanPlayer.Id, nx, spawnBuilding.Y, type);

            HumanPlayer.AddUnit(newUnit);
            _ui.ShowMessage($"{type} trained!");
            Refresh();
        }

        public void WorkerGatherOrDeposit()
        {
            if (!(SelectedEntity is Worker worker) || worker.OwnerId != HumanPlayer.Id) return;

            // If carrying resources and standing on the base, deposit
            if (worker.CarriedAmount > 0)
            {
                var baseBuilding = HumanPlayer.Buildings.FirstOrDefault(b => b.Type == BuildingType.MainBase && b.IsAlive);
                if (baseBuilding != null && worker.X == baseBuilding.X && worker.Y == baseBuilding.Y)
                {
                    var result = worker.Deposit(HumanPlayer.Resources);
                    if (result.HasValue)
                        _ui.ShowMessage($"Deposited {result.Value.Amount} {result.Value.Type}.");
                    Refresh();
                    return;
                }
            }

            // Otherwise gather from current tile
            int gathered = worker.Gather(GameState.Map);
            if (gathered > 0)
                _ui.ShowMessage($"Gathered {gathered} {worker.CarriedResource}. Carrying: {worker.CarriedAmount}/{worker.CarryCapacity}.");
            else if (worker.CarriedAmount > 0)
                _ui.ShowMessage($"Carrying {worker.CarriedAmount} {worker.CarriedResource}. Move to your HQ to deposit.");
            else
                _ui.ShowMessage("No resources on this tile to gather.");

            Refresh();
        }

        public void EndTurn()
        {
            SelectedEntity = null;
            CurrentMode = ActionMode.None;

            GameState.Tick();
            CleanDeadEntities();

            if (GameState.IsGameOver)
            {
                var winner = GameState.GetWinner();
                bool humanWon = winner != null && winner.Id == HumanPlayer.Id;
                _ui.ShowGameOver(humanWon, winner?.Name ?? "Nobody");
            }
            else
            {
                _ui.ShowMessage($"Turn {GameState.CurrentTurn} — your move.");
            }

            Refresh();
        }

        // ────────────────────────────────────────────────────────────
        // Helpers
        // ────────────────────────────────────────────────────────────

        private void CleanDeadEntities()
        {
            foreach (var p in GameState.Players)
                p.RemoveDefeatedEntities();
        }

        private void Refresh()
        {
            _ui?.Refresh();
        }

        private static string GetEntityLabel(Entity e)
        {
            if (e is Unit u)  return u.Type.ToString();
            if (e is Building b) return b.Type.ToString();
            return "entity";
        }

        public static IReadOnlyDictionary<ResourceType, int> GetTrainingCost(UnitType type)
        {
            switch (type)
            {
                case UnitType.Worker:  return new Dictionary<ResourceType, int> { [ResourceType.Gold] = 50 };
                case UnitType.Soldier: return new Dictionary<ResourceType, int> { [ResourceType.Gold] = 100 };
                case UnitType.Archer:  return new Dictionary<ResourceType, int> { [ResourceType.Gold] = 80,  [ResourceType.MagicOre] = 20 };
                case UnitType.Knight:  return new Dictionary<ResourceType, int> { [ResourceType.Gold] = 200, [ResourceType.MagicOre] = 50 };
                default:               return new Dictionary<ResourceType, int>();
            }
        }
    }
}
