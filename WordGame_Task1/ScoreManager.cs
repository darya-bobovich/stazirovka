using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

namespace WordGame_Task1
{
    /// <summary>
    /// Contains game data including players and game history.
    /// </summary>
    [Serializable]
    public class GameData
    {
        /// <summary>
        /// Gets or sets the list of players.
        /// </summary>
        public List<Player> Players { get; set; } = new List<Player>();

        /// <summary>
        /// Gets or sets the game session history.
        /// </summary>
        public List<GameSession> History { get; set; } = new List<GameSession>();

        /// <summary>
        /// Gets or sets the last update timestamp.
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Represents player statistics for display.
    /// </summary>
    public class PlayerStats
    {

        public string Name { get; set; }

        public int GamesPlayed { get; set; }

        public int Wins { get; set; }

        /// <summary>
        /// Returns a string representation of player statistics in the specified language.
        /// </summary>
        public string ToString(bool isRussian)
        {
            if (isRussian)
                return $"{Name}: Игр сыграно: {GamesPlayed}, Побед: {Wins}";
            else
                return $"{Name}: Games played: {GamesPlayed}, Wins: {Wins}";
        }
    }

    /// <summary>
    /// Manages player scores, game sessions, and persistence.
    /// </summary>
    public class ScoreManager
    {
        private readonly string _filePath = "scores.json";
        private GameData _gameData = new GameData();
        private readonly object _saveLock = new object();

        /// <summary>
        /// Initializes a new ScoreManager and loads existing data.
        /// </summary>
        public ScoreManager()
        {
            LoadScores();
        }

        /// <summary>
        /// Gets an existing player or creates a new one.
        /// </summary>
        public Player GetOrCreatePlayer(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Player name cannot be empty");

            var player = _gameData.Players.FirstOrDefault(p =>
                p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (player == null)
            {
                player = new Player(name);
                _gameData.Players.Add(player);
                SaveScores();
            }

            return player;
        }

        /// <summary>
        /// Gets all players sorted by wins, then games, then name.
        /// </summary>
        public List<Player> GetAllPlayers()
        {
            return _gameData.Players.OrderByDescending(p => p.Wins)
                          .ThenByDescending(p => p.TotalGames)
                          .ThenBy(p => p.Name)
                          .ToList();
        }

        /// <summary>
        /// Records a game result in the history and updates player stats.
        /// </summary>
        public void RecordGameResult(GameSession session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            var existingUnfinishedSession = _gameData.History.FirstOrDefault(s =>
                s.BaseWord == session.BaseWord &&
                s.Player1Name == session.Player1Name &&
                s.Player2Name == session.Player2Name &&
                string.IsNullOrEmpty(s.WinnerName));

            if (existingUnfinishedSession != null)
                _gameData.History.Remove(existingUnfinishedSession);

            _gameData.History.Add(session);

            if (!string.IsNullOrWhiteSpace(session.WinnerName))
            {
                string loserName = (session.WinnerName == session.Player1Name)
                    ? session.Player2Name
                    : session.Player1Name;

                var winner = GetOrCreatePlayer(session.WinnerName);
                winner.AddGame(true);

                var loser = GetOrCreatePlayer(loserName);
                loser.AddGame(false);
            }

            _gameData.LastUpdated = DateTime.Now;
            SaveScores();
        }

        /// <summary>
        /// Gets statistics for two current players.
        /// </summary>
        public (PlayerStats p1Stats, PlayerStats p2Stats) GetStatsForCurrentPlayers(string player1Name, string player2Name)
        {
            if (string.IsNullOrWhiteSpace(player1Name) || string.IsNullOrWhiteSpace(player2Name))
                return (new PlayerStats(), new PlayerStats());

            LoadScores();

            var gamesBetweenPlayers = _gameData.History.Where(g =>
                !string.IsNullOrWhiteSpace(g.WinnerName) &&
                ((g.Player1Name.Equals(player1Name, StringComparison.OrdinalIgnoreCase) &&
                  g.Player2Name.Equals(player2Name, StringComparison.OrdinalIgnoreCase)) ||
                 (g.Player1Name.Equals(player2Name, StringComparison.OrdinalIgnoreCase) &&
                  g.Player2Name.Equals(player1Name, StringComparison.OrdinalIgnoreCase))))
                .ToList();

            int gamesCount = gamesBetweenPlayers.Count;
            int player1Wins = gamesBetweenPlayers.Count(g =>
                g.WinnerName?.Equals(player1Name, StringComparison.OrdinalIgnoreCase) == true);
            int player2Wins = gamesBetweenPlayers.Count(g =>
                g.WinnerName?.Equals(player2Name, StringComparison.OrdinalIgnoreCase) == true);

            var p1Stats = new PlayerStats
            {
                Name = player1Name,
                GamesPlayed = gamesCount,
                Wins = player1Wins
            };

            var p2Stats = new PlayerStats
            {
                Name = player2Name,
                GamesPlayed = gamesCount,
                Wins = player2Wins
            };

            return (p1Stats, p2Stats);
        }

        /// <summary>
        /// Gets total statistics for all players.
        /// </summary>
        public List<PlayerStats> GetTotalStats()
        {
            LoadScores();

            return _gameData.Players.Select(p => new PlayerStats
            {
                Name = p.Name,
                GamesPlayed = p.TotalGames,
                Wins = p.Wins
            })
            .OrderByDescending(p => p.Wins)
            .ThenByDescending(p => p.GamesPlayed)
            .ThenBy(p => p.Name)
            .ToList();
        }

        /// <summary>
        /// Saves scores to JSON file with backup mechanism.
        /// </summary>
        public void SaveScores()
        {
            try
            {
                lock (_saveLock)
                {
                    string json = JsonConvert.SerializeObject(_gameData, Formatting.Indented);
                    string tempFile = _filePath + ".tmp";
                    File.WriteAllText(tempFile, json);

                    if (File.Exists(_filePath))
                        File.Delete(_filePath);

                    File.Move(tempFile, _filePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving results: {ex.Message}");
                CreateBackup();
            }
        }

        /// <summary>
        /// Creates a backup file in case of save error.
        /// </summary>
        private void CreateBackup()
        {
            try
            {
                string backupFile = "scores_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
                string json = JsonConvert.SerializeObject(_gameData, Formatting.Indented);
                File.WriteAllText(backupFile, json);
                System.Diagnostics.Debug.WriteLine($"Backup created: {backupFile}");
            }
            catch (Exception backupEx)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to create backup: {backupEx.Message}");
                throw new IOException("Failed to save game data and create backup", backupEx);
            }
        }

        /// <summary>
        /// Loads scores from JSON file.
        /// </summary>
        public void LoadScores()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _gameData = new GameData();
                    return;
                }

                string json = File.ReadAllText(_filePath);
                _gameData = JsonConvert.DeserializeObject<GameData>(json) ?? new GameData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading results: {ex.Message}");
                _gameData = new GameData();
                throw;
            }
        }

        /// <summary>
        /// Gets the complete game history.
        /// </summary>
        public List<GameSession> GetGameHistory()
        {
            LoadScores();
            return _gameData.History;
        }
    }
}