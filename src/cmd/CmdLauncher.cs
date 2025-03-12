using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SCE
{
    public class CmdLauncher
    {
        private bool active;

        private readonly Package _native;

        public CmdLauncher(int capacity = 0)
        {
            Packages = new(capacity);
            Custom = new();
            _native = new(new()
            {
                { "help", new(HelpCMD) { MaxArgs = -1, 
                    Description = "Displays every command.",
                    Usage = "<package_name>? ..." } },

                { "quit", new(args => Exit()) { Description = "Exits the command line." } },

                { "showfeed", Cmd.QCommand<bool>((c, cb) => cb.Launcher.CommandFeedback = c,
                    "Sets whether command feedback should be displayed.") },

                { "isfeed", new(GetFeedCMD) { Description = "Adds the feed state to memory." } },

                { "cmdexists", new(CommandExistsCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Determines whether the given command exists." } },

                { "showerror", new(ErrorsCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Sets whether error feedback should be displayed." } },

                { "feedback", new(FeedbackCMD(false)) { MinArgs = 1, MaxArgs = -1,
                    Description = "Feedbacks the given arguments." } },

                { "feedbackl", new(FeedbackCMD(true)) { MinArgs = 0, MaxArgs = -1,
                    Description = "Feedbacks the given arguments on new lines." } },

                { "loop", new(LoopCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Runs the command a given amount of times." } },

                { "catch", new(CatchCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Catches command execution errors. Useful in command chains."} },

                { "runif", new(IfCMD) { MinArgs = 2, MaxArgs = -1,
                    Description = "Runs the command if the condition is true."} },

                { "abort", new(args => throw new Exception("Aborted.")) { 
                    Description = "Ends execution of a command chain." } },

                { "haspkg", Cmd.QCommand<string>(HasPackageCMD, 
                    "Displays whether a package with the specified name exists.") },

                { "packages", new(PackagesCMD) { Description = "Displays all loaded packages." } },

                { "memclear", new(MemClearCMD) { Description = "Clears all items in laucher memory."} },

                { "memadd", new(MemAddCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Adds every given item to memory." } },

                { "memrem", new(MemRemCMD) { MinArgs = 0, MaxArgs = 1,
                    Description = "Removes the specified number of items from memory"} },

                { "memview", new(MemViewCMD) { Description = "Displays all items in memory" } },

                { "memlock", Cmd.QCommand<bool>(c => MemoryLock = c, "Sets the lock state of memory.") },

                { "memrun", new(MemRunCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Adds items to memory before running the command with no arguments." } },

                { "memins", new(MemoryInsertCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Inserts and removes items from memory into the given command" } },
            })
            {
                Name = "Native",
            };
        }

        public string Name { get; init; } = "Launcher";

        public Package Custom { get; init; }

        public HashSet<Package> Packages { get; init; }

        public Func<string>? InputRender { get; set; }

        public bool CommandFeedback { get; set; } = true;

        public bool ErrorFeedback { get; set; } = true;

        public bool MemoryLock { get; set; } = false;

        public Stack<object?> MemoryStack { get; } = new();

        public IEnumerable<Package> GetPackageEnumerator()
        {
            yield return _native;
            yield return Custom;
            foreach (var package in Packages)
                yield return package;
        }

        public void Exit()
        {
            active = false;
        }

        public void Run()
        {
            Console.WriteLine($"{Name}\nStart typing or type help to see available commands:");

            active = true;
            while (active)
            {
                if (InputRender != null)
                    Console.Write(InputRender.Invoke());
                var input = Console.ReadLine() ?? "";
                SExecuteCommand(input);
            }
        }

        #region Search

        public bool TryGetCommand(string name, [NotNullWhen(true)] out Cmd? command, [NotNullWhen(true)] out Package? package)
        {
            foreach (var item in GetPackageEnumerator())
            {
                if (item.Commands.TryGetValue(name, out command))
                {
                    package = item;
                    return true;
                }
            }
            command = null;
            package = null;
            return false;
        }

        public bool TryGetCommand(string name, [NotNullWhen(true)] out Cmd? command)
        {
            return TryGetCommand(name, out command, out _);
        }

        public bool CommandExists(string name)
        {
            return TryGetCommand(name, out _);
        }

        public bool TryGetPackage(string name, [NotNullWhen(true)] out Package? package)
        {
            var nLower = name.ToLower();
            foreach (var pkg in GetPackageEnumerator())
            {
                if (pkg.Name.ToLower() == nLower)
                {
                    package = pkg;
                    return true;
                }
            }
            package = null;
            return false;
        }

        public bool ContainsPackage(string name)
        {
            return TryGetPackage(name, out _);
        }

        #endregion

        #region Commands

        private Cmd.MemItem CommandExistsCMD(string[] args)
        {
            var exists = CommandExists(args[0]);
            FeedbackLine(exists);
            return new(exists);
        }

        private Cmd.MemItem GetFeedCMD(string[] args)
        {
            return new(CommandFeedback);
        }

        private Action<string[]> FeedbackCMD(bool newLine)
        {
            return args =>
            {
                if (!CommandFeedback)
                    return;
                StringBuilder sb = new();
                foreach (var arg in args)
                    sb.Append(newLine ? $"{arg}\n" : arg);
                Console.Write(sb.ToString());
            };
        }

        private void MemoryInsertCMD(string[] args)
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
                        if (!MemoryStack.TryPeek(out var res))
                            throw new CmdException("Launcher", $"Memory is empty.");
                        if (j != str.Length - 1 && str[j + 1] == '^')
                        {
                            MemoryStack.Pop();
                            ++j;
                        }
                        sb.Append(res);
                    }
                }
                args[i] = sb.ToString();
            }
            ExecuteCommand(args[0], ArrUtils.TrimFirst(args));
        }

        private void PackagesCMD(string[] args)
        {
            StringBuilder sb = new();
            foreach (var package in GetPackageEnumerator())
            {
                string name = package.Name == "" ? "Anonymous" : package.Name;
                sb.AppendLine($"{name}:\n  > Commands: {package.Commands.Count}");
            }
            Console.Write(sb.ToString());
        }

        private void ErrorsCMD(string[] args)
        {
            if (!bool.TryParse(args[0], out var result))
                throw new CmdException("Launcher", $"Invalid boolean \'{args[0]}\'.");
            ErrorFeedback = result;
            FeedbackLine($"Error feedback set to {result}.");
        }

        private void IfCMD(string[] args)
        {
            if (!bool.TryParse(args[0], out var result))
                throw new CmdException("Launcher", $"Invalid boolean \'{args[0]}\'.");
            if (result)
                SExecuteCommand(args[1], ArrUtils.TrimFromStart(args, 2));
        }

        private void CatchCMD(string[] args)
        {
            try
            {
                ExecuteCommand(args[0], ArrUtils.TrimFirst(args));  
            }
            catch
            {
            }
        }

        private void LoopCMD(string[] args)
        {
            if (!int.TryParse(args[0], out var loops))
                throw new CmdException("Launcher", $"Invalid loops \'{args[0]}\'.");
            var cmdArgs = ArrUtils.TrimFromStart(args, 2);
            for (int i = 0; i < loops; ++i)
            {
                if (!SExecuteCommand(args[1], cmdArgs))
                    throw new CmdException("Launcher", "Loop ended as command failed to execute.");
            }
        }

        private void MemRemCMD(string[] args)
        {
            int count = 1;
            if (args.Length > 0 && !int.TryParse(args[0], out count))
                throw new CmdException("Launcher", $"Failed to convert \'{args[0]}\' to int.");
            for (int i = 0; i < count; ++i)
            {
                if (MemoryStack.Count == 0)
                    throw new CmdException("Launcher", $"Ran out of items to remove. Cleared {i}/{count}.");
                MemoryStack.Pop();
            }
            FeedbackLine($"Sucessfully cleared {count} items from memory (now {MemoryStack.Count}).");
        }

        private void HasPackageCMD(string name)
        {
            if (TryGetPackage(name, out var package))
                FeedbackLine($"Found package \'{package.Name}\' with {package.Commands.Count} command(s).");
            else
                FeedbackLine($"No package with name \'{name}\' found.");
        }

        private void MemViewCMD(string[] args)
        {
            if (MemoryStack.Count == 0)
                FeedbackLine("No items to view.");
            else
            {
                StringBuilder sb = new();
                foreach (var item in MemoryStack)
                    sb.AppendLine($"> \"{item}\"");
                Console.Write(sb.ToString());
            }
        }

        private void MemRunCMD(string[] args)
        {
            for (int i = 1; i < args.Length; ++i)
                MemoryStack.Push(args[i]);
            SExecuteCommand(args[0]);
        }

        private void MemAddCMD(string[] args)
        {
            foreach (var item in args)
                MemoryStack.Push(item);
            FeedbackLine($"Sucessfully added {args.Length} items to memory.");
        }

        private void MemClearCMD(string[] args)
        {
            if (MemoryStack.Count == 0)
                FeedbackLine($"No items to clear.");
            else
            {
                FeedbackLine($"Successfully cleared {MemoryStack.Count} items from memory.");
                MemoryStack.Clear();
            }
        }

        private void HelpCMD(string[] args)
        {
            StringBuilder sb = new("- Commands -\n");
            if (args.Length == 0)
            {
                foreach (var pkg in GetPackageEnumerator())
                    sb.Append(BuildPackageHelp(pkg));               
            }
            else
            {
                foreach (var name in args)
                {
                    if (!TryGetPackage(name, out var pkg))
                        throw new CmdException("Launcher", $"Unknown package \'{name}\'.");
                    sb.Append(BuildPackageHelp(pkg));
                }
            }
            Console.Write(sb.ToString());
        }

        private static string BuildPackageHelp(Package pkg)
        {
            StringBuilder sb = new();
            sb.AppendLine(pkg.Name == "" ? "Anonymous Package:\n" : $"{pkg.Name}:\n");
            foreach (var item in pkg.Commands)
            {
                (var name, var c) = item;

                if (c.MinArgs != c.MaxArgs)
                    sb.AppendLine($"{name}[{c.MinArgs}-{(c.MaxArgs >= 0 ? c.MaxArgs : "n")}]");
                else
                    sb.AppendLine($"{name}[{c.MinArgs}]");

                if (c.Description != string.Empty)
                    sb.AppendLine($"- {c.Description}");
                if (c.Usage != string.Empty)
                    sb.AppendLine($"> {name} {c.Usage}");

                sb.AppendLine();
            }
            return sb.ToString();
        }

        #endregion

        #region Feedback

        public bool FeedbackLine(object? obj = null)
        {
            if (!CommandFeedback)
                return false;
            Console.WriteLine(obj);
            return true;
        }

        public bool Feedback(object? obj)
        {
            if (!CommandFeedback)
                return false;
            Console.Write(obj);
            return true;
        }

        #endregion

        #region Execute

        public bool SExecuteCommand(string name, string[] args)
        {
            try
            {
                ExecuteCommand(name, args);
                return true;
            }
            catch (CmdException e)
            {
                if (ErrorFeedback)
                    Console.WriteLine(e);
            }
            catch (Exception e)
            {
                if (ErrorFeedback)
                    StrUtils.PrettyErr("Launcher", e.Message);
            }
            return false;
        }

        public bool SExecuteCommand(string line)
        {
            string name = StrUtils.BuildWhile(line, (c) => c != ' ');
            var args = ArrUtils.TrimFirst(StrUtils.TrimArgs(line));
            return SExecuteCommand(name, args);
        }

        public void ExecuteCommand(string name, string[] args)
        {
            if (!TryGetCommand(name, out var cmd, out var package))
                throw new CmdException("Launcher", $"Unrecognised command \'{name}\'.");
            if (args.Length < cmd.MinArgs)
                throw new CmdException("Launcher", $"Too few arguments provided for command \'{name}\' (minimum of {cmd.MinArgs}, received {args.Length}).");
            if (cmd.MaxArgs >= 0 && args.Length > cmd.MaxArgs)
                throw new CmdException("Launcher", $"Too many arguments provided for command \'{name}\' (maximum of {cmd.MaxArgs}, recieved {args.Length}).");

            var result = cmd.Func(args, new(package, this));
            if (result != null && !MemoryLock)
                MemoryStack.Push(result.Value);
        }

        public void ExecuteEveryCommand(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                if (line != string.Empty && !SExecuteCommand(line))
                    throw new CmdException("Launcher", "Ending command chain.");
            }
        }

        #endregion
    }
}
