

namespace WordGame_Task1
{
    /// <summary>
    /// Controls current application language and provides localized text.
    /// </summary>
    internal class LanguageManager
    {
        public bool IsRussian { get; private set; } = true;

        /// <summary>
        /// Allows the user to switch between Russian and English.
        /// </summary>
        public void ChooseLanguage(IUserInterface ui)
        {
            ui.WriteLine("1. Русский");
            ui.WriteLine("2. English");

            if (int.TryParse(ui.ReadLine(), out int choice))
                IsRussian = choice == 1;
        }

        /// <summary>
        /// Returns text in selected language.
        /// </summary>
        public string GetText(string ru, string en)
        {
            return IsRussian ? ru : en;
        }
    }
}



