﻿using CSUtils;
using System.Text;
namespace SCE
{
    public class MemoryPKG : Package
    {
        public MemoryPKG()
        {
            Name = "Memory";
            Version = new(0, 3, 0);
            Desc = "Core launcher memory management.";
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
                    Desc = "Adds items to a new memory stack before running the command.",
                    Usage = "<FullCommand> ?<Item1>..." } },

                { "poprun", new(PopRunCMD) { Min = 2, Max = -1,
                    Desc = "Pops the given number of items from memory before running the command on a new stack.",
                    Usage = "<Count> " + Cmd.BCHAIN } },

                { "memins", new(MemInsCMD) { Min = 1, Max = -1,
                    Desc = "Inserts items from memory into the command ('&'=peek '&^'=pop).",
                    Usage = Cmd.BCHAIN } },

                { ">stack", new((_, cl) => cl.AddStack()) {
                    Desc = "Adds a new memory stack." } },

                { "<stack", new((_, cl) => cl.RemoveStack()) {
                    Desc = "Remove the current memory stack." } },

                { "/stack", new(RunStackCMD) { Min = 1, Max = -1,
                    Desc = "Runs the command in a new locked stack.",
                    Usage = Cmd.BCHAIN } },

                { "?stack", new(GetStackCMD) {
                    Desc = "Remove the current memory stack.",
                    Usage = Cmd.MBCHAIN } },
            };
        }

        private static void NoMemCMD(string[] args, CmdLauncher cl)
        {
            bool old = cl.MemLock;
            cl.MemLock = true;
            cl.SExecuteCommand(args[0], Utils.TrimFirst(args));
            cl.MemLock = old;
        }

        private static void MemAddCMD(string[] args, CmdLauncher cl)
        {
            foreach (var arg in args)
                cl.MemoryStack.Push(arg);
        }

        private static void MemRemCMD(string[] args, CmdLauncher cl)
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

        private static void MemViewCMD(string[] args, CmdLauncher cl)
        {
            if (cl.MemoryStack.Count == 0)
                throw new CmdException("Memory", "No items to view.");
            StringBuilder sb = new();
            foreach (var item in cl.MemoryStack)
                sb.AppendLine($"> \'{item}\'");
            Console.Write(sb.ToString());
        }

        private static void MemClearCMD(string[] args, CmdLauncher cl)
        {
            if (cl.MemoryStack.Count == 0)
                throw new CmdException("Memory", "No items to clear.");
            cl.FeedbackLine($"Successfully cleared {cl.MemoryStack.Count} items from memory.");
            cl.MemoryStack.Clear();
        }

        private static Cmd.MItem MemSizeCMD(string[] args, CmdLauncher cl)
        {
            int size = cl.MemoryStack.Count;
            cl.FeedbackLine(size);
            return new(size);
        }

        private static void StackWrap(CmdLauncher cl, Action action)
        {
            int dest = cl.SStackCount;
            cl.AddStack(true);
            action.Invoke();
            int dif = cl.SStackCount - dest;
            if (dif <= 0)
                throw new CmdException("Memory", "Stack is missing.");
            for (int i = 0; i < dif; ++i)
                cl.RemoveStack(true);
        }

        private static void MemRunCMD(string[] args, CmdLauncher cl)
        {
            StackWrap(cl, () =>
            {
                for (int i = 1; i < args.Length; ++i)
                    cl.MemoryStack.Push(args[i]);
                cl.SExecuteCommand(args[0]);
            });
        }

        private static void PopRunCMD(string[] args, CmdLauncher cl)
        {
            int count = int.Parse(args[0]);
            var arr = new object[count];
            for (int i = 0; i < count; ++i)
                arr[i] = cl.MemoryStack.Pop();
            StackWrap(cl, () =>
            {
                for (int i = 0; i < count; ++i)
                    cl.MemoryStack.Push(arr[i]);
                cl.SExecuteCommand(args[1], Utils.TrimFromStart(args, 2));
            });
        }

        private static void MemInsCMD(string[] args, CmdLauncher cl)
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

        private static void RunStackCMD(string[] args, CmdLauncher cl)
        {
            StackWrap(cl, () => cl.SExecuteCommand(args[0], Utils.TrimFirst(args)));
        }

        private static Cmd.MItem GetStackCMD(string[] args, CmdLauncher cl)
        {
            if (args.Length > 0)
                cl.SExecuteCommand(args[0], Utils.TrimFirst(args));
            return new(cl.SStackCount);
        }
    }
}
