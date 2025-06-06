﻿using CSUtils;
using System.Text;
namespace SCE
{
    public class DefinePKG : Package
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
            Version = new(0, 2, 1);
            Desc = "Macro preprocessor package.";
            Commands = new()
            {
                { "#define", new(DefineCMD) { Min = 2, Max = 2,
                    Desc = "Creates a new #define.",
                    Usage = "<DefineName> <Replace>" } },

                { "#func", new(FuncCMD) { Min = 2, Max = -1,
                    Desc = "Creates a new #func.",
                    Usage = "<FuncName> " + Cmd.BCHAIN } },

                { "#action", new(ActionCMD) { Min = 2, Max = -1,
                    Desc = "Creates a new #action.",
                    Usage = "<ActionName> " + Cmd.BCHAIN } },

                { "#undefine", new(UndefineCMD) { Min = 1, Max = 1,
                    Desc = "Removes a given define.",
                    Usage = "<DefineName>" } },

                { "#cleardefines", new(ClearDefinesCMD) {
                    Desc = "Removes every define." } },

                { "#viewdefines", new(ViewDefinesCMD) {
                    Desc = "Views every define." } },

                { "#prep", new(PrepCMD) { Min = 1, Max = -1,
                    Desc = "Preprocesses the following command." } },
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

        private void DefineCMD(string[] args, CmdLauncher cl)
        {
            if (!VerifyName(args[0]))
            {
                cl.FeedbackLine("Define canceled.");
                return;
            }
            _userdefines.Defines[args[0]] = () => args[1];
            _defineview[args[0]] = args[1];
            cl.FeedbackLine("#define created successfully.");
        }

        private void FuncCMD(string[] args, CmdLauncher cl)
        {
            if (!cl.ContainsCommand(args[1]))
                throw new CmdException("Define", $"Unknown command \'{args[1]}\'.");
            if (!VerifyName(args[0]))
            {
                cl.FeedbackLine("#func creation canceled.");
                return;
            }
            var newArgs = Utils.TrimFromStart(args, 2);
            _userdefines.Defines[args[0]] = () =>
            {
                cl.ExecuteCommand(args[1], newArgs);
                if (cl.MemoryStack.Count == 0)
                    throw new CmdException("Define", $"#func \'{args[0]}\' call failed | Memory stack empty.");
                var obj = cl.MemoryStack.Pop() ??
                    throw new CmdException("Define", $"#func \'{args[0]}\' call failed | Memory item was null.");
                return obj.ToString() ??
                    throw new CmdException("Define", $"#func \'{args[0]}\' call failed | Memory item was null.");
            };
            _defineview[args[0]] = args[1] + Utils.Infill(newArgs, " ");
            cl.FeedbackLine("#func created successfully.");
        }

        private void ActionCMD(string[] args, CmdLauncher cl)
        {
            if (!cl.ContainsCommand(args[1]))
                throw new CmdException("Define", $"Unknown command \'{args[1]}\'.");
            if (!VerifyName(args[0]))
            {
                cl.FeedbackLine("#action creation canceled.");
                return;
            }
            var newArgs = Utils.TrimFromStart(args, 2);
            _userdefines.Defines[args[0]] = () =>
            {
                cl.ExecuteCommand(args[1], newArgs);
                return "";
            };
            _defineview[args[0]] = args[1] + Utils.Infill(newArgs, " ");
            cl.FeedbackLine("#action created successfully.");
        }

        private void UndefineCMD(string[] args, CmdLauncher cl)
        {
            if (!_userdefines.Defines.ContainsKey(args[0]))
                throw new CmdException("Define", $"No define with name \'{args[0]}\' found.");
            _userdefines.Defines.Remove(args[0]);
            _defineview.Remove(args[0]);
            cl.FeedbackLine("#define removed successfully.");
        }

        private void ClearDefinesCMD(string[] args, CmdLauncher cl)
        {
            int count = _userdefines.Defines.Count;
            if (count == 0)
                throw new CmdException("Define", "No defines to remove.");
            if (!Utils.BoolPrompt($"Are you sure you want to remove {count} define(s)?\n" +
                $"Yes[Y] or No[N]: "))
            {
                cl.FeedbackLine("Define clear canceled.");
                return;
            }
            _userdefines.Defines.Clear();
            _defineview.Clear();
            cl.FeedbackLine($"Successfully removed {count} define(s)");
        }

        private void ViewDefinesCMD(string[] args)
        {
            StringBuilder sb = new();
            foreach (var def in _userdefines.Defines)
                sb.AppendLine($"{def.Key} = {_defineview[def.Key]}");
            Console.Write(sb.ToString());
        }

        private static void PrepCMD(string[] args, CmdLauncher cl)
        {
            var name = cl.PreProcess(args[0]);
            var newArgs = Utils.TrimFirst(args);
            for (int i = 0; i < newArgs.Length; ++i)
                newArgs[i] = cl.PreProcess(newArgs[i]);
            cl.ExecuteCommand(name, newArgs);
        }
    }
}
