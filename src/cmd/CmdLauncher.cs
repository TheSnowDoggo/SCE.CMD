using CSUtils;
using System.Diagnostics.CodeAnalysis;
namespace SCE
{
    public class CmdLauncher
    {
        public const string VERSION = "0.8.4";

        private readonly Dictionary<string, Package> _packages = new();

        private readonly Dictionary<string, string> _commandCache = new();

        private bool active;

        public CmdLauncher(string name = "Unnamed Launcher")
        {
            Name = name;
        }

        public string Name { get; init; }

        public Func<string>? InputRender { get; set; }

        public SortedSet<Preprocessor> Preprocessors { get; set; } = new();

        public Stack<object?> MemoryStack { get; } = new();

        #region Options

        public bool CmdFeedback { get; set; } = true;

        public bool ErrFeedback { get; set; } = true;

        public bool MemLock { get; set; } = false;

        public bool CmdCaching { get; set; } = true;

        public bool NeatErrors { get; set; } = true;

        #endregion

        public void Exit()
        {
            active = false;
        }

        public void Run()
        {
            active = true;
            while (active)
            {
                if (InputRender != null)
                    Console.Write(InputRender.Invoke());
                var input = Console.ReadLine() ?? "";
                SExecuteCommand(input);
            }
        }

        #region Preprocess

        public string Process(string input)
        {
            foreach (var p in Preprocessors)
                input = p.Process(input);
            return input;
        }

        #endregion

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
            pkg.Initialize(this);
            return true;
        }

        public void SLoadPackages(IEnumerable<Package> packages)
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

        #region Get

        public bool TryGetCommand(string cmd, string pkgName, [NotNullWhen(true)] out Cmd? command, [NotNullWhen(true)] out Package? package)
        {
            command = null;
            return TryGetPackage(pkgName, out package) && package.Commands.TryGetValue(cmd, out command);
        }

        public bool TryGetCommand(string cmd, [NotNullWhen(true)] out Cmd? command, [NotNullWhen(true)] out Package? package)
        {
            if (CmdCaching && _commandCache.TryGetValue(cmd, out var cachedPName) &&
                TryGetPackage(cachedPName, out package) && package.Commands.TryGetValue(cmd, out command))
            {
                return true;
            }
            foreach (var pkg in Packages())
            {
                if (pkg.Commands.TryGetValue(cmd, out command))
                {
                    package = pkg;
                    if (CmdCaching)
                        _commandCache[cmd] = pkg.Name;
                    return true;
                }
            }
            command = null;
            package = null;
            return false;
        }

        public bool TryGetCommand(string cmd, [NotNullWhen(true)] out Cmd? command)
        {
            return TryGetCommand(cmd, out command, out _);
        }

        public bool ContainsCommand(string name)
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

        #region Cache

        public int ClearCache()
        {
            int count = _commandCache.Count;
            _commandCache.Clear();
            return count;
        }

        public int CacheSize()
        {
            return _commandCache.Count;
        }

        #endregion

        #region Feedback

        public bool FeedbackLine(object? obj = null)
        {
            if (!CmdFeedback)
                return false;
            Console.WriteLine(obj);
            return true;
        }

        public bool Feedback(object? obj)
        {
            if (!CmdFeedback)
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

            var res = cmd.Func.Invoke(args, new Cmd.Callback(package, this));
            if (res != null && !MemLock)
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
            catch (CmdException e)
            {
                if (ErrFeedback)
                    Console.WriteLine(e);
            }
            catch (Exception e)
            {
                if (ErrFeedback)
                    Console.WriteLine(NeatErrors ? e.Message : e);
            }
            return false;
        }

        private bool SExecuteCommand(string line)
        {
            try
            {
                line = Process(line);
                if (string.IsNullOrWhiteSpace(line))
                    return false;
                ExecuteCommand(line);
                return true;
            }
            catch (CmdException e)
            {
                if (ErrFeedback)
                    Console.WriteLine(e);
            }
            catch (Exception e)
            {
                if (ErrFeedback)
                    Console.WriteLine(NeatErrors ? e.Message : e);
            }
            return false;
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
