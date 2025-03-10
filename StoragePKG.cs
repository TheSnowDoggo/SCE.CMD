
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
                { "storemem", new(StoreMemCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Stores the latest memory stored in the launcher." } },
                { "store", new(StoreCMD) { MinArgs = 2, MaxArgs = 2,
                    Description = "Stores the data into a variable." } },
                { "insert", new(InsertCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Inserts variables into the given command" } },
            };
        }

        private void StoreVariable(string name, string data, Command.Callback cb)
        {
            if (variables.TryGetValue(name, out var oldValue))
                cb.Launcher.FeedbackLine($"{name}: {oldValue} -> {data}");
            else
                cb.Launcher.FeedbackLine($"{name} = {data}");
            variables[name] = data;
        }

        private void StoreMemCMD(string[] args, Command.Callback cb)
        {
            bool remove = false;
            if (args.Length > 1 && !bool.TryParse(args[1], out remove))
                throw new CommandException("Storage", $"Invalid boolean \'{args[1]}\'.");
            if (cb.Launcher.MemoryStack.Count == 0)
                throw new CommandException("Storage", "Memory is empty.");
            var store = remove ? cb.Launcher.MemoryStack.Pop() : cb.Launcher.MemoryStack.Peek();
            if (store.ToString() is string str)
                StoreVariable(args[0], str, cb);
            else
                throw new CommandException("Storage", "Stored value is null.");
        }

        private void StoreCMD(string[] args, Command.Callback cb)
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

        private void InsertCMD(string[] args, Command.Callback cb)
        {
            var newArgs = ArrayUtils.TrimFirst(args);
            for (int i = 0; i < newArgs.Length; ++i)
                newArgs[i] = Replace(newArgs[i]);
            cb.Launcher.ExecuteCommand(args[0], newArgs);
        }
    }
}
