using WordGame_Task1;

namespace zadaacha1
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    internal class Program
    {
        static void Main()
        {
            IUserInterface ui = new ConsoleUI();
            LanguageManager languageManager = new LanguageManager();
            int choice;

            do
            {
                Menu.Show(languageManager, ui);
                choice = Menu.GetChoice(ui);

                switch (choice)
                {
                    case 1:
                        Game game = new Game(languageManager, ui);
                        game.Start();
                        break;

                    case 2:
                        languageManager.ChooseLanguage(ui);
                        break;

                    case 3:
                        ui.WriteLine(languageManager.IsRussian ? "Выход" : "Exit");
                        break;

                    default:
                        ui.WriteLine(languageManager.IsRussian ? "Ошибка ввода!" : "Input error!");
                        break;
                }

            } while (choice != 3);
        }
    }
}


