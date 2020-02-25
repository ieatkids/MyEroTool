using System;

namespace MyEroTool
{
    public static class ConsoleHelper
    {
        public static void ShowHeaders(params string[] headers)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("----------------------------------");
            Console.WriteLine(string.Join('\t', headers));
            Console.WriteLine("----------------------------------");
            Console.ForegroundColor = default;
        }
    }
}