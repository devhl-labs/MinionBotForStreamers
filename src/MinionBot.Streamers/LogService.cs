using System;

namespace MinionBot.Streamers
{
    public enum LogLevel
    {
        Trace,
        Debug,
        Information,
        Warning,
        Error,
        Critical,
        None
    }

    public static class LogService
    {
        private static readonly object _logLock = new();

        private static void ResetConsoleColor()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void PrintLogLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("[trac] ");
                    break;

                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("[dbug] ");
                    break;

                case LogLevel.None:
                    break;
                case LogLevel.Information:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("[info] ");
                    break;

                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("[warn] ");
                    break;

                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.Write("[err]");
                    ResetConsoleColor();
                    Console.Write("  ");
                    break;

                case LogLevel.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("[crit] ");
                    break;
            }

            ResetConsoleColor();
        }

        public static void Log(LogLevel logLevel, params string?[] messages)
        {
            lock (_logLock)
            {
                PrintLogLevel(logLevel);

                Console.Write(DateTime.UtcNow.ToString("HH:mm"));

                foreach (string? message in messages)
                {
                    if (string.IsNullOrEmpty(message))
                        continue;

                    Console.Write($" | {message}");
                }

                Console.WriteLine();
            }
        }

        //public static void Log(string message)
        //{
        //    lock (_logLock)
        //    {
        //        Console.Write(message);
        //    }
        //}

        public static void LogLine(string message)
        {
            lock (_logLock)
            {
                Console.WriteLine(message);
            }
        }
    }
}