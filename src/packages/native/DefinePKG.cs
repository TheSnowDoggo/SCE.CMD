using CSUtils;

namespace SCE
{
    internal class DefinePKG : Package
    {
        private readonly DefinePRP _definePRP = new(0);

        public DefinePKG()
        {
            Name = "Define";
            Version = "0.0.0";
            Commands = new()
            {
                { "#define", new(DefineCMD) { MinArgs = 2, MaxArgs = 2,
                    Description = "Creates a new #define preprocessor.",
                    Usage = "<DefineName> <Replace>" } },

                { "#undefine", new(UndefineCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Removes a #define preprocessor.",
                    Usage = "<DefineName>" } },

                { "#cleardefines", new(ClearDefinesCMD) {
                    Description = "Removes every #define preprocessor." } },
            };
        }

        public override void Initialize(CmdLauncher launcher)
        {
            launcher.Preprocessors.Add(_definePRP);
        }

        private void DefineCMD(string[] args, Cmd.Callback cb)
        {
            if (args[0].Length == 0)
                throw new CmdException("Define", "Defined name cannot be empty.");
            if (_definePRP.Defines.ContainsKey(args[0]) && !Utils.BoolPrompt($"Define with name \'{args[0]}\' already exists\n" +
                $"Do you want to overwrite it? Yes[Y] or No[N]: "))
            {
                cb.Launcher.FeedbackLine("Define canceled.");
                return;
            }
            _definePRP.Defines[args[0]] = args[1];
            cb.Launcher.FeedbackLine("Define created successfully.");
        }

        private void UndefineCMD(string[] args, Cmd.Callback cb)
        {
            if (!_definePRP.Defines.ContainsKey(args[0]))
                throw new CmdException("Define", $"No define with name \'{args[0]}\' found.");
            _definePRP.Defines.Remove(args[0]);
            cb.Launcher.FeedbackLine("Define removed successfully.");
        }

        private void ClearDefinesCMD(string[] args, Cmd.Callback cb)
        {
            int count = _definePRP.Defines.Count;
            if (count == 0)
                throw new CmdException("Define", "No defines to remove.");
            if (!Utils.BoolPrompt($"Are you sure you want to remove {count} define(s)?\n" +
                $"Yes[Y] or No[N]: "))
            {
                return;
            }
            _definePRP.Defines.Clear();
            cb.Launcher.FeedbackLine($"Successfully removed {count} define(s)");
        }
    }
}
