using System.Diagnostics.CodeAnalysis;

namespace CMD
{
    public class CommandLauncher
    {
        private bool active;

        public CommandLauncher(int capacity = 0)
        {
            Packages = new(capacity);
            Native = new();
        }

        public Package Native { get; init; }

        public HashSet<Package> Packages { get; init; }

        public Func<string>? InputRender { get; set; }

        public bool CommandFeedback { get; set; } = true;

        public bool TryGetCommand(string name, [NotNullWhen(true)] out Command? command, [NotNullWhen(true)] out Package? package)
        {
            if (Native.Commands.TryGetValue(name, out command))
            {
                package = Native;
                return true;
            }
            foreach (var item in Packages)
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
                RunCommand(input);
            }
        }

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

        public bool RunCommand(string name, string[] args, bool safeEvaluation = true)
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
                cmd.Action(args, new(package, this));
            else
            {
                try
                {
                    cmd.Action(args, new(package, this));
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

        public bool RunCommand(string line, bool safeEvaluation = true)
        {
            string name = StringUtils.BuildWhile(line, (c) => c != ' ');
            var args = ArrayUtils.TrimFirst(StringUtils.TrimArgs(line));
            return RunCommand(name, args, safeEvaluation);
        }

        public void RunEveryCommand(string[] lines, bool safeEvaluation = true)
        {
            foreach (var line in lines)
                RunCommand(line, safeEvaluation);
        }
    }
}
