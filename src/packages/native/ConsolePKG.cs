using System.Text;
using CSUtils;
namespace SCE
{
    internal class ConsolePKG : Package
    {
        public ConsolePKG()
        {
            Name = "Console";
            Version = new(0, 2, 0);
            Desc = "Console IO management.";
            Commands = new()
            {
                { "printl", new(PrintlCMD) { Max = -1, 
                    Desc = "Prints the given message with a newline.",
                    Usage = "?<Message1>..." } },

                { "print", new(PrintCMD) { Min = 1, Max = -1, 
                    Desc = "Prints the given message.",
                    Usage = "<Message>" } },

                { "escins", new(EscapeInsertCMD) { Min = 1, Max = -1,
                    Desc = "Inserts control characters into the given command.",
                    Usage = Cmd.BCHAIN } },

                { "readl", new(a => new Cmd.MItem(Console.ReadLine())) {
                    Desc = "Reads a line from the console." } },

                { "readkey", new(ReadKeyCMD) { Min = 1,
                    Desc = "Console.ReadKey().",
                    Usage = "?<True/False->False>" } },

                { "read", new(a => new Cmd.MItem(Console.Read())) {
                    Desc = "Reads the next character from the input stream." } },

                { "fg", new(SetColorGEN(true)) { Min = 1, Max = 1,
                    Desc = "Sets the Foreground color of the Console.",
                    Usage = "<ConsoleColor>" } },

                { "bg", new(SetColorGEN(false)) { Min = 1, Max = 1,
                    Desc = "Sets the Background color of the Console.",
                    Usage = "<ConsoleColor>" } },

                { "boolprompt", new(BoolPromptCMD) { Max = -1,
                    Desc = "Reads a yes or no response from user and outputs result.",
                    Usage = "?<Msg1>..." } },

                { "resetcolor", new(args => Console.ResetColor()) { 
                    Desc = "Resets the foreground and background colors." } },

                { "clear", new(args => Console.Clear()) {
                    Desc = "Clears the Console." } },

                { "setcurpos", new(SetCurPosCMD) { Min = 2, Max = 2,
                    Desc = "Sets the cursor position.",
                    Usage = "<Left> <Top>" } },

                { "getcurpos", new(GetCurPosCMD) {
                    Desc = "Gets the cursor position." } },

                { "curvis", Cmd.QCommand<bool>(c => Console.CursorVisible = c,
                    "Sets the visible state of the Cursor.") },

                { "title", new(TitleCMD) {
                    Desc = "Sets the title of the Console.",
                    Usage = "<Title>" } },

                { "beep", new(BeepCMD) { Min = 0, Max = 2,
                    Desc = "Makes a beep sound.",
                    Usage = "?<Frequency(Hz)->2000Hz> ?<Duration(ms)->1000ms>" } },
            };
        }

        #region Commands

        private static void PrintlCMD(string[] args)
        {
            Console.WriteLine(Utils.Infill(args, " "));
        }

        private static void PrintCMD(string[] args)
        {
            Console.Write(Utils.Infill(args, " "));
        }

        private static void TitleCMD(string[] args, CmdLauncher cl)
        {
            Console.Title = args[0];
            cl.FeedbackLine($"Title set to \'{args[0]}\'.");
        }

        private static void SetCurPosCMD(string[] args)
        {
            Console.SetCursorPosition(int.Parse(args[0]), int.Parse(args[1]));
        }

        private static Cmd.MItem GetCurPosCMD(string[] args, CmdLauncher cl)
        {
            var pos = Console.GetCursorPosition();
            cl.FeedbackLine(pos);
            return new($"{pos.Left} {pos.Top}");
        }

        private static void EscapeInsertCMD(string[] args, CmdLauncher cl)
        {
            for (int i = 1; i < args.Length; ++i)
                args[i] = Utils.InsertEscapeCharacters(args[i], @"*/");
            cl.ExecuteCommand(args[0], Utils.TrimFirst(args));
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

        private static Cmd.MItem ReadKeyCMD(string[] args)
        {
            bool intercept = false;
            if (args.Length > 0 && !bool.TryParse(args[0], out intercept))
                throw new CmdException("Console", $"Cannot convert \'{args[0]}\' to bool.");
            return new(Console.ReadKey(intercept));
        }

        private static Action<string[], CmdLauncher> SetColorGEN(bool foreground)
        {
            return (args, cl) =>
            {
                if (!Enum.TryParse(args[0], true, out ConsoleColor result))
                    throw new CmdException("SetColor", $"Invalid color '{args[0]}'.");
                if (foreground)
                {
                    Console.ForegroundColor = result;
                    cl.FeedbackLine($"Foreground set to {result}.");
                }
                else
                {
                    Console.BackgroundColor = result;
                    cl.FeedbackLine($"Background set to {result}.");
                }
            };
        }

        private static Cmd.MItem BoolPromptCMD(string[] args)
        {
            if (args.Length > 0)
                PrintCMD(args);
            return new(Utils.BoolPrompt());
        }

        #endregion
    }
}
