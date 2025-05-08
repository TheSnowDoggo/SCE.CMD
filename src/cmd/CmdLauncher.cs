using CSUtils;
using System.Diagnostics.CodeAnalysis;
namespace SCE
{
    public class CmdLauncher
    {
        private bool active;

        private readonly Dictionary<string, Package> _packages = new();

        public CmdLauncher(string? name = null)
        {
            if (name != null)
                Name = name;
        }

        public string Name { get; init; } = "Launcher";

        public Func<string>? InputRender { get; set; }

        public Stack<object?> MemoryStack { get; } = new();

        #region Options

        public bool CommandFeedback { get; set; } = true;

        public bool ErrorFeedback { get; set; } = true;

        public bool MemoryLock { get; set; } = false;

        #endregion

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

        #region Package

        public IEnumerable<Package> Packages()
        {
            foreach (var pkg in _packages.Values)
                yield return pkg;
        }

        public bool LoadPackage(Package pkg)
        {
            var name = pkg.Name.ToLower();
            if (_packages.ContainsKey(name))
                return false;
            _packages[name] = pkg;
            return true;
        }

        public void SafeLoadPackages(IEnumerable<Package> packages)
        {
            foreach (var pkg in packages)
                if (!LoadPackage(pkg))
                    FeedbackLine($"<!> Failed to load package {pkg.Name} <!>");
        }

        public bool RemovePackage(string name)
        {
            return _packages.Remove(name.ToLower());
        }

        #endregion

        #region Search

        public bool TryGetCommand(string name, [NotNullWhen(true)] out Cmd? command, [NotNullWhen(true)] out Package? package)
        {
            foreach (var pkg in Packages())
            {
                if (pkg.Commands.TryGetValue(name, out command))
                {
                    package = pkg;
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

        public Package GetPackage(string name)
        {
            return _packages[name.ToLower()];
        }

        public bool TryGetPackage(string name, [NotNullWhen(true)] out Package? pkg)
        {
            return _packages.TryGetValue(name, out pkg);
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

        public void ExecuteCommand(string name, string[] args)
        {
            if (!TryGetCommand(name, out var cmd, out var package))
                throw new CmdException("Launcher", $"Unrecognised command \'{name}\'.");
            if (args.Length < cmd.MinArgs)
                throw new CmdException("Launcher", $"Too few args given for command \'{name}\' (mim of {cmd.MinArgs}, got {args.Length}).");
            if (cmd.MaxArgs >= 0 && args.Length > cmd.MaxArgs)
                throw new CmdException("Launcher", $"Too many args given for command \'{name}\' (max of {cmd.MaxArgs}, got {args.Length}).");

            var res = cmd.Func(args, new(package, this));
            if (res != null && !MemoryLock)
                MemoryStack.Push(res.Value);
        }

        public void ExecuteCommand(string line)
        {
            var name = StrUtils.BuildWhile(line, (c) => c != ' ');
            var args = Utils.TrimFirst(StrUtils.TrimArgs(line));
            ExecuteCommand(name, args);
        }

        public bool SExecuteCommand(string name, string[] args)
        {
            try
            {
                ExecuteCommand(name, args);
                return true;
            }
            catch (Exception e)
            {
                if (ErrorFeedback)
                    Console.WriteLine(e);
            }
            return false;
        }

        public bool SExecuteCommand(string line)
        {
            var name = StrUtils.BuildWhile(line, (c) => c != ' ');
            var args = Utils.TrimFirst(StrUtils.TrimArgs(line));
            return SExecuteCommand(name, args);
        }

        public void ExecuteEveryCommand(IEnumerable<string> lines)
        {
            foreach (var line in lines)
                if (line != "" && !SExecuteCommand(line))
                    throw new CmdException("Launcher", "Ending command chain.");
        }

        #endregion
    }
}
