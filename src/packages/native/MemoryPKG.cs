using CSUtils;
using System.Text;
namespace SCE
{
    internal class MemoryPKG : Package
    {
        public MemoryPKG()
        {
            Name = "Memory";
            Version = "0.2.0";
            Name = "Core launcher memory management.";
            Commands = new()
            {
                { "nout", new(NoMemCMD) { Min = 1, Max = -1,
                    Desc = "Locks memory for the subsequent command.",
                    Usage = Cmd.BCHAIN } },

                { "memclear", new(MemClearCMD) {
                    Desc = "Clears all items in laucher memory." } },

                { "memadd", new(MemAddCMD) { Min = 1, Max = -1,
                    Desc = "Adds every given item to memory.",
                    Usage = "<Item1>..." } },

                { "memrem", new(MemRemCMD) { Max = 1,
                    Desc = "Removes the specified number of items from memory",
                    Usage = "?<ItemCount->All>" } },

                { "memview", new(MemViewCMD) {
                    Desc = "Displays all items in memory" } },

                { "memsize", new(MemSizeCMD) {
                    Desc = "Outputs the number of items in the memory stack." } },

                { "memrun", new(MemRunCMD) { Min = 1, Max = -1,
                    Desc = "Adds items to memory before running the command with no arguments.",
                    Usage = "<CommandName> ?<Item1>..." } },

                { "memins", new(MemInsCMD) { Min = 1, Max = -1,
                    Desc = "Inserts items from memory into the command ('&'=peek '&^'=pop).",
                    Usage = Cmd.BCHAIN } },
            };
        }

        private static void NoMemCMD(string[] args, CmdLauncher cl)
        {
            bool old = cl.MemLock;
            cl.MemLock = true;
            cl.SExecuteCommand(args[0], Utils.TrimFirst(args));
            cl.MemLock = old;
        }

        private void MemAddCMD(string[] args, CmdLauncher cl)
        {
            foreach (var arg in args)
                cl.MemoryStack.Push(arg);
        }

        private void MemRemCMD(string[] args, CmdLauncher cl)
        {
            int count = 1;
            if (args.Length > 0 && !int.TryParse(args[0], out count))
                throw new CmdException("Launcher", $"Failed to convert \'{args[0]}\' to int.");
            for (int i = 0; i < count; ++i)
            {
                if (cl.MemoryStack.Count == 0)
                    throw new CmdException("Launcher", $"Ran out of items to remove. Cleared {i}/{count}.");
                cl.MemoryStack.Pop();
            }
            cl.FeedbackLine($"Sucessfully cleared {count} items from memory (now {cl.MemoryStack.Count}).");
        }

        private void MemViewCMD(string[] args, CmdLauncher cl)
        {
            if (cl.MemoryStack.Count == 0)
                throw new CmdException("Memory", "No items to view.");
            StringBuilder sb = new();
            foreach (var item in cl.MemoryStack)
                sb.AppendLine($"> \'{item}\'");
            Console.Write(sb.ToString());
        }

        private void MemClearCMD(string[] args, CmdLauncher cl)
        {
            if (cl.MemoryStack.Count == 0)
                throw new CmdException("Memory", "No items to clear.");
            cl.FeedbackLine($"Successfully cleared {cl.MemoryStack.Count} items from memory.");
            cl.MemoryStack.Clear();
        }

        private Cmd.MItem MemSizeCMD(string[] args, CmdLauncher cl)
        {
            int size = cl.MemoryStack.Count;
            cl.FeedbackLine(size);
            return new(size);
        }

        private void MemRunCMD(string[] args, CmdLauncher cl)
        {
            for (int i = 1; i < args.Length; ++i)
                cl.MemoryStack.Push(args[i]);
            cl.ExecuteCommand(args[0], Array.Empty<string>());
        }

        private void MemInsCMD(string[] args, CmdLauncher cl)
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
                        if (!cl.MemoryStack.TryPeek(out var res))
                            throw new CmdException("Launcher", $"Memory is empty.");
                        if (j != str.Length - 1 && str[j + 1] == '^')
                        {
                            cl.MemoryStack.Pop();
                            ++j;
                        }
                        sb.Append(res);
                    }
                }
                args[i] = sb.ToString();
            }
            cl.ExecuteCommand(args[0], Utils.TrimFirst(args));
        }
    }
}
