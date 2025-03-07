using SCE;

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
                    { "print", new(Command.Translator(PrintT,
                        new[] { typeof(string), typeof(int), typeof(bool) })) { MinArgs = 1, MaxArgs = 3 } },
                    { "fg", new(SetColor(true)) { MinArgs = 1, MaxArgs = 1 } },
                    { "bg", new(SetColor(false)) { MinArgs = 1, MaxArgs = 1 } },
                    { "clear", new(args => Console.Clear()) },
                    { "cursor", Command.QCommand<bool>(c => Console.CursorVisible = c) },
                    { "eval", new(Command.Translator(Evaluate, 
                    new[] { typeof(string), typeof(int) })) { MinArgs = 1, MaxArgs = 1 } },
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
                    StringUtils.PrettyErr("SetColor", $"Invalid color '{args[0]}'.");
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

        private static void Evaluate(object[] args)
        {
            if (!RPNConverter.BasicDouble.TryEvaluate((string)args[0], out var result))
            {
                StringUtils.PrettyErr("Evaluate", "Unable to evlaute.");
                return;
            }
            if (args.Length >= 2)
                result = Math.Round(result, (int)args[1]);
            Console.WriteLine(result);
        }

        private static void PrintT(object[] args)
        {
            int count = args.Length >= 2 ? (int)args[1] : 1;
            bool newLine = args.Length >= 3 ? (bool)args[2] : true;
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