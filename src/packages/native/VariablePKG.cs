using CSUtils;
using System.Text;

namespace SCE
{
    internal class VariablePKG : Package
    {
        private readonly Dictionary<string, string> _variables = new();

        public VariablePKG()
        {
            Name = "Variable";
            Version = "0.0.0";
            Commands = new()
            {
                { "takefmem", new(StoreMemCMD) { MinArgs = 1, MaxArgs = -1, 
                    Description = "Stores the latest memory and removes it.",
                    Usage = "<VariableName1>..." } },

                { "ststore", new(StoreCMD) { MinArgs = 2, MaxArgs = 2,
                    Description = "Stores the data into a variable.",
                    Usage = "<VariableName> <Data>" } },

                { "stclear", new(ClearCMD) { 
                    Description = "Clears every variable" } },

                { "stdel", new(RemoveVariableCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Deletes the specified variable.",
                    Usage = "<VariableName1>..." } },

                { "stview", new(ViewVariableCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Displays all the specified variables.",
                    Usage = "<*>:<VariableName1>..." } },

                { "insert", new(InsertCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Inserts variables into the given command: $var_name$",
                    Usage = "<CommandName> <Arg1>..." } },
            };
        }

        private void ViewVariableCMD(string[] args, Cmd.Callback cb)
        {
            PatternUtils.ViewGeneric(_variables, args, Name, "variable");
        }

        private void StoreVariable(string name, string data, Cmd.Callback cb)
        {
            if (_variables.TryGetValue(name, out var oldValue))
                cb.Launcher.FeedbackLine($"{name}: {oldValue} -> {data}");
            else
                cb.Launcher.FeedbackLine($"{name} = {data}");
            _variables[name] = data;
        }

        private void StoreMemCMD(string[] args, Cmd.Callback cb)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                if (cb.Launcher.MemoryStack.Count == 0)
                    throw new CmdException("Variable", "Memory is empty.");
                var store = cb.Launcher.MemoryStack.Pop();
                if (store?.ToString() is string str)
                    StoreVariable(args[i], str, cb);
                else
                    throw new CmdException("Variable", "Stored value is null.");
            }
        }

        private void RemoveVariableCMD(string[] args, Cmd.Callback cb)
        {
            foreach (var name in args)
            {
                if (!_variables.ContainsKey(name))
                    throw new CmdException("Variable", $"Unknown variable \'{name}\'.");

                _variables.Remove(name);
                cb.Launcher.FeedbackLine($"Sucessfully removed  variable \'{name}\'.");
            }
        }

        private void ClearCMD(string[] args, Cmd.Callback cb)
        {
            if (_variables.Count == 0)
                cb.Launcher.FeedbackLine("No variables to clear.");
            else
            {
                cb.Launcher.FeedbackLine($"Sucessfully cleared {_variables.Count} variables.");
                _variables.Clear();
            }
        }

        private void StoreCMD(string[] args, Cmd.Callback cb)
        {
            StoreVariable(args[0], args[1], cb);
        }

        private string Replace(string str)
        {
            StringBuilder sb = new();
            for (int i = 0; i < str.Length; ++i)
            {
                int next = str.IndexOf('$', i + 1);
                if (str[i] != '$' || next == -1 || next == i + 1)
                    sb.Append(str[i]);
                else
                {
                    var name = str[(i + 1)..next];
                    if (!_variables.TryGetValue(name, out var val))
                        throw new CmdException("Variable", $"Unknown variable \'{name}\'.");
                    sb.Append(val);
                    i = next;
                }
            }
            return sb.ToString();
        }

        private void InsertCMD(string[] args, Cmd.Callback cb)
        {
            for (int i = 0; i < args.Length; ++i)
                args[i] = Replace(args[i]);     
            cb.Launcher.ExecuteCommand(Utils.Infill(args, " "));
        }
    }
}
