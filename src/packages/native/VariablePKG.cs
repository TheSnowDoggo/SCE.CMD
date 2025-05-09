using CSUtils;
using System.Text;
namespace SCE
{
    internal class VariablePKG : Package
    {
        private readonly Stack<Dictionary<string, string>> _scopes = new();

        private readonly Dictionary<string, string> _globalScope = new();

        private readonly Stack<int> _tempScopes = new();

        private bool global = false;

        private bool tempGlobal = false;

        public VariablePKG()
        {
            Name = "Variable";
            Version = "1.2.0";
            Commands = new()
            {
                { ">scope", new(EnterScopeCMD) {
                    Description = "Enters a variable scope." } },

                { "<scope", new(ExitScopeCMD) {
                    Description = "Exits a variable scope." } },

                { "<*scope", new(ExitAllScopeCMD) {
                    Description = "Exits every variable scope." } },

                { "/scope", new(TempScopeCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Safely enters a scope for the duration of the command.",
                    Usage = "<CommandName> ?<Arg1>..." } },

                { "?scope", new(GetScopeCMD) {
                    Description = "Outputs the current scope." } },

                { ">global", new(EnterGlobalCMD) {
                    Description = "Enters global scope." } },

                { "<global", new(ExitGlobalCMD) {
                    Description = "Exits global scope." } },

                { "?global", new(GetGlobalCMD) {
                    Description = "Outputs whether global scope is entered." } },

                { "/global", new(TempGlobalCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Safely enters global scope for the duration of the command.",
                    Usage = "<CommandName> ?<Arg1>..." } },

                { "ststore", new(StoreCMD) { MinArgs = 2, MaxArgs = 2,
                    Description = "Stores the data into a variable.",
                    Usage = "<VariableName> <Data>" } },

                { "ststoreres", new(StoreResCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Stores the latest memory item into the variable after running the given command.",
                    Usage = "<VariableName> <CommandName> ?<Arg1>..." } },

                { "tfm", new(TakeFMemCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Stores the latest items in memory stack and removes it.",
                    Usage = "<VariableName1>..." } },

                { "ins", new(InsertCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Inserts variables into the given command: $var_name$",
                    Usage = "<CommandName> <Arg1>..." } },

                { "insl", new(InsertLimCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Inserts a select number of variables into the given command: $var_name$",
                    Usage = "<VariableCount> <CommandName> <Arg1>..." } },

                { "stmem", new(StoreMemCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Stores the variables into the memory stack.",
                    Usage = "<VariableName1>..." } },

                { "stmem^", new(StoreMemRCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Stores the variables into the memory stack and removes them.",
                    Usage = "<VariableName1>..." } },

                { "stclear", new(ClearCMD) {
                    Description = "Clears every variable in the current scope." } },

                { "stdel", new(RemoveVariableCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Deletes the specified variable.",
                    Usage = "<VariableName1>..." } },

                { "stview", new(ViewVariableCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Displays all the specified variables.",
                    Usage = "<*>:<VariableName1>..." } },

                { "stcleanup", new(CleanupCMD) }
            };
        }

        private Dictionary<string, string> GetScope()
        {
            if (global)
                return _globalScope;
            if (_scopes.Count == 0)
                throw new CmdException("Variable", "No scopes defined.");
            return _scopes.Peek();
        }

        private void StoreVariable(string name, string data, Cmd.Callback cb)
        {
            var scope = GetScope();
            if (scope.TryGetValue(name, out var oldValue))
                cb.Launcher.FeedbackLine($"{name}: {oldValue} -> {data}");
            else
                cb.Launcher.FeedbackLine($"{name} = {data}");
            scope[name] = data;
        }

        private string GetVariable(string name, out Dictionary<string, string> scope)
        {
            scope = GetScope();
            if (scope.TryGetValue(name, out var data))
                return data;
            else if (!global && _globalScope.TryGetValue(name, out data))
                return data;
            throw new CmdException("Variable", $"Unknown variable \'{name}\' | Scope = {(global ? "global" : _scopes.Count)}.");
        }

        private string GetVariable(string name)
        {
            return GetVariable(name, out _);
        }

        private string[] Replace(string[] arr, int lim = -1)
        {
            int count = 0;
            for (int i = 0; i < arr.Length; ++i)
            {
                StringBuilder sb = new();
                var str = arr[i];
                for (int j = 0; j < str.Length; ++j)
                {
                    int next = str.IndexOf('$', j + 1);
                    if (str[j] != '$' || next == -1 || next == j + 1)
                        sb.Append(str[j]);
                    else
                    {
                        var name = str[(j + 1)..next];
                        sb.Append(GetVariable(name));
                        j = next;
                        ++count;
                        if (lim >= 0 && count >= lim)
                            break;
                    }
                }
                arr[i] = sb.ToString();
                sb.Clear();
            }
            return arr;
        }

        private void EnterGlobalCMD(string[] args)
        {
            if (global)
                throw new CmdException("Variable", "Cannot re-enter global scope.");
            global = true;
        }

        private void ExitGlobalCMD(string[] args)
        {
            if (!global)
                throw new CmdException("Variable", "Cannot re-exit global scope.");
            if (tempGlobal)
                throw new CmdException("Variable", "Cannot exit global scope as exiting a temp scope is disallowed.");
            global = false;
        }

        private Cmd.MemItem GetGlobalCMD(string[] args, Cmd.Callback cb)
        {
            cb.Launcher.FeedbackLine($"Global? {global}");
            return new(global);
        }

