using System;
using System.Collections.Generic;
using System.Linq;
using Magicarium.Buildings;
using Magicarium.Map;
using Magicarium.Players;

namespace Magicarium.Game
{
    /// <summary>
    /// Top-level game state. Holds the map and up to 8 players.
    /// </summary>
    public class GameState
    {
        public const int MaxPlayers = 8;

        public GameMap Map { get; }
        public IReadOnlyList<Player> Players => _players;
        public int CurrentTurn { get; private set; }
        public bool IsGameOver => _players.Count > 1 && _players.Count(p => !p.IsDefeated) <= 1;

        private readonly List<Player> _players = new List<Player>();

        public GameState(GameMap map)
        {
            Map = map ?? throw new ArgumentNullException("map");
        }

        public static GameState CreateNew(int mapWidth = 64, int mapHeight = 64, int? seed = null)
        {
            var map = MapGenerator.Generate(mapWidth, mapHeight, seed);
            return new GameState(map);
        }

        // ── Player management ───────────────────────────────────────

        public void AddPlayer(Player player)
        {
            if (_players.Count >= MaxPlayers)
                throw new InvalidOperationException($"Cannot add more than {MaxPlayers} players.");
            if (_players.Any(p => p.Id == player.Id))
                throw new InvalidOperationException($"A player with id {player.Id} already exists.");

            _players.Add(player);
        }

        // ── Turn processing ─────────────────────────────────────────

        /// <summary>
        /// Advances one game tick: applies building effects and lets AI players act.
        /// Human-player actions are driven externally via unit/building APIs.
        /// </summary>
        public void Tick()
        {
            if (IsGameOver) return;

            CurrentTurn++;

            foreach (var player in _players)
            {
                if (player.IsDefeated) continue;

                BuildingEffectSystem.Apply(player, Map);

                if (player is AIPlayer ai)
                    ai.TakeTurn(_players);

                player.RemoveDefeatedEntities();
            }
        }

        /// <summary>Returns the player who has not been defeated, or null if nobody has won yet.</summary>
        public Player GetWinner() =>
            IsGameOver ? _players.FirstOrDefault(p => !p.IsDefeated) : null;
    }
}
