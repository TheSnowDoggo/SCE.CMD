using CSUtils;
using System.Diagnostics.CodeAnalysis;

namespace SCE
{
    public class CmdLauncher
    {
        private bool active;

        public CmdLauncher(int capacity = 0)
        {
            Packages = new(capacity);
        }

        public string Name { get; init; } = "Launcher";

        public HashSet<Package> Packages { get; init; }

        public Func<string>? InputRender { get; set; }

        public bool CommandFeedback { get; set; } = true;

        public bool ErrorFeedback { get; set; } = true;

        public bool MemoryLock { get; set; } = false;

        public bool PrettyErrors { get; set; } = true;

        public Stack<object?> MemoryStack { get; } = new();

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
                var input = Console.ReadLine() ?? string.Empty;
                if (input != string.Empty)
                    SExecuteCommand(input);
            }
        }

        #region Search

        public bool TryGetCommand(string name, [NotNullWhen(true)] out Cmd? command, [NotNullWhen(true)] out Package? package)
        {
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
            foreach (var pkg in Packages)
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
                if (PrettyErrors)
                    StrUtils.PrettyErr(e.Source ?? "Unknown", e.Message);
                else
                    Console.WriteLine(e);
            }
            return false;
        }

        public bool SExecuteCommand(string line)
        {
            string name = StrUtils.BuildWhile(line, (c) => c != ' ');
            var args = Utils.TrimFirst(StrUtils.TrimArgs(line));
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

        public void ExecuteCommand(string line)
        {
            string name = StrUtils.BuildWhile(line, (c) => c != ' ');
            var args = Utils.TrimFirst(StrUtils.TrimArgs(line));
            ExecuteCommand(name, args);
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