        private void TempGlobalCMD(string[] args, Cmd.Callback cb)
        {
            if (global)
                throw new CmdException("Variable", "Cannot re-enter global scope.");
            global = true;
            tempGlobal = true;
            cb.Launcher.SExecuteCommand(args[0], Utils.TrimFirst(args));
            global = false;
            tempGlobal = false;
        }

        private void EnterScopeCMD(string[] args)
        {
            if (global)
                throw new CmdException("Variable", "Cannot enter scope in global (run <global to exit)");
            _scopes.Push(new());
        }

        private void ExitScopeCMD(string[] args)
        {
            if (global)
                throw new CmdException("Variable", "Cannot exit scope in global (run <global to exit)");
            if (_scopes.Count == 0)
                throw new CmdException("Variable", "No scopes defined.");
            if (_tempScopes.Count > 0 && _scopes.Count == _tempScopes.Peek())
                throw new CmdException("Variable", "Cannot exit scope as exiting a temp scope is disallowed.");
            _scopes.Pop();
        }

        private void ExitAllScopeCMD(string[] args)
        {
            if (_scopes.Count == 0)
                throw new CmdException("Variable", "No scopes defined.");
            if (_tempScopes.Count != 0)
                throw new CmdException("Variable", "Cannot exit all scopes as one or more temp scope(s) are defined.");
            _scopes.Clear();
        }

        private void TempScopeCMD(string[] args, Cmd.Callback cb)
        {
            _scopes.Push(new());
            _tempScopes.Push(_scopes.Count);
            cb.Launcher.SExecuteCommand(args[0], Utils.TrimFirst(args));
            if (_scopes.Count == 0 || _tempScopes.Count == 0)
                throw new CmdException("Variable", "Critical temp scope error.");
            _scopes.Pop();
            _tempScopes.Pop();
        }

        private Cmd.MemItem GetScopeCMD(string[] args, Cmd.Callback cb)
        {
            cb.Launcher.FeedbackLine($"Scope = {_scopes.Count}");
            return new(_scopes.Count);
        }

        private void ViewVariableCMD(string[] args, Cmd.Callback cb)
        {
            var scope = GetScope();
            Console.WriteLine(global ? "- Global -" : $"- Scope {_scopes.Count} -");
            PatternUtils.ViewGeneric(scope, args, Name, "variable");
        }

        private void StoreCMD(string[] args, Cmd.Callback cb)
        {
            StoreVariable(args[0], args[1], cb);
        }

        private void StoreResCMD(string[] args, Cmd.Callback cb)
        {
            cb.Launcher.ExecuteCommand(args[1], Utils.TrimFromStart(args, 2));
            if (cb.Launcher.MemoryStack.Count == 0)
                throw new CmdException("Variable", "No items in memory to store.");
            var obj = cb.Launcher.MemoryStack.Pop() ??
                throw new CmdException("Variable", "Memory object was null.");
            var str = obj.ToString() ??
                throw new CmdException("Variable", "Memory string conversion was null.");
            StoreVariable(args[0], str, cb);
        }

        private void TakeFMemCMD(string[] args, Cmd.Callback cb)
        {
            GetScope();
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

        private void InsertCMD(string[] args, Cmd.Callback cb)
        {
            args = Replace(args);
            cb.Launcher.ExecuteCommand(args[0], Utils.TrimFirst(args));
        }

        private void InsertLimCMD(string[] args, Cmd.Callback cb)
        {
            int lim = int.Parse(args[0]);
            args = Replace(args, lim);
            cb.Launcher.ExecuteCommand(args[1], Utils.TrimFromStart(args, 2));
        }

        private Cmd.MemItem StoreMemCMD(string[] args)
        {
            return new(GetVariable(args[0]));
        }

        private Cmd.MemItem StoreMemRCMD(string[] args)
        {
            var data = GetVariable(args[0], out var scope);
            if (!scope.ContainsKey(args[0]))
                throw new CmdException("Variable", $"Unknown variable \'{args[0]}\'.");
            scope.Remove(args[0]);
            return new(data);
        }

        private void RemoveVariableCMD(string[] args, Cmd.Callback cb)
        {
            var scope = GetScope();
            foreach (var name in args)
            {
                if (!scope.ContainsKey(name))
                    throw new CmdException("Variable", $"Unknown variable \'{name}\'.");
                scope.Remove(name);
                cb.Launcher.FeedbackLine($"Sucessfully removed variable \'{name}\'.");
            }
        }

        private void ClearCMD(string[] args, Cmd.Callback cb)
        {
            if (_scopes.Count == 0)
                cb.Launcher.FeedbackLine("No variables to clear.");
            else
            {
                cb.Launcher.FeedbackLine($"Sucessfully cleared {_scopes.Count} variables.");
                _scopes.Clear();
            }
        }

        private void CleanupCMD(string[] args, Cmd.Callback cb)
        {
            bool skip = false;
            if (args.Length > 0 && !bool.TryParse(args[0], out skip))
                throw new CmdException("Variable", $"Cannot convert \'{args[0]}\' to bool.");
            if (!skip && !Utils.BoolPrompt("Cleaning up will reset all scopes and variables and may cause errors in command chains.\n" +
                "Are you sure? Yes[Y] or No[N]: "))
            {
                cb.Launcher.FeedbackLine("Cleanup canceled.");
                return;
            }
            _scopes.Clear();
            _tempScopes.Clear();
            _globalScope.Clear();
            global = false;
            tempGlobal = false;
            cb.Launcher.FeedbackLine("Cleanup successfull.");
        }
    }
}
