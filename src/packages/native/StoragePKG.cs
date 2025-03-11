
using System.Text;

namespace CMD
{
    internal class StoragePKG : Package
    {
        private readonly Dictionary<string, string> variables = new();

        public StoragePKG()
        {
            Name = "Storage";
            Commands = new()
            {
                { "takefmem", new(StoreMemCMD(true)) { MinArgs = 1, MaxArgs = -1, 
                    Description = "Stores the latest memory and removes it."} },
                { "peekfmem", new(StoreMemCMD(false)) { MinArgs = 1, MaxArgs = -1,
                    Description = "Takes the latest memory without removing it." } },
                { "ststore", new(StoreCMD) { MinArgs = 2, MaxArgs = 2,
                    Description = "Stores the data into a variable." } },
                { "stclear", new(ClearCMD) { Description = "Clears every variable" } },
                { "stdel", new(RemoveVariableCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Deletes the specified variable." } },
                { "stview", new(ViewVariableCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Displays all the specified variables." } },
                { "insert", new(InsertCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Inserts variables into the given command" } },
            };
        }

        private void ViewVariableCMD(string[] args, Cmd.Callback cb)
        {
            if (variables.Count == 0)
            {
                cb.Launcher.FeedbackLine("No variables to view.");
                return;
            }
            StringBuilder sb = new();
            if (args[0] == "*")
            {
                foreach (var item in variables)
                    sb.AppendLine($"{item.Key} = {item.Value}");
            }
            else
            {
                foreach (var name in args)
                {
                    if (!variables.TryGetValue(name, out var store))
                        throw new CmdException("Storage", $"Unknown variable \'{name}\'.");
                    sb.AppendLine($"{name} = {store}");
                }
            }
            Console.Write(sb.ToString());
        }

        private void StoreVariable(string name, string data, Cmd.Callback cb)
        {
            if (variables.TryGetValue(name, out var oldValue))
                cb.Launcher.FeedbackLine($"{name}: {oldValue} -> {data}");
            else
                cb.Launcher.FeedbackLine($"{name} = {data}");
            variables[name] = data;
        }

        private Action<string[], Cmd.Callback> StoreMemCMD(bool remove)
        {
            return (args, cb) =>
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    if (cb.Launcher.MemoryStack.Count == 0)
                        throw new CmdException("Storage", "Memory is empty.");
                    var store = remove ? cb.Launcher.MemoryStack.Pop() : cb.Launcher.MemoryStack.Peek();
                    if (store.ToString() is string str)
                        StoreVariable(args[i], str, cb);
                    else
                        throw new CmdException("Storage", "Stored value is null.");
                }
            };
        }

        private void RemoveVariableCMD(string[] args, Cmd.Callback cb)
        {
            foreach (var name in args)
            {
                if (!variables.ContainsKey(name))
                    throw new CmdException("Storage", $"Unknown variable \'{name}\'.");
                variables.Remove(name);
                cb.Launcher.FeedbackLine($"Sucessfully removed  variable \'{name}\'.");
            }
        }

        private void ClearCMD(string[] args, Cmd.Callback cb)
        {
            if (variables.Count == 0)
                cb.Launcher.FeedbackLine("No variables to clear.");
            else
            {
                cb.Launcher.FeedbackLine($"Sucessfully cleared {variables.Count} variables.");
                variables.Clear();
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
                if (str[i] == '$')
                {
                    string name = "";
                    for (int j = 0; i + j + 1 < str.Length; ++j)
                    {
                        name += str[i + j + 1];
                        if (variables.TryGetValue(name, out var stored))
                        {
                            sb.Append(stored);
                            if (name.Length > 0)
                                i += name.Length;
                            break;
                        }
                    }
                }
                else
                {
                    sb.Append(str[i]);
                }
            }
            return sb.ToString();
        }

        private void InsertCMD(string[] args, Cmd.Callback cb)
        {
            var newArgs = ArrayUtils.TrimFirst(args);
            for (int i = 0; i < newArgs.Length; ++i)
                newArgs[i] = Replace(newArgs[i]);
            cb.Launcher.ExecuteCommand(args[0], newArgs);
        }
    }
}
