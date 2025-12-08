using System;
using System.Text;
using System.Threading;
using Timer = System.Timers.Timer;

namespace WordGame_Task1
{
    internal class Game
    {
        private readonly LanguageManager _languageManager;
        private readonly WordValidator _validator;
        private readonly IUserInterface _ui;
        private readonly ScoreManager _scoreManager;
        private GameSession _currentSession;
        private string _currentPlayerName;
        private string _player1Name;
        private string _player2Name;
        private bool _gameActive = false;
        private bool _gameSavedOnExit = false;
        private object _saveLock = new object();

        /// <summary>
        /// Initializes a new instance of the Game class.
        /// </summary>
        public Game(LanguageManager languageManager, IUserInterface ui, ScoreManager scoreManager)
        {
            _languageManager = languageManager;
            _validator = new WordValidator(languageManager.IsRussian);
            _ui = ui;
            _scoreManager = scoreManager;
        }

        /// <summary>
        /// Starts the main game loop.
        /// </summary>
        public void Start()
        {
            try
            {
                _ui.Clear();

                _ui.WriteLine(_languageManager.GetText(
                    "Введите имя первого игрока:",
                    "Enter first player name:"));
                _player1Name = ReadInput();
                _scoreManager.GetOrCreatePlayer(_player1Name);

                _ui.WriteLine(_languageManager.GetText(
                    "Введите имя второго игрока:",
                    "Enter second player name:"));
                _player2Name = ReadInput();
                _scoreManager.GetOrCreatePlayer(_player2Name);

                _ui.WriteLine(_languageManager.GetText(
                    "Введите начальное слово (от 8 до 30 букв):",
                    "Enter the base word (8–30 letters):"));
                string baseWord = ReadInput();

                while (!_validator.IsOnlyLetters(baseWord) || baseWord.Length < 8 || baseWord.Length > 30)
                {
                    _ui.WriteLine(_languageManager.GetText(
                        "Ошибка: слово должно содержать только буквы (8–30).",
                        "Error: only letters (8–30)."));
                    baseWord = ReadInput();
                }

                // Create a session but DO NOT write to file until the game ends
                _currentSession = new GameSession(baseWord, _player1Name, _player2Name);
                _gameActive = true;
                _gameSavedOnExit = false;
                _currentPlayerName = _player1Name;
                int currentPlayerNumber = 1;

                _ui.WriteLine(_languageManager.GetText(
                    $"Игра началась! Базовое слово: {baseWord}",
                    $"Game started! Base word: {baseWord}"));
                _ui.WriteLine(_languageManager.GetText(
                    "Доступные команды: /show-words, /score, /total-score",
                    "Available commands: /show-words, /score, /total-score"));

                while (_gameActive)
                {
                    _ui.WriteLine("");
                    _ui.WriteLine(_languageManager.GetText(
                        $"{_currentPlayerName} (Игрок {currentPlayerNumber}), введите слово за 10 секунд:",
                        $"{_currentPlayerName} (Player {currentPlayerNumber}), enter a word within 10 seconds:"));

                    string input = ReadWordWithTimer(10);

                    if (input == null)
                    {
                        EndGame(_currentPlayerName, _languageManager.GetText(
                            "Время вышло!",
                            "Time is up!"));
                        break;
                    }

                    if (input.StartsWith("/"))
                    {
                        ProcessCommand(input);
                        continue;
                    }

                    if (!_validator.IsOnlyLetters(input))
                    {
                        EndGame(_currentPlayerName, _languageManager.GetText(
                            "Использованы неверные символы!",
                            "Invalid alphabet!"));
                        break;
                    }

                    if (_currentSession.UsedWords.Contains(input))
                    {
                        EndGame(_currentPlayerName, _languageManager.GetText(
                            "Слово уже использовалось!",
                            "Word already used!"));
                        break;
                    }

                    if (!_validator.CanBeMadeFrom(baseWord, input))
                    {
                        EndGame(_currentPlayerName, _languageManager.GetText(
                            "Нельзя составить из букв исходного слова!",
                            "Cannot be made from base word!"));
                        break;
                    }

                    _currentSession.UsedWords.Add(input);
                    _ui.WriteLine(_languageManager.GetText(
                        $"Слово принято: {input}",
                        $"Word accepted: {input}"));

                    _currentPlayerName = (_currentPlayerName == _player1Name) ? _player2Name : _player1Name;
                    currentPlayerNumber = (currentPlayerNumber == 1) ? 2 : 1;
                }

                // After the game ends, wait for Enter press
                if (!_gameActive)
                {
                    _ui.WriteLine(_languageManager.GetText(
                        "\nНажмите Enter, чтобы вернуться в меню...",
                        "\nPress Enter to return to menu..."));
                    _ui.ReadLine();
                }
            }
            catch (Exception ex)
            {
                _ui.WriteLine(_languageManager.GetText(
                    $"Произошла ошибка: {ex.Message}",
                    $"An error occurred: {ex.Message}"));
                _ui.WriteLine(_languageManager.GetText(
                    "Нажмите Enter, чтобы вернуться в меню...",
                    "Press Enter to return to menu..."));
                _ui.ReadLine();
            }
            finally
            {
                _gameActive = false;
                _ui.Clear();
            }
        }

