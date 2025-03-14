namespace SCE
{
    internal class ConsolePKG : Package
    {
        public ConsolePKG()
        {
            Name = "Console";
            Commands = new()
            {
                { "printl", new(Cmd.Translator(PrintLCMD, new[] { typeof(string), typeof(int), typeof(bool) }))
                    { MinArgs = 0, MaxArgs = 3, Description = "Prints the string a given amount of times with a new line." } },

                { "print", new(Cmd.Translator(PrintCMD, new[] { typeof(string), typeof(int) }) )
                    { MinArgs = 1, MaxArgs = 2, Description = "Prints the string a given amount of times." } },

                { "fg", new(SetColorCMD(true)) { MinArgs = 1, MaxArgs = 1,
                    Description = "Sets the Foreground color of the Console." } },

                { "bg", new(SetColorCMD(false)) { MinArgs = 1, MaxArgs = 1,
                    Description = "Sets the Background color of the Console." } },

                { "resetcolor", new(args => Console.ResetColor()) { 
                    Description = "Resets the foreground and background colors." } },

                { "clear", new(args => Console.Clear()) {
                    Description = "Clears the Console." } },

                { "cursor", Cmd.QCommand<bool>(c => Console.CursorVisible = c,
                    "Sets the visible state of the Cursor.") },

                { "title", Cmd.QCommand<string>(name => Console.Title = name,
                    "Sets the title of the Console.") },

                { "beep", new(BeepCMD) { MinArgs = 0, MaxArgs = 2,
                    Description = "Makes a beep sound." } },
            };
        }

        private static void PrintLCMD(object[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine();
                return;
            }
            int count = args.Length >= 2 ? (int)args[1] : 1;
            bool newLine = args.Length < 3 || (bool)args[2];
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
            string write = StrUtils.Copy((string)args[0], count);
            if (write != string.Empty)
                Console.Write(write);
        }

        private void BeepCMD(string[] args)
        {
            int frequency = 2000;
            if (args.Length > 0 && !int.TryParse(args[0], out frequency))
                throw new CmdException("Console", $"Frequency invalid \'{args[0]}\'.");
            int duration = 1000;
            if (args.Length > 1 && !int.TryParse(args[1], out duration))
                throw new CmdException("Console", $"Duration invalid \'{args[1]}\'.");
            Console.Beep(frequency, duration);
        }

        private static Action<string[]> SetColorCMD(bool foreground)
        {
            return args =>
            {
                if (!Enum.TryParse(args[0], true, out ConsoleColor result))
                    throw new CmdException("SetColor", $"Invalid color '{args[0]}'.");
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
    }
}
