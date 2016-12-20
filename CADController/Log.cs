using System;
using System.Collections.Generic;

namespace CADCore
{
    public class Log
    {
        private static readonly List<string> StaticLogList = new List<string>();

        public static List<string> LogList
        {
            get { return StaticLogList; }
        }

        public static void Fatal(string description)
        {
            LogList.Add("Fatal: " + description);
            Console.WriteLine(description);
        }

        public static void Error(string description)
        {
            LogList.Add("Error: " + description);
            Console.WriteLine(description);
        }

        public static void Warning(string description)
        {
            LogList.Add("Warning: " + description);
            Console.WriteLine(description);
        }

        public static void Info(string description)
        {
            LogList.Add("Info: " + description);
            Console.WriteLine(description);
        }
    }
}
