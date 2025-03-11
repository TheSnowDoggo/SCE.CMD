namespace CMD
{
    internal class PrintPKG : Package
    {
        public PrintPKG()
        {
            Name = "Print";
            Commands = new()
            {
                { "printl", new(Cmd.Translator(PrintLCMD, new[] { typeof(string), typeof(int), typeof(bool) }))
                    { MinArgs = 0, MaxArgs = 3, Description = "Prints the string a given amount of times with a new line." } },
                { "print", new(Cmd.Translator(PrintCMD, new[] { typeof(string), typeof(int) }) )
                    { MinArgs = 1, MaxArgs = 2, Description = "Prints the string a given amount of times." } },
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
