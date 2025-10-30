using System;

namespace WordGame_Task1

{
    internal static class Menu
    {
        public static void Show(LanguageManager lang)
        {
            Console.Clear();

            if (lang.IsRussian)
            {
                Console.WriteLine("Игра в Слова");
                Console.WriteLine("1. Начать игру");
                Console.WriteLine("2. Выбор языка");
                Console.WriteLine("3. Выход");
                Console.Write("Ваш выбор: ");
            }
            else
            {
                Console.WriteLine("The Word Game");
                Console.WriteLine("1. Start Game");
                Console.WriteLine("2. Choose Language");
                Console.WriteLine("3. Exit");
                Console.Write("Your choice: ");
            }
        }

        public static int GetChoice(LanguageManager lang)
        {
            if (!int.TryParse(Console.ReadLine(), out int choice))
                return -1;
            return choice;
        }
    }
}
