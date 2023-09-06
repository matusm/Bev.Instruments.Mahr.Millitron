using System;

namespace GBC
{
    public class MyCommandLine
    {
        public MyCommandLine(string[] args)
        {
            ParseCommandLine(args);
        }

        public bool ResetMillitron { get; private set } = false;
        public bool PerformCenter { get; private set; } = true;
        public bool PerformVariation { get; private set; } = false;
        public bool AutoMoveProbe { get; private set; } = false;

        private void ParseCommandLine(string[] args)
        {
            if (args.Length == 0) return;
            foreach (var arg in args)
            {
                switch(arg.ToLower())
                {
                    case "-x":
                    case "-x+":
                        ResetMillitron = true;
                        break;
                    case "-5":
                    case "-5+":
                        PerformVariation = true;
                        break;
                    case "-5-":
                        PerformVariation = false;
                        break;
                    case "-c":
                    case "-c+":
                        PerformCenter = true;
                        break;
                    case "-c-":
                        PerformCenter = false;
                        break;
                    case "-a":
                    case "-a+":
                        AutoMoveProbe = true;
                        break;
                    case "-a-":
                        AutoMoveProbe = false;
                        break;
                    case "--help":
                        PrintHelpAndExit();
                        break;
                    default:
                        break;
                }
            }
            if (PerformCenter == false && PerformVariation == false) PerformCenter = true;
        }

        private void PrintHelpAndExit()
        {
            Console.WriteLine($"Usage: GBC -c[+|-] -5[+|-] -a[+|-] ");
            Console.WriteLine($"   -c : center length measurement (true)");
            Console.WriteLine($"   -5 : variation of length measurement (false)");
            Console.WriteLine($"   -a : move probe automatically (false)");
            Console.WriteLine();
            Environment.Exit(0);
        }
    }
}
