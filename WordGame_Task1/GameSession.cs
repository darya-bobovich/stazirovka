using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WordGame_Task1
{
    [Serializable]
    public class GameSession
    {
        public string BaseWord { get; set; }
        public string Player1Name { get; set; }
        public string Player2Name { get; set; }
        public string WinnerName { get; set; }
        public List<string> UsedWords { get; set; } = new List<string>();
        public string Reason { get; set; }

        public GameSession() { }

        public GameSession(string baseWord, string player1Name, string player2Name)
        {
            BaseWord = baseWord;
            Player1Name = player1Name;
            Player2Name = player2Name;
        }
    }
}