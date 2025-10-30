using System;

namespace WordGame_Task1

{
    internal class LanguageManager
    {
        public bool IsRussian { get; private set; } = true;

        public void ChooseLanguage()
        {
            Console.WriteLine("1. Русский");
            Console.WriteLine("2. English");

            if (int.TryParse(Console.ReadLine(), out int choice))
                IsRussian = choice == 1;
        }

        public string GetText(string ru, string en)
        {
            return IsRussian ? ru : en;
        }
    }
}

