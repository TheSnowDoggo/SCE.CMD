namespace CMD
{
    internal class CommandLauncher
    {
        public CommandLauncher(int capacity = 0)
        {
            Commands = new(capacity);
        }

        public Dictionary<string, Command> Commands { get; init; }

        public bool RunCommand(string name, string[] args, bool feedback = true)
        {
            if (!Commands.TryGetValue(name, out var command))
            {
                if (feedback)
                    Console.WriteLine($"<!> Unrecognised command \'{name}\'. <!>");
                return false;
            }
            if (args.Length < command.MinArgs)
            {
                if (feedback)
                    Console.WriteLine($"<!> Too few arguments provided for command \'{name}\' (minimum of {command.MinArgs}, received {args.Length}). <!>");
                return false;
            }
            if (command.MaxArgs >= 0 && args.Length > command.MaxArgs)
            {
                if (feedback)
                    Console.WriteLine($"<!> Too many arguments provided for command \'{name}\' (maximum of {command.MaxArgs}, recieved {args.Length}). <!>");
            }
            try
            {
                command.Action(args);
                return true;
            }
            catch
            {
                Console.WriteLine($"<!> Command \'{name}\' failed to execute. <!>");
                return false;
            }
        }
    }
}
