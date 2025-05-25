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
            Version = new(1, 2, 0);
            Desc = "Safe storing and inserting of data in named variables.";
            Commands = new()
            {
                { "ststore", new(StoreCMD) { Min = 2, Max = 2,
                    Desc = "Stores the data into a variable.",
                    Usage = "<VariableName> <Data>" } },

                { "ststoreres", new(StoreResCMD) { Min = 2, Max = -1,
                    Desc = "Stores the latest memory item into the variable after running the given command.",
                    Usage = "<VariableName> " + Cmd.BCHAIN } },

                { "tfm", new(TakeFMemCMD) { Min = 1, Max = -1,
                    Desc = "Stores the latest items in memory stack and removes it.",
                    Usage = "<VariableName1>..." } },

                { "ins", new(InsertCMD) { Min = 2, Max = -1,
                    Desc = "Inserts variables into the given command: $var_name$",
                    Usage = Cmd.BCHAIN } },

                { "insl", new(InsertLimCMD) { Min = 2, Max = -1,
                    Desc = "Inserts a select number of variables into the given command: $var_name$",
                    Usage = "<VariableCount> " + Cmd.BCHAIN } },

                { "stmem", new(StoreMemCMD) { Min = 1, Max = -1,
                    Desc = "Stores the variables into the memory stack.",
                    Usage = "<VariableName1>..." } },

                { "stmem^", new(StoreMemRCMD) { Min = 1, Max = -1,
                    Desc = "Stores the variables into the memory stack and removes them.",
                    Usage = "<VariableName1>..." } },

                { "stclear", new(ClearCMD) {
                    Desc = "Clears every variable in the current scope." } },

                { "stdel", new(RemoveVariableCMD) { Min = 1, Max = -1,
                    Desc = "Deletes the specified variable.",
                    Usage = "<VariableName1>..." } },

                { "stview", new(ViewVariableCMD) { Min = 1, Max = -1,
                    Desc = "Displays all the specified variables.",
                    Usage = "<*>:<VariableName1>..." } },

                { "stcleanup", new(CleanupCMD) },

                #region Scope

                { ">scope", new(EnterScopeCMD) {
                    Desc = "Enters a variable scope." } },

                { "<scope", new(ExitScopeCMD) {
                    Desc = "Exits a variable scope." } },

                { "<*scope", new(ExitAllScopeCMD) {
                    Desc = "Exits every variable scope." } },

                { "/scope", new(TempScopeCMD) { Min = 1, Max = -1,
                    Desc = "Safely enters a scope for the duration of the command.",
                    Usage = Cmd.BCHAIN } },

                { "?scope", new(GetScopeCMD) {
                    Desc = "Outputs the current scope." } },

                { ">global", new(EnterGlobalCMD) {
                    Desc = "Enters global scope." } },

                { "<global", new(ExitGlobalCMD) {
                    Desc = "Exits global scope." } },

                { "?global", new(GetGlobalCMD) {
                    Desc = "Outputs whether global scope is entered." } },

                { "/global", new(TempGlobalCMD) { Min = 1, Max = -1,
                    Desc = "Safely enters global scope for the duration of the command.",
                    Usage = Cmd.BCHAIN } },

                #endregion
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

        private void StoreVariable(string name, string data, CmdLauncher cl)
        {
            var scope = GetScope();
            if (scope.TryGetValue(name, out var oldValue))
                cl.FeedbackLine($"{name}: {oldValue} -> {data}");
            else
                cl.FeedbackLine($"{name} = {data}");
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
                        {
                            sb.Append(str[(j + 1)..]);
                            arr[i] = sb.ToString();
                            return arr;
                        }
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

        private Cmd.MItem GetGlobalCMD(string[] args, CmdLauncher cl)
        {
            cl.FeedbackLine($"Global? {global}");
            return new(global);
        }

        private void TempGlobalCMD(string[] args, CmdLauncher cl)
        {
            if (global)
                throw new CmdException("Variable", "Cannot re-enter global scope.");
            global = true;
            tempGlobal = true;
            cl.SExecuteCommand(args[0], Utils.TrimFirst(args));
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

        private void TempScopeCMD(string[] args, CmdLauncher cl)
        {
            _scopes.Push(new());
            int ts = _scopes.Count;
            _tempScopes.Push(ts);
            cl.SExecuteCommand(args[0], Utils.TrimFirst(args));
            if (_scopes.Count == 0 || _tempScopes.Count == 0)
                throw new CmdException("Variable", "Critical temp scope error.");
            while (_scopes.Count >= ts)
            {
                if (_scopes.Count == _tempScopes.Peek())
                {
                    _tempScopes.Pop();
                }
                _scopes.Pop();
            }
        }

        private Cmd.MItem GetScopeCMD(string[] args, CmdLauncher cl)
        {
            cl.FeedbackLine($"Scope = {_scopes.Count}");
            return new(_scopes.Count);
        }

        private void ViewVariableCMD(string[] args, CmdLauncher cl)
        {
            var scope = GetScope();
            Console.WriteLine(global ? "- Global -" : $"- Scope {_scopes.Count} -");
            PatternUtils.ViewGeneric(scope, args, Name, "variable");
        }

        private void StoreCMD(string[] args, CmdLauncher cl)
        {
            StoreVariable(args[0], args[1], cl);
        }

        private void StoreResCMD(string[] args, CmdLauncher cl)
        {
            cl.ExecuteCommand(args[1], Utils.TrimFromStart(args, 2));
            if (cl.MemoryStack.Count == 0)
                throw new CmdException("Variable", "No items in memory to store.");
            var obj = cl.MemoryStack.Pop() ??
                throw new CmdException("Variable", "Memory object was null.");
            var str = obj.ToString() ??
                throw new CmdException("Variable", "Memory string conversion was null.");
            StoreVariable(args[0], str, cl);
        }

        private void TakeFMemCMD(string[] args, CmdLauncher cl)
        {
            GetScope();
            for (int i = 0; i < args.Length; ++i)
            {
                if (cl.MemoryStack.Count == 0)
                    throw new CmdException("Variable", "Memory is empty.");
                var store = cl.MemoryStack.Pop();
                if (store?.ToString() is not string str)
                    throw new CmdException("Variable", "Stored value is null.");
                StoreVariable(args[i], str, cl);
            }
        }

        private void InsertCMD(string[] args, CmdLauncher cl)
        {
            args = Replace(args);
            cl.ExecuteCommand(args[0], Utils.TrimFirst(args));
        }

        private void InsertLimCMD(string[] args, CmdLauncher cl)
        {
            int lim = int.Parse(args[0]);
            args = Replace(args, lim);
            cl.ExecuteCommand(args[1], Utils.TrimFromStart(args, 2));
        }

        private Cmd.MItem StoreMemCMD(string[] args)
        {
            return new(GetVariable(args[0]));
        }

        private Cmd.MItem StoreMemRCMD(string[] args)
        {
            var data = GetVariable(args[0], out var scope);
            if (!scope.ContainsKey(args[0]))
                throw new CmdException("Variable", $"Unknown variable \'{args[0]}\'.");
            scope.Remove(args[0]);
            return new(data);
        }

        private void RemoveVariableCMD(string[] args, CmdLauncher cl)
        {
            var scope = GetScope();
            foreach (var name in args)
            {
                if (!scope.ContainsKey(name))
                    throw new CmdException("Variable", $"Unknown variable \'{name}\'.");
                scope.Remove(name);
                cl.FeedbackLine($"Sucessfully removed variable \'{name}\'.");
            }
        }

        private void ClearCMD(string[] args, CmdLauncher cl)
        {
            if (_scopes.Count == 0)
                cl.FeedbackLine("No variables to clear.");
            else
            {
                cl.FeedbackLine($"Sucessfully cleared {_scopes.Count} variables.");
                _scopes.Clear();
            }
        }

        private void CleanupCMD(string[] args, CmdLauncher cl)
        {
            bool skip = false;
            if (args.Length > 0 && !bool.TryParse(args[0], out skip))
                throw new CmdException("Variable", $"Cannot convert \'{args[0]}\' to bool.");
            if (!skip && !Utils.BoolPrompt("Cleaning up will reset all scopes and variables and may cause errors in command chains.\n" +
                "Are you sure? Yes[Y] or No[N]: "))
            {
                cl.FeedbackLine("Cleanup canceled.");
                return;
            }
            _scopes.Clear();
            _tempScopes.Clear();
            _globalScope.Clear();
            global = false;
            tempGlobal = false;
            cl.FeedbackLine("Cleanup successfull.");
        }
    }
}
