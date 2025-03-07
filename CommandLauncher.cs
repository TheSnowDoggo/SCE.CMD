namespace CMD
{
    internal class CommandLauncher
    {
        public CommandLauncher(int capacity = 0)
        {
            Commands = new(capacity);
        }

        public Dictionary<string, Command> Commands { get; init; }

        public bool RunCommand(string input, bool safeEvaluation = true)
        {
            string name = ArrayUtils.BuildWhile(input, (c) => c != ' ');
            if (!Commands.TryGetValue(name, out var command))
            {
                StringUtils.PrettyErr("Launcher", $"Unrecognised command \'{name}\'.");
                return false;
            }
            var args = ArrayUtils.TrimArgs(input);
            if (args.Length < command.MinArgs)
            {
                StringUtils.PrettyErr("Launcher", $"Too few arguments provided for command \'{name}\' (minimum of {command.MinArgs}, received {args.Length}).");
                return false;
            }
            if (command.MaxArgs >= 0 && args.Length > command.MaxArgs)
            {
                StringUtils.PrettyErr("Launcher", $"Too many arguments provided for command \'{name}\' (maximum of {command.MaxArgs}, recieved {args.Length}).");
                return false;
            }
            if (!safeEvaluation)
                command.Action(args);
            else
            {
                try
                {
                    command.Action(args);
                }
                catch
                {
                    StringUtils.PrettyErr("Launcher", $"Command \'{name}\' failed to execute.");
                    return false;
                }
            }
            return true;
        }
    }
}
