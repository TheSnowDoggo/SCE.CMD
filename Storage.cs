using System.Text;

namespace CMD
{
    internal class Storage : Package
    {
        private readonly Dictionary<string, string> variables = new();

        public Storage()
        {
            Name = "Storage";
            Commands = new()
            {
                { "store", new(StoreCMD) { MinArgs = 2, MaxArgs = 2,
                    Description = "Stores the data into a variable." } },
                { "insert", new(InsertCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Inserts variables into the given command" } },
            };
        }

        public void StoreCMD(string[] args, Command.Callback cb)
        {
            if (variables.TryGetValue(args[0], out var oldValue))
                cb.Launcher.FeedbackLine($"{args[0]}: {oldValue} -> {args[1]}");
            else
                cb.Launcher.FeedbackLine($"{args[0]} = {args[1]}");
            variables[args[0]] = args[1];
        }

        public string Replace(string str)
        {
            StringBuilder sb = new();
            bool inside = false;
            for (int i = 0; i < str.Length; ++i)
            {
                if (str[i] == '$')
                {
                    inside = !inside;
                    if (inside)
                        continue;
                }
                if (inside && str[i] != '$')
                {
                    string name = "";
                    for (int j = 0; i + j < str.Length; ++j)
                    {
                        name += str[i + j];
                        if (variables.TryGetValue(name, out var stored))
                        {
                            sb.Append(stored);
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

        public void InsertCMD(string[] args, Command.Callback cb)
        {
            var newArgs = ArrayUtils.TrimFirst(args);
            for (int i = 0; i < newArgs.Length; ++i)
                newArgs[i] = Replace(newArgs[i]);
            cb.Launcher.RunCommand(args[0], newArgs);
        }
    }
}
