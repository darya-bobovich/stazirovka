using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordGame_Task1;

namespace WordGame_Task1
{
    /// <summary>
    /// Represents a player with their statistics.
    /// </summary>
    [Serializable]
    public class Player : IEquatable<Player>
    {
        /// <summary>
        /// Gets or sets the player's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets total games played.
        /// </summary>
        public int TotalGames { get; set; }

        /// <summary>
        /// Gets or sets total wins.
        /// </summary>
        public int Wins { get; set; }

        /// <summary>
        /// Initializes a new instance of the Player class.
        /// </summary>
        public Player() { }

        /// <summary>
        /// Initializes a new instance of the Player class with a name.
        /// </summary>
        public Player(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Adds a game result to player's statistics.
        /// </summary>
        public void AddGame(bool isWin)
        {
            TotalGames++;
            if (isWin) Wins++;
        }

        /// <summary>
        /// Determines whether the current player equals another player by name.
        /// </summary>
        public bool Equals(Player other)
        {
            if (other is null) return false;
            return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether the current object equals another object.
        /// </summary>
        public override bool Equals(object obj) => Equals(obj as Player);

        /// <summary>
        /// Returns the hash code for this player.
        /// </summary>
        public override int GetHashCode() => Name.ToLower().GetHashCode();

        /// <summary>
        /// Returns string representation of the player (in Russian).
        /// </summary>
        public override string ToString()
        {
            return $"{Name}: Игр сыграно: {TotalGames}, Побед: {Wins}";
        }

        /// <summary>
        /// Returns string representation in specified language.
        /// </summary>
        public string ToString(bool isRussian)
        {
            if (isRussian)
                return $"{Name}: Игр сыграно: {TotalGames}, Побед: {Wins}";
            else
                return $"{Name}: Games played: {TotalGames}, Wins: {Wins}";
        }
    }
}