using System.Text;
using CSUtils;
namespace SCE
{
    internal class ConsolePKG : Package
    {
        public ConsolePKG()
        {
            Name = "Console";
            Version = "0.0.0";
            Commands = new()
            {
                { "printl", new(PrintlCMD) { MaxArgs = -1, 
                    Description = "Prints the given message with a newline.",
                    Usage = "?<Message1>..." } },

                { "print", new(PrintCMD) { MinArgs = 1, MaxArgs = -1, 
                    Description = "Prints the given message.",
                    Usage = "<Message>" } },

                { "escins", new(EscapeInsertCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Inserts control characters into the given command.",
                    Usage = "<CommandName> ?<Arg1>..." } },

                { "readl", new(a => new Cmd.MemItem(Console.ReadLine())) {
                    Description = "Reads a line from the console." } },

                { "readkey", new(a => new Cmd.MemItem(Console.ReadKey())) {
                    Description = "Reads a key from the console." } },

                { "fg", new(SetColorCMD(true)) { MinArgs = 1, MaxArgs = 1,
                    Description = "Sets the Foreground color of the Console.",
                    Usage = "<ConsoleColor>" } },

                { "bg", new(SetColorCMD(false)) { MinArgs = 1, MaxArgs = 1,
                    Description = "Sets the Background color of the Console.",
                    Usage = "<ConsoleColor>" } },

                { "resetcolor", new(args => Console.ResetColor()) { 
                    Description = "Resets the foreground and background colors." } },

                { "clear", new(args => Console.Clear()) {
                    Description = "Clears the Console." } },

                { "setcurpos", new(SetCurPosCMD) { MinArgs = 2, MaxArgs = 2,
                    Description = "Sets the cursor position.",
                    Usage = "<Left> <Top>" } },

                { "getcurpos", new(GetCurPosCMD) {
                    Description = "Gets the cursor position." } },

                { "curvis", Cmd.QCommand<bool>(c => Console.CursorVisible = c,
                    "Sets the visible state of the Cursor.") },

                { "title", new(TitleCMD) {
                    Description = "Sets the title of the Console.",
                    Usage = "<Title>" } },

                { "beep", new(BeepCMD) { MinArgs = 0, MaxArgs = 2,
                    Description = "Makes a beep sound.",
                    Usage = "?<Frequency(Hz)->2000Hz> ?<Duration(ms)->1000ms>" } },
            };
        }

        #region Commands

        private static void PrintlCMD(string[] args)
        {
            StringBuilder sb = new();
            for (int i = 0; i < args.Length; ++i)
            {
                if (i != 0)
                    sb.Append(' ');
                sb.Append(args[i]);
            }
            Console.WriteLine(sb.ToString());
        }

        private static void PrintCMD(string[] args)
        {
            StringBuilder sb = new();
            for (int i = 0; i < args.Length; ++i)
            {
                if (i != 0)
                    sb.Append(' ');
                sb.Append(args[i]);
            }
            Console.Write(sb.ToString());
        }

        private static void TitleCMD(string[] args, Cmd.Callback cb)
        {
            Console.Title = args[0];
            cb.Launcher.FeedbackLine($"Title set to \'{args[0]}\'.");
        }

        private static void SetCurPosCMD(string[] args)
        {
            Console.SetCursorPosition(int.Parse(args[0]), int.Parse(args[1]));
        }

        private static Cmd.MemItem GetCurPosCMD(string[] args, Cmd.Callback cb)
        {
            var pos = Console.GetCursorPosition();
            cb.Launcher.FeedbackLine(pos);
            return new(pos);
        }

        private static void EscapeInsertCMD(string[] args, Cmd.Callback cb)
        {
            for (int i = 1; i < args.Length; ++i)
                args[i] = Utils.InsertEscapeCharacters(args[i]);
            cb.Launcher.ExecuteCommand(Utils.Infill(args, " "));
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
            string write = Utils.Copy((string)args[0], count);
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

        private static Action<string[], Cmd.Callback> SetColorCMD(bool foreground)
        {
            return (args, cb) =>
            {
                if (!Enum.TryParse(args[0], true, out ConsoleColor result))
                    throw new CmdException("SetColor", $"Invalid color '{args[0]}'.");
                if (foreground)
                {
                    Console.ForegroundColor = result;
                    cb.Launcher.FeedbackLine($"Foreground set to {result}.");
                }
                else
                {
                    Console.BackgroundColor = result;
                    cb.Launcher.FeedbackLine($"Background set to {result}.");
                }
            };
        }

        #endregion
    }
}
