using CSUtils;
using System.Text;

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
                    Description = "Creates a new #define.",
                    Usage = "<DefineName> <Replace>" } },

                { "#func", new(FuncCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Creates a new #func.",
                    Usage = "<FuncName> <CommandName> ?<Arg1>..." } },

                { "#undefine", new(UndefineCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Removes a given #define or #func.",
                    Usage = "<DefineName>" } },

                { "#cleardefines", new(ClearDefinesCMD) {
                    Description = "Removes every #define and #func." } },

                { "#viewdefines", new(ViewDefinesCMD) {
                    Description = "Views every #define and #func." } },
            };
        }

        public override void Initialize(CmdLauncher launcher)
        {
            launcher.Preprocessors.Add(_definePRP);
        }

        private bool VerifyName(string name)
        {
            if (name.Length == 0)
                throw new CmdException("Define", "Defined name cannot be empty.");
            return !_definePRP.Defines.ContainsKey(name) || Utils.BoolPrompt($"Define with name \'{name}\' already exists\n" +
                $"Do you want to overwrite it? Yes[Y] or No[N]: ");
        }

        private void DefineCMD(string[] args, Cmd.Callback cb)
        {
            if (!VerifyName(args[0]))
            {
                cb.Launcher.FeedbackLine("Define canceled.");
                return;
            }
            _definePRP.Defines[args[0]] = () => args[1];
            cb.Launcher.FeedbackLine("#define created successfully.");
        }

        private void FuncCMD(string[] args, Cmd.Callback cb)
        {
            if (!cb.Launcher.CommandExists(args[1]))
                throw new CmdException("Define", $"Unknown command \'{args[1]}\'.");
            if (!VerifyName(args[0]))
            {
                cb.Launcher.FeedbackLine("#func creation canceled.");
                return;
            }
            var newArgs = Utils.TrimFromStart(args, 2);
            _definePRP.Defines[args[0]] = () =>
            {
                cb.Launcher.ExecuteCommand(args[1], newArgs);
                if (cb.Launcher.MemoryStack.Count == 0)
                    throw new CmdException("Define", $"#func \'{args[0]}\' call failed | Memory stack empty.");
                var obj = cb.Launcher.MemoryStack.Pop() ??
                    throw new CmdException("Define", $"#func \'{args[0]}\' call failed | Memory item was null.");
                return obj.ToString() ??
                    throw new CmdException("Define", $"#func \'{args[0]}\' call failed | Memory item was null.");
            };
            cb.Launcher.FeedbackLine("#func created successfully.");
        }

        private void UndefineCMD(string[] args, Cmd.Callback cb)
        {
            if (!_definePRP.Defines.ContainsKey(args[0]))
                throw new CmdException("Define", $"No define with name \'{args[0]}\' found.");
            _definePRP.Defines.Remove(args[0]);
            cb.Launcher.FeedbackLine("#define removed successfully.");
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
            cb.Launcher.FeedbackLine($"Successfully removed {count} #define(s)");
        }

        private void ViewDefinesCMD(string[] args)
        {
            StringBuilder sb = new();
            foreach (var def in _definePRP.Defines)
                sb.AppendLine($"{def.Key} = {def.Value}");
            Console.Write(sb.ToString());
        }
    }
}