        /// <summary>
        /// Ends the game and declares a winner.
        /// </summary>
        private void EndGame(string loserName, string reason)
        {
            _gameActive = false;

            string winnerName = (loserName == _player1Name) ? _player2Name : _player1Name;

            _currentSession.WinnerName = winnerName;
            _currentSession.Reason = reason;

            _ui.WriteLine("");
            _ui.WriteLine(_languageManager.GetText(
                "═".PadRight(50, '═'),
                "═".PadRight(50, '═')));

            _ui.WriteLine(_languageManager.GetText(
                $"ИГРА ОКОНЧЕНА!",
                $"GAME OVER!"));

            _ui.WriteLine(_languageManager.GetText(
                $"Победитель: {winnerName}",
                $"Winner: {winnerName}"));

            _ui.WriteLine(_languageManager.GetText(
                $"Проиграл: {loserName}",
                $"Loser: {loserName}"));

            _ui.WriteLine(_languageManager.GetText(
                $"Причина проигрыша: {reason}",
                $"Reason for loss: {reason}"));

            _ui.WriteLine(_languageManager.GetText(
                "═".PadRight(50, '═'),
                "═".PadRight(50, '═')));

            // Only now record the game result
            _scoreManager.RecordGameResult(_currentSession);
            _gameSavedOnExit = true;

            // Show current players' statistics
            ProcessCommand("/score");
        }

