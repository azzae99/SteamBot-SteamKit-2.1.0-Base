using System;
using System.IO;

namespace SteamBot
{
    public class Logger
    {
        private StreamWriter Stream;
        private readonly string Username;

        public Logger(string username)
        {
            Username = username;
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "00_Logs"));
            Stream = File.AppendText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "00_Logs", $"{Username}.log"));
            Stream.AutoFlush = true;
        }

        public void Debug(string output, params object[] args)
        {
            OutputLine(ConsoleColor.DarkGray, $"[DEBUG]: { (args.Length == 0 ? output : String.Format(output, args)) }");
        }

        public void Info(string output, params object[] args)
        {
            OutputLine(ConsoleColor.White, $"[INFO]: { (args.Length == 0 ? output : String.Format(output, args)) }");
        }

        public void Success(string output, params object[] args)
        {
            OutputLine(ConsoleColor.Green, $"[SUCCESS]: { (args.Length == 0 ? output : String.Format(output, args)) }");
        }

        public void Warn(string output, params object[] args)
        {
            OutputLine(ConsoleColor.Yellow, $"[WARNING]: { (args.Length == 0 ? output : String.Format(output, args)) }");
        }

        public void Error(string output, params object[] args)
        {
            OutputLine(ConsoleColor.Red, $"[ERROR]: { (args.Length == 0 ? output : String.Format(output, args)) }");
        }

        public void Prompt(string output, params object[] args)
        {
            OutputLine(ConsoleColor.Magenta, $"[PROMPT]: { (args.Length == 0 ? output : String.Format(output, args)) }");
        }

        public void OutputLine(ConsoleColor color, string output)
        {
            string Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.ForegroundColor = color;
            Console.WriteLine($"{Timestamp} {Username} {output}");
            Console.ForegroundColor = ConsoleColor.White;
            Stream.WriteLine($"{Timestamp} {output}");
        }
    }
}
