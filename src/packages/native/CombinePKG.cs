using CSUtils;
namespace SCE
{
    internal class CombinePKG : Package
    {
        private readonly HashSet<string> _combines = new();

        public CombinePKG()
        {
            Name = "Combine";
            Commands = new()
            {
                { "combine", new(CombineCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Combines multiple commands into one.",
                    Usage = "<NewCommandName> <Command1>..." } },

                { "combdel", new(DeleteCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Deletes the given combine commands.",
                    Usage = "<*>:<Command1>..." } },
            };
        }

        private void DeleteCMD(string[] args, Cmd.Callback cb)
        {
            if (args[0] == "*")
            {
                if (_combines.Count == 0)
                    cb.Launcher.FeedbackLine("No commands to remove.");
                else
                {
                    foreach (var comb in _combines)
                        Commands.Remove(comb);
                    cb.Launcher.FeedbackLine($"Sucessfully removed {_combines.Count} command(s).");
                    _combines.Clear();
                }
            }
            else
            {
                foreach (var arg in args)
                {
                    if (!_combines.Contains(arg))
                        throw new CmdException("Combine", $"Unknown command \'{arg}\'.");
                    Commands.Remove(arg);
                    _combines.Remove(arg);
                }
                cb.Launcher.FeedbackLine($"Sucessfully removed {args.Length} command(s).");
            }
        }

        private void CombineCMD(string[] args, Cmd.Callback cb)
        {
            if (cb.Launcher.CommandExists(args[0]))
                throw new CmdException("Combine", $"Command \'{args[0]}\' already exists.");
            var trim = Utils.TrimFirst(args);
            Commands.Add(args[0], new(_ => cb.Launcher.ExecuteEveryCommand(trim)) { 
                Description = $"Combined command of {trim.Length} line(s)." });
            _combines.Add(args[0]);
            cb.Launcher.FeedbackLine($"Sucessfully made new command \'{args[0]}\'.");
        }
    }
}
