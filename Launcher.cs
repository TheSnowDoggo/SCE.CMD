namespace CMD
{
    internal static class Launcher
    {
        internal static void Main()
        {
            CommandLauncher launcher = new()
            {
                Commands = new()
                {
                    { "print", new(Print) { MinArgs = 1, MaxArgs = 3 } },
                    { "fg", new(SetColor(true)) { MinArgs = 1, MaxArgs = 1 } },
                    { "bg", new(SetColor(false)) { MinArgs = 1, MaxArgs = 1 } },
                    { "clear", new(args => Console.Clear()) },
                    { "scursor", new(args => Console.CursorVisible = true) },
                    { "hcursor", new(args => Console.CursorVisible = false) },
                },
            };

            while (true)
            {
                string input = Console.ReadLine() ?? "";

                string name = ArrayUtils.BuildWhile(input, (c) => c != ' ');
                var args = ArrayUtils.TrimArgs(input);

                launcher.RunCommand(name, args);
            }
        }

        private static Action<string[]> SetColor(bool foreground)
        {
            return args =>
            {
                if (!Enum.TryParse(args[0], true, out ConsoleColor result))
                {
                    Console.WriteLine($"<!> ForegroundColor | Invalid color \'{args[0]}\'. <!>");
                    return;
                }
                if (foreground)
                {
                    Console.ForegroundColor = result;
                    Console.WriteLine($"Foreground set to {result}.");
                }
                else
                {
                    Console.BackgroundColor = result;
                    Console.WriteLine($"Background set to {result}.");
                }
            };
        }

        private static void SetBackgroundColor(string[] args)
        {
            if (!Enum.TryParse(args[0], out ConsoleColor result))
            {
                Console.WriteLine($"<!> BackgroundColor | Invalid color \'{args[0]}\'. <!>");
                return;
            }
            Console.ForegroundColor = result;
        }

        private static void Print(string[] args)
        {
            int count = 1;
            if (args.Length >= 2 && !int.TryParse(args[1], out count))
            {
                Console.WriteLine($"<!> Print | Invalid count \'{args[1]}'. <!>");
                return;
            }
            bool newLine = true;
            if (args.Length >= 3 && !bool.TryParse(args[2], out newLine))
            {
                Console.WriteLine($"<!> Print | Invalid newline \'{args[2]}\'. <!>");
                return;
            }
            for (int i = 0; i < count; ++i)
            {
                if (newLine || i == count - 1)
                    Console.WriteLine(args[0]);
                else
                    Console.Write(args[0]);
            }
        }
    }
}