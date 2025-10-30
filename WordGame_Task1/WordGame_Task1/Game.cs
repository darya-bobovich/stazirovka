using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Timer = System.Timers.Timer;

namespace WordGame_Task1
{
    internal class Game
    {
        private readonly LanguageManager lang;      //вызов чтения класса языка игры 
        private readonly WordValidator validator;   //вызов чтения класса проверки слов

        public Game(LanguageManager lang)
        {
            this.lang = lang;                                        //сохраняет ссылку на обьект LanguageManager, чтобы знать текущий язык
            this.validator = new WordValidator(lang.IsRussian);      //создает новую проверку слов настроенный на заданный язык
        }

        public void Start()
        {
            Console.Clear();
            Console.WriteLine(lang.GetText("Введите начальное слово (от 8 до 30 букв):",  
                                           "Enter the base word (8–30 letters):"));

            string baseWord = Console.ReadLine();
            while (!validator.IsOnlyLetters(baseWord) || baseWord.Length < 8 || baseWord.Length > 30)   //проверка что слово состоит только из букв и соответствует определенной длине
            {
                Console.WriteLine(lang.GetText("Ошибка: слово должно содержать только русские буквы (8–30).",
                                               "Error: only English letters (8–30)."));
                baseWord = Console.ReadLine();
            }

            List<string> usedWords = new List<string>();  
            int currentPlayer = 1;

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine(lang.GetText($"Игрок {currentPlayer}, введите слово за 10 секунд:",
                                               $"Player {currentPlayer}, enter a word within 10 seconds:"));

                string word = ReadWordWithTimer(10); //метод таймера,который отсчитывает 10 секунд

                if (word == null)                    //таймер вышел
                {
                    Console.WriteLine(lang.GetText($"Игрок {currentPlayer} проиграл! Время вышло.",
                                                   $"Player {currentPlayer} lost! Time is up."));
                    break;
                }

                if (!validator.IsOnlyLetters(word))  //проверка ввода правильных букв в зависимости от локализации
                {
                    Console.WriteLine(lang.GetText($"Игрок {currentPlayer} проиграл! Использованы неверные символы.",
                                                   $"Player {currentPlayer} lost! Invalid alphabet."));
                    break;
                }

                if (usedWords.Contains(word))        //проверка на повтор слова
                {
                    Console.WriteLine(lang.GetText($"Слово уже использовалось! Игрок {currentPlayer} проиграл!",
                                                   $"Word already used! Player {currentPlayer} lost!"));
                    break;
                }

                if (!validator.CanBeMadeFrom(baseWord, word))  //проверка состовления слова из базового слова
                {
                    Console.WriteLine(lang.GetText($"Нельзя составить из букв исходного слова. Игрок {currentPlayer} проиграл!",
                                                   $"Cannot be made from base word. Player {currentPlayer} lost!"));
                    break;
                }

                usedWords.Add(word);                          //добавляем слово в список использованных слов
                Console.WriteLine(lang.GetText($"Слово принято: {word}", $"Word accepted: {word}"));
                currentPlayer = currentPlayer == 1 ? 2 : 1;
            }

            Console.WriteLine(lang.GetText("Нажмите Enter, чтобы начать новую игру...",
                                           "Press Enter to start a new game..."));
            Console.ReadLine();
            Console.Clear();
        }

        private string ReadWordWithTimer(int seconds)         //метод таймера
        {
            int secondsLeft = seconds;
            StringBuilder buffer = new StringBuilder();       //хранение введеных символов
            bool timeExpired = false;                         //флаг окончания врмени
            object lockObj = new object();                    //обьект, чтобы ввод и таймер не мешали друг другу

            Console.WriteLine(lang.GetText($"Осталось секунд: {secondsLeft}", $"Time left: {secondsLeft}"));
            int timerTop = Console.CursorTop - 1;
            Console.Write("> ");

            Timer timer = new Timer(1000);                    //таймер срабатывает каждую 1 секунду
            timer.Elapsed += (s, e) =>                        //событие которое работает каждый раз, когда таймер срабатывает
                                                              //s-обьект таймера, а e-аргумент события ElapsedEventArgs
            {
                lock (lockObj)                                //защита кода от одновременного доступа из разных потоков
                {
                    if (timeExpired) return;
                    secondsLeft--;
                    if (secondsLeft < 0)
                    {
                        timeExpired = true;
                        timer.Stop();
                        return;
                    }

                    try  //чтобы программа выводилось корректно, когда курсор выходит за пределы консоли
                    {
                        Console.SetCursorPosition(0, timerTop);   //кусор в начало строки где выводится таймер
                        Console.Write(new string(' ', Console.WindowWidth - 1)); //стираем текст таймера(обновляем таймер)
                        Console.SetCursorPosition(0, timerTop);
                        Console.Write(lang.GetText($"Осталось секунд: {secondsLeft}", $"Time left: {secondsLeft}"));
                        Console.SetCursorPosition(2 + buffer.Length, timerTop + 1);  //перемещаем курсор на строчку для ввода игрока
                    }
                    catch { }
                }
            };
            timer.Start();

            while (!timeExpired)
            {
                if (Console.KeyAvailable)  //проверяет нажата ли клавиша
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)  //если нажат enter то ввод завершён
                        break;
                    else if (key.Key == ConsoleKey.Backspace && buffer.Length > 0) //если нажат backspace то удаляем последнюю букву
                    {
                        buffer.Length--;
                        Console.Write("\b \b");
                    }
                    else if (char.IsLetter(key.KeyChar)) //если введена буква 
                    {
                        buffer.Append(key.KeyChar);      //добавляем в хранилище buffer
                        Console.Write(key.KeyChar);
                    }
                }

                Thread.Sleep(50);  //пауза 50мс
            }

            timer.Stop();          //остановка таймера
            Console.WriteLine();

            if (timeExpired && buffer.Length == 0)   //если время истекло или игрок ничего не ввел 
                return null;

            return buffer.ToString().ToLower();
        }
    }
}

