using System.Text;

namespace CMD
{
    internal class AliasPKG : Package
    {
        private readonly Dictionary<string, string> _aliases;

        public AliasPKG()
        {
            _aliases = new();

            Name = "Alias";
            Commands = new()
            {
                { "alias", new(AddAliasCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Adds a list of command aliases." } },
                { "aliasrem", new(RemoveAliasCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Removes the given command alias." } },
                { "aliasclear", new(ClearAliasCMD) { Description = "Clears all command aliases." } },
                { "aliasview", new(ViewAliasCMD) { Description = "Displays all command aliases." } },
            };
        }

        private void ClearAliasCMD(string[] args, Cmd.Callback cb)
        {
            if (_aliases.Count == 0)
                cb.Launcher.FeedbackLine($"No items to clear.");
            else
            {
                cb.Launcher.FeedbackLine($"Successfully cleared {_aliases.Count} aliases.");
                _aliases.Clear();
            }
        }

        private void ViewAliasCMD(string[] args)
        {
            if (_aliases.Count == 0)
                Console.WriteLine("No alias comamnds to display.");
            else
            {
                StringBuilder sb = new();
                foreach (var alias in _aliases)
                    sb.AppendLine($"\'{alias.Key}\' -> \'{alias.Value}\'");
                Console.Write(sb.ToString());
            }
        }

        private void AddAliasCMD(string[] args, Cmd.Callback cb)
        {
            foreach (var alias in args)
            {
                var split = alias.Split("->");
                if (split.Length != 2)
                    throw new CommandException("Alias", $"Invalid alias \'{alias}\'.");
                if (!cb.Launcher.TryGetCommand(split[0], out var command))
                    throw new CommandException("Alias", $"Unknown command \'{split[0]}\'.");
                if (cb.Launcher.CommandExists(split[1]))
                    throw new CommandException("Alias", $"Command with name \'{split[1]}\' already exists.");
                Commands.Add(split[1], command);
                _aliases[split[1]] = split[0];
                cb.Launcher.FeedbackLine($"\'{split[1]}\' -> \'{split[0]}\'");
            }
        }

        private void RemoveAliasCMD(string[] args, Cmd.Callback cb)
        {
            if (!_aliases.ContainsKey(args[0]))
                throw new CommandException("Alias", $"Unknown alias command \'{args[0]}\'.");
            Commands.Remove(args[0]);
            _aliases.Remove(args[0]);
            cb.Launcher.FeedbackLine($"Sucessfully removed alias \'{args[0]}\'.");
        }
    }
}
