using CSUtils;
namespace SCE
{
    public class CombinePKG : Package
    {
        private readonly HashSet<string> _combines = new();

        public CombinePKG()
        {
            Name = "Combine";
            Version = new(0, 0, 0);
            Desc = "Helps create commands from other commands.";
            Commands = new()
            {
                { "combine", new(CombineCMD) { Min = 2, Max = -1,
                    Desc = "Combines multiple commands into one.",
                    Usage = "<NewCommandName> <Command1>..." } },

                { "combdel", new(DeleteCMD) { Min = 1, Max = -1,
                    Desc = "Deletes the given combine commands.",
                    Usage = "<*>:<Command1>..." } },
            };
        }

        #region Combine

        private void DeleteCMD(string[] args, CmdLauncher cl)
        {
            if (args[0] == "*")
            {
                if (_combines.Count == 0)
                    cl.FeedbackLine("No commands to remove.");
                else
                {
                    foreach (var comb in _combines)
                        Commands.Remove(comb);
                    cl.FeedbackLine($"Sucessfully removed {_combines.Count} command(s).");
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
                cl.FeedbackLine($"Sucessfully removed {args.Length} command(s).");
            }
        }

        private void CombineCMD(string[] args, CmdLauncher cl)
        {
            if (cl.ContainsCommand(args[0]))
                throw new CmdException("Combine", $"Command \'{args[0]}\' already exists.");
            var trim = Utils.TrimFirst(args);
            Commands.Add(args[0], new(_ => cl.ExecuteEveryCommand(trim)) { 
                Desc = $"Combined command of {trim.Length} line(s)." });
            _combines.Add(args[0]);
            cl.FeedbackLine($"Sucessfully made new command \'{args[0]}\'.");
        }

        #endregion
    }
}
