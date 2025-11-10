using System;

namespace WordGame_Task1
{
    /// <summary>
    /// Console-based implementation of IUserInterface.
    /// </summary>
    public class ConsoleUI : IUserInterface
    {
        public void Clear() => Console.Clear();
        public void Write(string text) => Console.Write(text);
        public void WriteLine(string text) => Console.WriteLine(text);
        public string ReadLine() => Console.ReadLine();
    }
}

