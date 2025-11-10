using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Timer = System.Timers.Timer;

namespace WordGame_Task1
{
    /// <summary>
    /// Represents the main gameplay process: selecting the base word,
    /// validating player input, handling turns, and detecting loss conditions.
    /// </summary>
    internal class Game
    {
        private readonly LanguageManager _languageManager;
        private readonly WordValidator _validator;
        private readonly IUserInterface _ui;

        public Game(LanguageManager languageManager, IUserInterface ui)
        {
            _languageManager = languageManager;
            _validator = new WordValidator(languageManager.IsRussian);
            _ui = ui;
        }

        /// <summary>
        /// Starts the gameplay loop between two players.
        /// </summary>
        public void Start()
        {
            _ui.Clear();
            _ui.WriteLine(_languageManager.GetText(
                "Введите начальное слово (от 8 до 30 букв):",
                "Enter the base word (8–30 letters):"));

            string baseWord = _ui.ReadLine();

            // Ensures base word is valid
            while (!_validator.IsOnlyLetters(baseWord) || baseWord.Length < 8 || baseWord.Length > 30)
            {
                _ui.WriteLine(_languageManager.GetText(
                    "Ошибка: слово должно содержать только русские буквы (8–30).",
                    "Error: only English letters (8–30)."));
                baseWord = _ui.ReadLine();
            }

            List<string> usedWords = new List<string>();
            int currentPlayer = 1;

            while (true)
            {
                _ui.WriteLine("");
                _ui.WriteLine(_languageManager.GetText(
                    $"Игрок {currentPlayer}, введите слово за 10 секунд:",
                    $"Player {currentPlayer}, enter a word within 10 seconds:"));

                // Timer-based user input
                string word = ReadWordWithTimer(10);

                if (word == null)
                {
                    _ui.WriteLine(_languageManager.GetText(
                        $"Игрок {currentPlayer} проиграл! Время вышло.",
                        $"Player {currentPlayer} lost! Time is up."));
                    break;
                }

                if (!_validator.IsOnlyLetters(word))
                {
                    _ui.WriteLine(_languageManager.GetText(
                        $"Игрок {currentPlayer} проиграл! Использованы неверные символы.",
                        $"Player {currentPlayer} lost! Invalid alphabet."));
                    break;
                }

                if (usedWords.Contains(word))
                {
                    _ui.WriteLine(_languageManager.GetText(
                        $"Слово уже использовалось! Игрок {currentPlayer} проиграл!",
                        $"Word already used! Player {currentPlayer} lost!"));
                    break;
                }

                if (!_validator.CanBeMadeFrom(baseWord, word))
                {
                    _ui.WriteLine(_languageManager.GetText(
                        $"Нельзя составить из букв исходного слова. Игрок {currentPlayer} проиграл!",
                        $"Cannot be made from base word. Player {currentPlayer} lost!"));
                    break;
                }

                usedWords.Add(word);
                _ui.WriteLine(_languageManager.GetText($"Слово принято: {word}", $"Word accepted: {word}"));

                currentPlayer = currentPlayer == 1 ? 2 : 1;
            }

            _ui.WriteLine(_languageManager.GetText(
                "Нажмите Enter, чтобы начать новую игру...",
                "Press Enter to start a new game..."));
            _ui.ReadLine();
            _ui.Clear();
        }

        /// <summary>
        /// Reads user input while displaying a countdown timer.
        /// Returns null if time runs out.
        /// </summary>
        private string ReadWordWithTimer(int seconds)
        {
            int secondsLeft = seconds;
            StringBuilder buffer = new StringBuilder();
            bool timeExpired = false;
            object lockObj = new object();

            Console.WriteLine(_languageManager.GetText(
                $"Осталось секунд: {secondsLeft}",
                $"Time left: {secondsLeft}"));
            int timerTop = Console.CursorTop - 1;
            Console.Write("> ");

            Timer timer = new Timer(1000);
            timer.Elapsed += (s, e) =>
            {
                lock (lockObj)
                {
                    if (timeExpired) return;
                    secondsLeft--;

                    if (secondsLeft < 0)
                    {
                        timeExpired = true;
                        timer.Stop();
                        return;
                    }

                    try
                    {
                        Console.SetCursorPosition(0, timerTop);
                        Console.Write(new string(' ', Console.WindowWidth - 1));
                        Console.SetCursorPosition(0, timerTop);
                        Console.Write(_languageManager.GetText(
                            $"Осталось секунд: {secondsLeft}",
                            $"Time left: {secondsLeft}"));
                        Console.SetCursorPosition(2 + buffer.Length, timerTop + 1);
                    }
                    catch { }
                }
            };
            timer.Start();

            while (!timeExpired)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.Enter)
                        break;

                    if (key.Key == ConsoleKey.Backspace && buffer.Length > 0)
                    {
                        buffer.Length--;
                        Console.Write("\b \b");
                    }
                    else if (char.IsLetter(key.KeyChar))
                    {
                        buffer.Append(key.KeyChar);
                        Console.Write(key.KeyChar);
                    }
                }

                Thread.Sleep(50);
            }

            timer.Stop();
            Console.WriteLine();

            if (timeExpired && buffer.Length == 0)
                return null;

            return buffer.ToString().ToLower();
        }
    }
}



