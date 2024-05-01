namespace Dictionary
{
    public class Program
    { 
        public static async Task Main()
        {
            await TelegramBot.Instance.Start();
            Console.ReadLine();
            TelegramBot.Instance.Stop();
        }
    }
}