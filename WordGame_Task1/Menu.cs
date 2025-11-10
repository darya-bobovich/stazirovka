namespace WordGame_Task1
{
    /// <summary>
    /// Displays the main menu.
    /// </summary>
    internal static class Menu
    {
        /// <summary>
        /// Shows localized menu.
        /// </summary>
        public static void Show(LanguageManager languageManager, IUserInterface ui)
        {
            ui.Clear();

            if (languageManager.IsRussian)
            {
                ui.WriteLine("Игра в Слова");
                ui.WriteLine("1. Начать игру");
                ui.WriteLine("2. Выбор языка");
                ui.WriteLine("3. Выход");
                ui.Write("Ваш выбор: ");
            }
            else
            {
                ui.WriteLine("The Word Game");
                ui.WriteLine("1. Start Game");
                ui.WriteLine("2. Choose Language");
                ui.WriteLine("3. Exit");
                ui.Write("Your choice: ");
            }
        }

        /// <summary>
        /// Reads menu selection.
        /// </summary>
        public static int GetChoice(IUserInterface ui)
        {
            if (!int.TryParse(ui.ReadLine(), out int choice))
                return -1;

            return choice;
        }
    }
}


