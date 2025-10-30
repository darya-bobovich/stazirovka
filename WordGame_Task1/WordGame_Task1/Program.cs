using System;
using WordGame_Task1;

namespace zadaacha1
{
    internal class Program
    {
        static void Main()
        {
            LanguageManager languageManager = new LanguageManager();
            int choice = 0;

            do
            {
                Menu.Show(languageManager);
                choice = Menu.GetChoice(languageManager);

                switch (choice)
                {
                    case 1:
                        Game game = new Game(languageManager);
                        game.Start();
                        break;
                    case 2:
                        languageManager.ChooseLanguage();
                        break;
                    case 3:
                        Console.WriteLine(languageManager.IsRussian ? "Выход" : "Exit");
                        break;
                    default:
                        Console.WriteLine(languageManager.IsRussian ? "Ошибка ввода!" : "Input error!");
                        break;
                }

            } while (choice != 3);
        }
    }
}
