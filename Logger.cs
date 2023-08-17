using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spectre.Console;

namespace AppiumApp
{
    public class Logger
    {
        public void Console(string text)
        {
            AnsiConsole.MarkupLine($"[grey]{text}[/]");
        }

        public void ErrorLogFile(string text)
        {
            string currentDir = Directory.GetCurrentDirectory();
            string logFile = $@"{currentDir}/errorLog.txt";
            var date = DateTime.Now;

            File.AppendAllText(logFile, $"{date}: {text}\n");
            //File.AppendAllText(logFile, $"{text}\n");
        }

        public void MainLogFile(string text)
        {
            string currentDir = Directory.GetCurrentDirectory();
            string logFile = $@"{currentDir}/log.txt";

            File.AppendAllText(logFile, text + Environment.NewLine);
        }
    }
}
