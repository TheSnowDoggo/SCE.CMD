using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CMD
{
    public class CommandLauncher
    {
        private bool active;

        private readonly Package _native;

        public CommandLauncher(int capacity = 0)
        {
            Packages = new(capacity);
            Custom = new();
            _native = new(new()
            {
                { "help", new(HelpCMD) { Description = "Displays every command." } },
                { "feedback", Command.QCommand<bool>((c, cb) => cb.Launcher.CommandFeedback = c) },
                { "memclear", new(MemClearCMD) { Description = "Clears all items in laucher memory."} },
                { "memadd", new(MemAddCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Adds every given item to memory." } },
                { "memview", new(MemViewCMD) { Description = "Displays all items in memory" } },
            })
            {
                Name = "Native",
            };
        }

        public Package Custom { get; init; }

        public HashSet<Package> Packages { get; init; }

        public Func<string>? InputRender { get; set; }

        public bool CommandFeedback { get; set; } = true;

        public Stack<object> MemoryStack { get; } = new();

        public IEnumerable<Package> GetPackageEnumerator()
        {
            yield return _native;
            yield return Custom;
            foreach (var package in Packages)
                yield return package;
        }

        public bool TryGetCommand(string name, [NotNullWhen(true)] out Command? command, [NotNullWhen(true)] out Package? package)
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

        public void Exit()
        {
            active = false;
        }

        public void Run()
        {
            Console.WriteLine("- SCE.CMD Launcher v0.0.0 -\nStart typing or type help to see available commands:");

            active = true;
            while (active)
            {
                if (InputRender != null)
                    Console.Write(InputRender.Invoke());
                var input = Console.ReadLine() ?? "";
                ExecuteCommand(input);
            }
        }

        #region Commands

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
            foreach (var package in GetPackageEnumerator())
            {
                sb.AppendLine(package.Name == "" ? "Anonymous Package:\n" : $"{package.Name}:\n");
                foreach (var item in package.Commands)
                {
                    var command = item.Value;
                    sb.AppendLine($"{item.Key}[{command.MinArgs}-{command.MaxArgs}]");
                    if (command.Description != string.Empty)
                        sb.AppendLine($"> {command.Description}");
                    sb.AppendLine();
                }
            }
            Console.Write(sb.ToString());
        }

        #endregion

        #region Feedback

        public bool FeedbackLine(object? obj)
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

        public bool ExecuteCommand(string name, string[] args, bool safeEvaluation = true)
        {
            if (!TryGetCommand(name, out var cmd, out var package))
            {
                StringUtils.PrettyErr("Launcher", $"Unrecognised command \'{name}\'.");
                return false;
            }
            if (args.Length < cmd.MinArgs)
            {
                StringUtils.PrettyErr("Launcher", $"Too few arguments provided for command \'{name}\' (minimum of {cmd.MinArgs}, received {args.Length}).");
                return false;
            }
            if (cmd.MaxArgs >= 0 && args.Length > cmd.MaxArgs)
            {
                StringUtils.PrettyErr("Launcher", $"Too many arguments provided for command \'{name}\' (maximum of {cmd.MaxArgs}, recieved {args.Length}).");
                return false;
            }
            if (!safeEvaluation)
                cmd.Func(args, new(package, this));
            else
            {
                try
                {
                    var result = cmd.Func(args, new(package, this));
                    if (result != null)
                        MemoryStack.Push(result);
                    return true;
                }
                catch (CommandException exception)
                {
                    Console.WriteLine(exception);
                }
                catch
                {
                    StringUtils.PrettyErr("Launcher", $"Command \'{name}\' failed to execute.");
                }
                return false;
            }
            return true;
        }

        public bool ExecuteCommand(string line, bool safeEvaluation = true)
        {
            string name = StringUtils.BuildWhile(line, (c) => c != ' ');
            var args = ArrayUtils.TrimFirst(StringUtils.TrimArgs(line));
            return ExecuteCommand(name, args, safeEvaluation);
        }

        public void ExecuteEveryCommand(string[] lines, bool safeEvaluation = true)
        {
            foreach (var line in lines)
                ExecuteCommand(line, safeEvaluation);
        }

        #endregion
    }
}
