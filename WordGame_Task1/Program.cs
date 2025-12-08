using System;
using WordGame_Task1;
using System.Runtime.InteropServices;

namespace zadaacha1
{
    /// <summary>
    /// Main program class for the Word Game application.
    /// </summary>
    internal class Program
    {
        private static Game _currentGame;
        private static ScoreManager _scoreManager;

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        private static EventHandler _handler;

        /// <summary>
        /// Types of control events for console handling.
        /// </summary>
        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        /// <summary>
        /// Handles console control events (like Ctrl+C or window close).
        /// </summary>
        private static bool ConsoleCtrlCheck(CtrlType ctrlType)
        {
            Console.WriteLine($"\nClosing event detected: {ctrlType}");
            SaveUnfinishedGame();
            System.Threading.Thread.Sleep(1000);
            return false;
        }

        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        static void Main()
        {
            try
            {
                Console.Title = "Word Game";
                _handler += new EventHandler(ConsoleCtrlCheck);
                SetConsoleCtrlHandler(_handler, true);

                IUserInterface ui = new ConsoleUI();
                LanguageManager languageManager = new LanguageManager();
                _scoreManager = new ScoreManager();
                int choice;

                do
                {
                    Menu.Show(languageManager, ui);
                    choice = Menu.GetChoice(ui);

                    switch (choice)
                    {
                        case 1:
                            _currentGame = new Game(languageManager, ui, _scoreManager);
                            _currentGame.Start();
                            _currentGame = null;
                            break;
                        case 2:
                            languageManager.ChooseLanguage(ui);
                            break;
                        case 3:
                            ui.WriteLine(languageManager.GetText("Выход", "Exit"));
                            break;
                        default:
                            ui.WriteLine(languageManager.GetText("Ошибка ввода!", "Input error!"));
                            break;
                    }
                } while (choice != 3);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error: {ex.Message}");
                SaveUnfinishedGame();
                Console.WriteLine("Press Enter to exit...");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Saves the unfinished game if one is in progress.
        /// </summary>
        private static void SaveUnfinishedGame()
        {
            try
            {
                if (_currentGame != null)
                {
                    Console.WriteLine("Saving unfinished game...");
                    _currentGame.OnApplicationExit();
                    Console.WriteLine("Game saved.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving game: {ex.Message}");
            }
        }
    }
}