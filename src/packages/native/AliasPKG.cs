namespace SCE
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
                    Description = "Adds a list of command aliases.",
                    Usage = "<AliasName->CommandName>..." } },

                { "aliasdel", new(RemoveAliasCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Removes the given command alias.",
                    Usage = "<AliasName>" } },

                { "aliasclear", new(ClearAliasCMD) { 
                    Description = "Clears all command aliases." } },

                { "aliasview", new(ViewAliasCMD) { MaxArgs = -1,
                    Description = "Displays the specified command aliases.",
                    Usage = "?<*>:<AliasName1>..." } },
            };
        }

        private void AddAliasCMD(string[] args, Cmd.Callback cb)
        {
            foreach (var alias in args)
            {
                var split = alias.Split("->");
                if (split.Length != 2)
                    throw new CmdException("Alias", $"Invalid alias \'{alias}\'.");
                if (!cb.Launcher.TryGetCommand(split[1], out var command))
                    throw new CmdException("Alias", $"Unknown command \'{split[1]}\'.");
                if (cb.Launcher.CommandExists(split[0]))
                    throw new CmdException("Alias", $"Command with name \'{split[0]}\' already exists.");
                Commands.Add(split[0], command);
                _aliases[split[0]] = split[1];
                cb.Launcher.FeedbackLine($"\'{split[0]}\' -> \'{split[1]}\'");
            }
        }

        private void RemoveAliasCMD(string[] args, Cmd.Callback cb)
        {
            if (!_aliases.ContainsKey(args[0]))
                throw new CmdException("Alias", $"Unknown alias command \'{args[0]}\'.");
            Commands.Remove(args[0]);
            _aliases.Remove(args[0]);
            cb.Launcher.FeedbackLine($"Sucessfully removed alias \'{args[0]}\'.");
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
            PatternUtils.ViewGeneric(_aliases, args, Name, "alias");
        }
    }
}
