using System.Text;

namespace SCE
{
    internal class MemoryPKG : Package
    {
        public MemoryPKG()
        {
            Name = "Memory";
            Commands = new()
            {
                { "memclear", new(MemClearCMD) {
                    Description = "Clears all items in laucher memory." } },

                { "memadd", new(MemAddCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Adds every given item to memory.",
                    Usage = "<Item1>..." } },

                { "memrem", new(MemRemCMD) { MaxArgs = 1,
                    Description = "Removes the specified number of items from memory",
                    Usage = "?<ItemCount->All>" } },

                { "memview", new(MemViewCMD) {
                    Description = "Displays all items in memory" } },

                { "memlock", new(MemLockCMD) { MaxArgs = 1,
                    Description = "Sets the lock state of memory.",
                    Usage = "?<True/False->Toggle>" } },

                { "memcount", new(MemSizeCMD) {
                    Description = "Outputs the number of items in the memory stack." } },

                { "memrun", new(MemRunCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Adds items to memory before running the command with no arguments.",
                    Usage = "<CommandName> ?<Item1>..." } },

                { "memins", new(MemInsCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Inserts items from memory into the command ('&'=peek '&^'=pop).",
                    Usage = "<Command> ?<Arg1>..." } },
            };
        }

        private void MemAddCMD(string[] args, Cmd.Callback cb)
        {
            foreach (var item in args)
                cb.Launcher.MemoryStack.Push(item);
            cb.Launcher.FeedbackLine($"Sucessfully added {args.Length} items to memory.");
        }

        private void MemRemCMD(string[] args, Cmd.Callback cb)
        {
            int count = 1;
            if (args.Length > 0 && !int.TryParse(args[0], out count))
                throw new CmdException("Launcher", $"Failed to convert \'{args[0]}\' to int.");
            for (int i = 0; i < count; ++i)
            {
                if (cb.Launcher.MemoryStack.Count == 0)
                    throw new CmdException("Launcher", $"Ran out of items to remove. Cleared {i}/{count}.");
                cb.Launcher.MemoryStack.Pop();
            }
            cb.Launcher.FeedbackLine($"Sucessfully cleared {count} items from memory (now {cb.Launcher.MemoryStack.Count}).");
        }

        private void MemViewCMD(string[] args, Cmd.Callback cb)
        {
            if (cb.Launcher.MemoryStack.Count == 0)
                cb.Launcher.FeedbackLine("No items to view.");
            else
            {
                StringBuilder sb = new();
                foreach (var item in cb.Launcher.MemoryStack)
                    sb.AppendLine($"> \"{item}\"");
                Console.Write(sb.ToString());
            }
        }

        private void MemClearCMD(string[] args, Cmd.Callback cb)
        {
            if (cb.Launcher.MemoryStack.Count == 0)
                cb.Launcher.FeedbackLine($"No items to clear.");
            else
            {
                cb.Launcher.FeedbackLine($"Successfully cleared {cb.Launcher.MemoryStack.Count} items from memory.");
                cb.Launcher.MemoryStack.Clear();
            }
        }

        private void MemLockCMD(string[] args, Cmd.Callback cb)
        {
            bool res = !cb.Launcher.MemoryLock;
            if (args.Length > 0 && !bool.TryParse(args[0], out res))
                throw new CmdException("Native", $"Unable to convert \'{args[0]}\'.");
            cb.Launcher.MemoryLock = res;
            cb.Launcher.FeedbackLine($"Memory lock set to {res}.");
        }

        private Cmd.MemItem MemSizeCMD(string[] args, Cmd.Callback cb)
        {
            int size = cb.Launcher.MemoryStack.Count;
            cb.Launcher.FeedbackLine(size);
            return new(size);
        }

        private void MemRunCMD(string[] args, Cmd.Callback cb)
        {
            for (int i = 1; i < args.Length; ++i)
                cb.Launcher.MemoryStack.Push(args[i]);
            cb.Launcher.SExecuteCommand(args[0]);
        }

        private void MemInsCMD(string[] args, Cmd.Callback cb)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                string str = args[i];
                StringBuilder sb = new();
                for (int j = 0; j < str.Length; ++j)
                {
                    if (str[j] != '&')
                        sb.Append(str[j]);
                    else
                    {
                        if (!cb.Launcher.MemoryStack.TryPeek(out var res))
                            throw new CmdException("Launcher", $"Memory is empty.");
                        if (j != str.Length - 1 && str[j + 1] == '^')
                        {
                            cb.Launcher.MemoryStack.Pop();
                            ++j;
                        }
                        sb.Append(res);
                    }
                }
                args[i] = sb.ToString();
            }
            cb.Launcher.ExecuteCommand(args[0], ArrUtils.TrimFirst(args));
        }
    }
}
