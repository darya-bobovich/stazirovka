namespace WordGame_Task1
{
    /// <summary>
    /// Defines methods for user interaction, independent from any specific UI.
    /// </summary>
    public interface IUserInterface
    {
        void Clear();
        void Write(string text);
        void WriteLine(string text);
        string ReadLine();
    }
}
