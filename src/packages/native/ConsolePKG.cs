using System.Diagnostics;

namespace SCE
{
    internal class ConsolePKG : Package
    {
        public ConsolePKG()
        {
            Name = "Console";
            Commands = new()
            {
                { "proc", new(args => Process.Start(new ProcessStartInfo() { FileName = args[0], Arguments = StrUtils.Build(ArrUtils.TrimFirst(args)), UseShellExecute = true })) {
                    MinArgs = 1, MaxArgs = -1, Description = "Starts the specified process." } },
                
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
            };
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
