using System.Text;

namespace CMD
{
    internal static class Launcher
    {
        internal static void Main()
        {
            CommandLauncher launcher = new()
            {
                Packages = new()
                {
                    new ConsoleCommands(),
                    new Evaluator(),
                    new Script(),
                    new Prime(),
                    new Storage(),
                },
                Native = new(new()
                {
                    { "printl", new(Command.Translator(PrintLCMD, new[] { typeof(string), typeof(int), typeof(bool) }))
                        { MinArgs = 0, MaxArgs = 3, Description = "Prints the string a given amount of times with a new line." } },
                    { "print", new(Command.Translator(PrintCMD, new[] { typeof(string), typeof(int) }) )
                        { MinArgs = 1, MaxArgs = 2, Description = "Prints the string a given amount of times." } },
                    { "feedback", Command.QCommand<bool>((c, cb) => cb.Launcher.CommandFeedback = c) },
                    { "help", new(Help) { Description = "Displays every command." } },
                }),
            };

            launcher.Run();
        }

        private static void Help(string[] args, Command.Callback cb)
        {
            StringBuilder sb = new("- Commands -\n");
            foreach (var package in cb.Launcher.Packages.Prepend(cb.Launcher.Native))
            {
                sb.AppendLine(package.Name == "" ? "Anonymous Package:\n" : $"{package.Name}:\n");
                foreach (var item in package.Commands)
                {
                    var command = item.Value;
                    sb.AppendLine($"{item.Key}[{command.MinArgs}-{command.MaxArgs}]");
                    if (command.Description != string.Empty)
                        sb.AppendLine($"> {command.Description}");
                    sb.AppendLine();
                }
            }
            Console.Write(sb.ToString());
        }

        private static void PrintLCMD(object[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine();
                return;
            }
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

        private static void PrintCMD(object[] args)
        {
            int count = args.Length >= 2 ? (int)args[1] : 1;
            string write = StringUtils.Copy((string)args[0], count);
            if (write != string.Empty)
                Console.Write(write);
        }
    }
}