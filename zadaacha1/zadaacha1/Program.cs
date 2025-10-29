using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace zadaacha1
{
    internal class Program
    {
        static void Main()
        {
           

            int choice = 0;
            do
            {
                choice = ShowMenu();

                switch (choice)
                {
                    case 1:
                        StartGame();
                        break;
                    case 2:
                        ChooseLanguage();
                        break;
                    case 3:
                        Console.WriteLine(isRussian ? "Выход" : "Exit");
                        break;
                    default:
                        Console.WriteLine(isRussian ? "Ошибка ввода!" : "Input error!");
                        break;
                }

            } while (choice != 3);
            return;
        }


        static void StartGame()
        {
            string base_word = "";
            bool Length = false;

            // Ввод базового слова
            while (!Length)
            {
                Console.WriteLine(isRussian ?
                    "Введите начальное слово (от 8 до 30 букв):" :
                    "Enter the base word (8–30 letters):");

                base_word = Console.ReadLine();

                if (!IsOnlyLetters(base_word))
                {
                    Console.WriteLine(isRussian ?
                        "Ошибка: используйте только буквы!" :
                        "Error: use only letters!");
                    continue;
                }

                Length = (base_word.Length >= 8 && base_word.Length <= 30);
                if (!Length)
                {
                    Console.WriteLine(isRussian ?
                        "Ошибка: длина слова должна быть от 8 до 30 символов." :
                        "Error: word length must be 8–30 characters.");
                }
            }

            List<string> UsedWords = new List<string>();
            int currentPlayer = 1;

            // Основной игровой цикл
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine(isRussian ?
                    $"Игрок {currentPlayer}, введите слово за 10 секунд:" :
                    $"Player {currentPlayer}, enter a word within 10 seconds:");

                StringBuilder inputBuffer = new StringBuilder();
                DateTime endTime = DateTime.Now.AddSeconds(10);

                // Сохраняем строки для таймера и ввода
                int timerLine = Console.CursorTop;
                Console.WriteLine(); // строка для таймера
                int inputLine = Console.CursorTop;
                Console.WriteLine(); // строка для ввода

                while (DateTime.Now < endTime)
                {
                    // Очистка строки таймера перед перерисовкой
                    Console.SetCursorPosition(0, timerLine);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, timerLine);

                    int secondsLeft = (int)(endTime - DateTime.Now).TotalSeconds;
                    Console.Write(isRussian ? $"Осталось секунд: {secondsLeft}" : $"Time left: {secondsLeft}");

                    // Очистка строки ввода перед отображением
                    Console.SetCursorPosition(0, inputLine);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, inputLine);

                    Console.Write(inputBuffer.ToString());
                    Console.SetCursorPosition(inputBuffer.Length, inputLine);

                    // Считывание клавиш
                    while (Console.KeyAvailable)
                    {
                        var keyInfo = Console.ReadKey(intercept: true);
                        if (keyInfo.Key == ConsoleKey.Enter)
                        {
                            Console.WriteLine();
                            goto InputFinished;
                        }
                        else if (keyInfo.Key == ConsoleKey.Backspace)
                        {
                            if (inputBuffer.Length > 0)
                                inputBuffer.Length--;
                        }
                        else
                        {
                            inputBuffer.Append(keyInfo.KeyChar);
                        }
                    }

                    Thread.Sleep(50);
                }

            InputFinished:
                string word = inputBuffer.ToString().ToLower();
                Console.WriteLine();


                if (string.IsNullOrEmpty(word))
                {
                    Console.WriteLine(isRussian ?
                        $"Игрок {currentPlayer} проиграл! Слово не было введено." :
                        $"Player {currentPlayer} lost! No word was entered.");
                    break;
                }

                
                if (!IsOnlyLetters(word))
                {
                    Console.WriteLine(isRussian ?
                        "Ошибка! Разрешены только буквы!" :
                        "Error! Only letters are allowed!");
                    continue;
                }

                
                if (UsedWords.Contains(word))
                {
                    Console.WriteLine(isRussian ?
                        $"Слово уже использовалось! Игрок {currentPlayer} проиграл!" :
                        $"Word already used! Player {currentPlayer} lost!");
                    break;
                }

                
                if (!IsLetterCorrespond(base_word, word))
                {
                    Console.WriteLine(isRussian ?
                        $"Слово нельзя составить из букв исходного слова. Игрок {currentPlayer} проиграл!" :
                        $"Word cannot be made from the base word. Player {currentPlayer} lost!");
                    break;
                }

                
                UsedWords.Add(word);
                currentPlayer = (currentPlayer == 1) ? 2 : 1;
            }
        }


        static bool isRussian = true;
        static bool IsOnlyLetters(string word)
        {
            if (isRussian)
                return Regex.IsMatch(word, @"^[а-яА-ЯёЁ]+$");
            else
                return Regex.IsMatch(word, @"^[a-zA-Z]+$");
        }
       
        static bool IsLetterCorrespond(string base_Word, string word)
        {
            Dictionary<char, int> baseLetters = new Dictionary<char, int>();
            Dictionary<char, int> playerLetters = new Dictionary<char, int>();

            foreach (char c in base_Word)
            {
                if (!baseLetters.ContainsKey(c))
                    baseLetters[c] = 0;
                baseLetters[c]++;
            }

            foreach (char c in word)
            {
                if (!playerLetters.ContainsKey(c))
                    playerLetters[c] = 0;
                playerLetters[c]++;
            }

            foreach (var pair in playerLetters)
            {       //есть ли такая буква в исходном слове     кол-во букв играка > кол-во базового слова
                if (!baseLetters.ContainsKey(pair.Key) || playerLetters[pair.Key] > baseLetters[pair.Key])
                    return false;
            }

            return true;
        }
        static void ChooseLanguage()
        {
                Console.WriteLine("1. Русский");
                Console.WriteLine("2. English");

                int choice;
                if (!int.TryParse(Console.ReadLine(), out choice))
                    return;

                if (choice == 1)
                    isRussian = true;
                else if (choice == 2)
                    isRussian = false;
        }
        static int ShowMenu()
        {
            if (isRussian)
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

            int choice;
            if (!int.TryParse(Console.ReadLine(), out choice))
                choice = -1; 

            return choice;
        }

    }
        
        
    
}

