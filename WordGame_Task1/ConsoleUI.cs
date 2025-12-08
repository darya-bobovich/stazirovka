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

        /// <summary>
        /// Checks if a key is available in the input buffer.
        /// </summary>
        public bool KeyAvailable => Console.KeyAvailable;

        /// <summary>
        /// Reads a key without displaying it.
        /// </summary>
        public ConsoleKeyInfo ReadKey(bool intercept) => Console.ReadKey(intercept);

        public void SetCursorPosition(int left, int top) => Console.SetCursorPosition(left, top);

        public (int Left, int Top) GetCursorPosition() => (Console.CursorLeft, Console.CursorTop);

        public int WindowWidth => Console.WindowWidth;
    }
}