        /// <summary>
        /// Handles application exit by saving unfinished game state.
        /// </summary>
        public void OnApplicationExit()
        {
            lock (_saveLock)
            {
                if (_gameActive && _currentSession != null && !string.IsNullOrEmpty(_currentPlayerName) && !_gameSavedOnExit)
                {
                    try
                    {
                        string loserName = _currentPlayerName;
                        string winnerName = (loserName == _player1Name) ? _player2Name : _player1Name;

                        _currentSession.WinnerName = winnerName;
                        _currentSession.Reason = _languageManager.GetText(
                            "Приложение было закрыто во время хода игрока " + loserName,
                            "Application was closed during " + loserName + "'s turn");

                        _ui.WriteLine("");
                        _ui.WriteLine(_languageManager.GetText(
                            "═".PadRight(50, '═'),
                            "═".PadRight(50, '═')));
                        _ui.WriteLine(_languageManager.GetText(
                            "СОХРАНЕНИЕ ПРЕРВАННОЙ ИГРЫ",
                            "SAVING INTERRUPTED GAME"));
                        _ui.WriteLine(_languageManager.GetText(
                            $"Победитель: {winnerName}",
                            $"Winner: {winnerName}"));
                        _ui.WriteLine(_languageManager.GetText(
                            $"Проиграл: {loserName}",
                            $"Loser: {loserName}"));
                        _ui.WriteLine(_languageManager.GetText(
                            $"Причина: Приложение закрыто",
                            $"Reason: Application closed"));
                        _ui.WriteLine(_languageManager.GetText(
                            "═".PadRight(50, '═'),
                            "═".PadRight(50, '═')));

                        // Force save the game result
                        _scoreManager.RecordGameResult(_currentSession);

                        // Additional save for reliability
                        _scoreManager.SaveScores();

                        _gameActive = false;
                        _gameSavedOnExit = true;
                    }
                    catch (Exception ex)
                    {
                        _ui.WriteLine($"Ошибка при сохранении результатов: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Processes game commands entered by players.
        /// </summary>
        private void ProcessCommand(string command)
        {
            try
            {
                switch (command.ToLower())
                {
                    case "/show-words":
                        ShowUsedWords();
                        break;

                    case "/score":
                        ShowCurrentPlayersStats();
                        break;

                    case "/total-score":
                        ShowAllPlayersStats();
                        break;

                    default:
                        _ui.WriteLine(_languageManager.GetText(
                            $"Неизвестная команда: {command}",
                            $"Unknown command: {command}"));
                        break;
                }
            }
            catch (Exception ex)
            {
                _ui.WriteLine(_languageManager.GetText(
                    $"Ошибка выполнения команды: {ex.Message}",
                    $"Command execution error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Displays all words used in the current game session.
        /// </summary>
        private void ShowUsedWords()
        {
            if (_currentSession.UsedWords.Count == 0)
            {
                _ui.WriteLine(_languageManager.GetText(
                    "Пока не введено ни одного слова.",
                    "No words entered yet."));
                return;
            }

            _ui.WriteLine(_languageManager.GetText(
                "Использованные слова:",
                "Used words:"));

            for (int i = 0; i < _currentSession.UsedWords.Count; i++)
            {
                _ui.WriteLine($"  {i + 1}. {_currentSession.UsedWords[i]}");
            }
        }

        /// <summary>
        /// Shows statistics for the two current players.
        /// </summary>
        private void ShowCurrentPlayersStats()
        {
            try
            {
                var (p1Stats, p2Stats) = _scoreManager.GetStatsForCurrentPlayers(_player1Name, _player2Name);

                if (_languageManager.IsRussian)
                {
                    _ui.WriteLine("Статистика текущих игроков:");
                    _ui.WriteLine($"  {p1Stats.ToString(true)}");
                    _ui.WriteLine($"  {p2Stats.ToString(true)}");
                }
                else
                {
                    _ui.WriteLine("Current players statistics:");
                    _ui.WriteLine($"  {p1Stats.ToString(false)}");
                    _ui.WriteLine($"  {p2Stats.ToString(false)}");
                }
            }
            catch (Exception ex)
            {
                _ui.WriteLine(_languageManager.GetText(
                    $"Ошибка загрузки статистики: {ex.Message}",
                    $"Error loading statistics: {ex.Message}"));
            }
        }

        /// <summary>
        /// Shows statistics for all players in the system.
        /// </summary>
        private void ShowAllPlayersStats()
        {
            try
            {
                var allPlayers = _scoreManager.GetTotalStats();

                if (allPlayers.Count == 0)
                {
                    _ui.WriteLine(_languageManager.GetText(
                        "Нет данных об игроках.",
                        "No player data available."));
                    return;
                }

                if (_languageManager.IsRussian)
                {
                    _ui.WriteLine("Общая статистика всех игроков:");
                    foreach (var player in allPlayers)
                    {
                        _ui.WriteLine($"  {player.ToString(true)}");
                    }
                }
                else
                {
                    _ui.WriteLine("Total statistics of all players:");
                    foreach (var player in allPlayers)
                    {
                        _ui.WriteLine($"  {player.ToString(false)}");
                    }
                }
            }
            catch (Exception ex)
            {
                _ui.WriteLine(_languageManager.GetText(
                    $"Ошибка загрузки общей статистики: {ex.Message}",
                    $"Error loading total statistics: {ex.Message}"));
            }
        }

        /// <summary>
        /// Reads a word input with a time limit.
        /// </summary>
        private string ReadWordWithTimer(int seconds)
        {
            // For ConsoleUI, use console-specific implementation
            if (_ui is ConsoleUI consoleUi)
            {
                return ReadWordWithTimerConsole(seconds, consoleUi);
            }
            else
            {
                // Fallback for other UI implementations
                return ReadWordWithTimerSimple(seconds);
            }
        }

        /// <summary>
        /// Console-specific implementation with real-time timer display.
        /// </summary>
        private string ReadWordWithTimerConsole(int seconds, ConsoleUI consoleUi)
        {
            // Используем оригинальную реализацию с Console
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
                    else if (char.IsLetter(key.KeyChar) || key.KeyChar == '/' || key.KeyChar == '-')
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

        /// <summary>
        /// Simple implementation for non-console UI.
        /// </summary>
       
        private string ReadWordWithTimerSimple(int seconds)
        {
            int secondsLeft = seconds;
            bool timeExpired = false;
            /// <summary>
            /// Start timer
            /// </summary>
            Timer timer = new Timer(1000);
            timer.Elapsed += (s, e) =>
            {
                secondsLeft--;
                if (secondsLeft <= 0)
                {
                    timeExpired = true;
                    timer.Stop();
                }
                else
                {   /// <summary>
                    /// Update timer display
                    /// </summary>
                    _ui.Write($"\r{_languageManager.GetText("Осталось секунд: ", "Time left: ")}{secondsLeft} ");
                }
            };
            timer.Start();
            /// <summary>
            /// Show initial timer
            ///  </summary>
            _ui.Write($"{_languageManager.GetText("Осталось секунд: ", "Time left: ")}{secondsLeft} ");

            string input = null;
            DateTime startTime = DateTime.Now;

            try
            {   
                while (!timeExpired && (DateTime.Now - startTime).TotalSeconds < seconds)
                {
                
                    Thread.Sleep(100);
                }
                if (timeExpired)
                    return null;

                input = _ui.ReadLine();
            }
            finally
            {
                timer.Stop();
                _ui.WriteLine(""); 
            }

            return input?.Trim().ToLower();
        }

        /// <summary>
        /// Reads user input with validation for non-empty strings.
        /// </summary>
        private string ReadInput()
        {
            while (true)
            {
                string input = _ui.ReadLine()?.Trim();

                if (!string.IsNullOrEmpty(input))
                {
                    return input;
                }

            }
        }
    }
}