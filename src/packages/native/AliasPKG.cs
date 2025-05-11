namespace SCE
{
    internal class AliasPKG : Package
    {
        private readonly Dictionary<string, string> _aliases = new();

        public AliasPKG()
        {
            Name = "Alias";
            Version = new(0, 0, 0);
            Desc = "Allows for the creation of alias commands for using prefered names.";
            Commands = new()
            {
                { "alias", new(AddAliasCMD) { Min = 1, Max = -1,
                    Desc = "Adds a list of command aliases.",
                    Usage = "<AliasName->CommandName>..." } },

                { "aliasdel", new(RemoveAliasCMD) { Min = 1, Max = 1,
                    Desc = "Removes the given command alias.",
                    Usage = "<AliasName>" } },

                { "aliasclear", new(ClearAliasCMD) { 
                    Desc = "Clears all command aliases." } },

                { "aliasview", new(ViewAliasCMD) { Max = -1,
                    Desc = "Displays the specified command aliases.",
                    Usage = "?<*>:<AliasName1>..." } },
            };
        }

        private void AddAliasCMD(string[] args, CmdLauncher cl)
        {
            foreach (var alias in args)
            {
                var split = alias.Split("->");
                if (split.Length != 2)
                    throw new CmdException("Alias", $"Invalid alias \'{alias}\'.");
                if (!cl.TryGetCommand(split[1], out var command))
                    throw new CmdException("Alias", $"Unknown command \'{split[1]}\'.");
                if (cl.ContainsCommand(split[0]))
                    throw new CmdException("Alias", $"Command with name \'{split[0]}\' already exists.");
                Commands.Add(split[0], command);
                _aliases[split[0]] = split[1];
                cl.FeedbackLine($"\'{split[0]}\' -> \'{split[1]}\'");
            }
        }

        private void RemoveAliasCMD(string[] args, CmdLauncher cl)
        {
            if (!_aliases.ContainsKey(args[0]))
                throw new CmdException("Alias", $"Unknown alias command \'{args[0]}\'.");
            Commands.Remove(args[0]);
            _aliases.Remove(args[0]);
            cl.FeedbackLine($"Sucessfully removed alias \'{args[0]}\'.");
        }

        private void ClearAliasCMD(string[] args, CmdLauncher cl)
        {
            if (_aliases.Count == 0)
                cl.FeedbackLine($"No items to clear.");
            else
            {
                cl.FeedbackLine($"Successfully cleared {_aliases.Count} aliases.");
                _aliases.Clear();
            }
        }

        private void ViewAliasCMD(string[] args)
        {
            PatternUtils.ViewGeneric(_aliases, args, Name, "alias");
        }
    }
}
