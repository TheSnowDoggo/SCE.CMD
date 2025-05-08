using CSUtils;
using System.Text;

namespace SCE
{
    internal class DefinePKG : Package
    {
        private static readonly DefinePRP _nativedefines = new(-1)
        {
            AutoRemoveIgnore = false,
            Defines = new()
            {
                { "#define", () => "@IGNORE#define" },
                { "#func", () => "@IGNORE#func" },
                { "#action", () => "@IGNORE#action" },
                { "#undefine", () => "@IGNORE#undefine" },
            },
        };

        private readonly DefinePRP _userdefines = new(0);

        private readonly Dictionary<string, string> _defineview = new();

        public DefinePKG()
        {
            Name = "Define";
            Version = "0.2.1";
            Commands = new()
            {
                { "#define", new(DefineCMD) { MinArgs = 2, MaxArgs = 2,
                    Description = "Creates a new #define.",
                    Usage = "<DefineName> <Replace>" } },

                { "#func", new(FuncCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Creates a new #func.",
                    Usage = "<FuncName> <CommandName> ?<Arg1>..." } },

                { "#action", new(ActionCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Creates a new #action.",
                    Usage = "<ActionName> <CommandName> ?<Arg1>..." } },

                { "#undefine", new(UndefineCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Removes a given define.",
                    Usage = "<DefineName>" } },

                { "#cleardefines", new(ClearDefinesCMD) {
                    Description = "Removes every define." } },

                { "#viewdefines", new(ViewDefinesCMD) {
                    Description = "Views every define." } },

                { "#prep", new(PrepCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Preprocesses the following command." } },
            };
        }

        public override void Initialize(CmdLauncher launcher)
        {
            launcher.Preprocessors.Add(_nativedefines);
            launcher.Preprocessors.Add(_userdefines);
        }

        private bool VerifyName(string name)
        {
            if (name.Length == 0)
                throw new CmdException("Define", "Defined name cannot be empty.");
            return !_userdefines.Defines.ContainsKey(name) || Utils.BoolPrompt($"Define with name \'{name}\' already exists\n" +
                $"Do you want to overwrite it? Yes[Y] or No[N]: ");
        }

        private void DefineCMD(string[] args, Cmd.Callback cb)
        {
            if (!VerifyName(args[0]))
            {
                cb.Launcher.FeedbackLine("Define canceled.");
                return;
            }
            _userdefines.Defines[args[0]] = () => args[1];
            _defineview[args[0]] = args[1];
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
            _userdefines.Defines[args[0]] = () =>
            {
                cb.Launcher.ExecuteCommand(args[1], newArgs);
                if (cb.Launcher.MemoryStack.Count == 0)
                    throw new CmdException("Define", $"#func \'{args[0]}\' call failed | Memory stack empty.");
                var obj = cb.Launcher.MemoryStack.Pop() ??
                    throw new CmdException("Define", $"#func \'{args[0]}\' call failed | Memory item was null.");
                return obj.ToString() ??
                    throw new CmdException("Define", $"#func \'{args[0]}\' call failed | Memory item was null.");
            };
            _defineview[args[0]] = args[1] + Utils.Infill(newArgs, " ");
            cb.Launcher.FeedbackLine("#func created successfully.");
        }

        private void ActionCMD(string[] args, Cmd.Callback cb)
        {
            if (!cb.Launcher.CommandExists(args[1]))
                throw new CmdException("Define", $"Unknown command \'{args[1]}\'.");
            if (!VerifyName(args[0]))
            {
                cb.Launcher.FeedbackLine("#action creation canceled.");
                return;
            }
            var newArgs = Utils.TrimFromStart(args, 2);
            _userdefines.Defines[args[0]] = () =>
            {
                cb.Launcher.ExecuteCommand(args[1], newArgs);
                return "";
            };
            _defineview[args[0]] = args[1] + Utils.Infill(newArgs, " ");
            cb.Launcher.FeedbackLine("#action created successfully.");
        }

        private void UndefineCMD(string[] args, Cmd.Callback cb)
        {
            if (!_userdefines.Defines.ContainsKey(args[0]))
                throw new CmdException("Define", $"No define with name \'{args[0]}\' found.");
            _userdefines.Defines.Remove(args[0]);
            _defineview.Remove(args[0]);
            cb.Launcher.FeedbackLine("#define removed successfully.");
        }

        private void ClearDefinesCMD(string[] args, Cmd.Callback cb)
        {
            int count = _userdefines.Defines.Count;
            if (count == 0)
                throw new CmdException("Define", "No defines to remove.");
            if (!Utils.BoolPrompt($"Are you sure you want to remove {count} define(s)?\n" +
                $"Yes[Y] or No[N]: "))
            {
                cb.Launcher.FeedbackLine("Define clear canceled.");
                return;
            }
            _userdefines.Defines.Clear();
            _defineview.Clear();
            cb.Launcher.FeedbackLine($"Successfully removed {count} define(s)");
        }

        private void ViewDefinesCMD(string[] args)
        {
            StringBuilder sb = new();
            foreach (var def in _userdefines.Defines)
                sb.AppendLine($"{def.Key} = {_defineview[def.Key]}");
            Console.Write(sb.ToString());
        }

        private static void PrepCMD(string[] args, Cmd.Callback cb)
        {
            var name = cb.Launcher.Process(args[0]);
            var newArgs = Utils.TrimFirst(args);
            for (int i = 0; i < newArgs.Length; ++i)
                newArgs[i] = cb.Launcher.Process(newArgs[i]);
            cb.Launcher.ExecuteCommand(name, newArgs);
        }
    }
}
