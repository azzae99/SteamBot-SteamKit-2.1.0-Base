using System;
using System.IO;

namespace SteamBot
{
    public class Logger
    {
        private StreamWriter Stream;
        private readonly string Name;

        public Logger(string BotUsername)
        {
            Name = BotUsername;
            Directory.CreateDirectory("00_Logs");
            Stream = File.AppendText(Path.Combine("00_Logs", Name + ".log"));
            Stream.AutoFlush = true;
        }

        public void Debug(string output, params object[] args)
        {
            OutputLine(ConsoleColor.DarkGray, String.Format("[DEBUG]: {0}", String.Format(output, args)));
        }

        public void Info(string output, params object[] args)
        {
            OutputLine(ConsoleColor.White, String.Format("[INFO]: {0}", String.Format(output, args)));
        }

        public void Success(string output, params object[] args)
        {
            OutputLine(ConsoleColor.Green, String.Format("[SUCCESS]: {0}", String.Format(output, args)));
        }

        public void Warn(string output, params object[] args)
        {
            OutputLine(ConsoleColor.Yellow, String.Format("[WARNING]: {0}", String.Format(output, args)));
        }

        public void Error(string output, params object[] args)
        {
            OutputLine(ConsoleColor.Red, String.Format("[ERROR]: {0}", String.Format(output, args)));
        }

        public void Prompt(string output, params object[] args)
        {
            OutputLine(ConsoleColor.Magenta, String.Format("[PROMPT]: {0}", String.Format(output, args)));
        }

        public void OutputLine(ConsoleColor color, string output)
        {
            string conOutput = String.Format("{0} {1} {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Name, output);
            string fileOutput = String.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), output);
            Console.ForegroundColor = color;
            Console.WriteLine(conOutput);
            Console.ForegroundColor = ConsoleColor.White;
            Stream.WriteLine(fileOutput);
        }
    }
}
