using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WordGame_Task1
{
    internal class WordValidator
    {
        private readonly bool isRussian;

        public WordValidator(bool isRussian)
        {
            this.isRussian = isRussian;
        }

        public bool IsOnlyLetters(string word)
        {
            if (isRussian)
                return Regex.IsMatch(word, @"^[а-яА-ЯёЁ]+$");
            else
                return Regex.IsMatch(word, @"^[a-zA-Z]+$");
        }

        public bool CanBeMadeFrom(string baseWord, string word)
        {
            var baseLetters = CountLetters(baseWord); //словарь количества букв в базовом слове 
            var wordLetters = CountLetters(word);     //словарь количества букв в слове игрока

            foreach (var kvp in wordLetters)
            { //проверка,что буква есть в исходном слове и совпадения количества этой буквы    
                if (!baseLetters.ContainsKey(kvp.Key) || baseLetters[kvp.Key] < kvp.Value)
                    return false;
            }
            return true;
        }

        private Dictionary<char, int> CountLetters(string s)
        {
            Dictionary<char, int> dict = new Dictionary<char, int>();
            foreach (char c in s.ToLower())  //перебираем каждый символ строки
            {
                if (!dict.ContainsKey(c))    //проверяем есть ли буква в словаре
                    dict[c] = 0;
                dict[c]++;
            }
            return dict;
        }
    }
}
