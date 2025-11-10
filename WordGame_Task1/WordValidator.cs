using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WordGame_Task1
{
    /// <summary>
    /// Provides word validation rules. Checks allowed alphabet
    /// and ensures that a word can be constructed from a base word.
    /// </summary>
    internal class WordValidator
    {
        private readonly bool _isRussian;

        public WordValidator(bool isRussian)
        {
            _isRussian = isRussian;
        }

        /// <summary>
        /// Checks whether the word contains only valid letters
        /// of the selected language.
        /// </summary>
        public bool IsOnlyLetters(string word)
        {
            return _isRussian
                ? Regex.IsMatch(word, @"^[а-яА-ЯёЁ]+$")
                : Regex.IsMatch(word, @"^[a-zA-Z]+$");
        }

        /// <summary>
        /// Determines if the given word can be constructed from
        /// the letters of the base word.
        /// </summary>
        public bool CanBeMadeFrom(string baseWord, string word)
        {
            var baseLetters = CountLetters(baseWord);
            var wordLetters = CountLetters(word);

            foreach (var pair in wordLetters)
            {
                if (!baseLetters.ContainsKey(pair.Key) || baseLetters[pair.Key] < pair.Value)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Counts occurrences of each character in a string.
        /// </summary>
        private Dictionary<char, int> CountLetters(string text)
        {
            Dictionary<char, int> letters = new Dictionary<char, int>();

            foreach (char c in text.ToLower())
            {
                if (!letters.ContainsKey(c))
                    letters[c] = 0;

                letters[c]++;
            }

            return letters;
        }
    }
}

