using CSUtils;
using System.Diagnostics.CodeAnalysis;
namespace SCE
{
    public class CmdLauncher
    {
        private readonly Dictionary<string, Package> _packages = new();

        private readonly Dictionary<string, string> _cmdCache = new();

        private bool active;

        public CmdLauncher()
        {
            _superStack.Push(new());
        }

        public PVersion Version { get; init; } = PVersion.Zero;

        public Func<string>? InputRender;

        public SortedSet<Preprocessor> Preprocessors { get; } = new();

        private readonly Stack<int> _locks = new();

        private readonly Stack<Stack<object>> _superStack = new();

        public Stack<object> MemoryStack { get => _superStack.Peek(); }

        public int SStackCount { get => _superStack.Count; }

        #region Options

        public bool CmdFeedback { get; set; } = true;

        public bool ErrFeedback { get; set; } = true;

        public bool MemLock { get; set; } = false;

        public bool CmdCaching { get; set; } = true;

        public bool NeatErrors { get; set; } = true;

        public bool Preprocessing { get; set; } = true;

        public bool InputRendering { get; set; } = true;

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
                if (InputRendering && InputRender != null)
                    Console.Write(InputRender.Invoke());
                var input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                    SExecuteCommand(PreProcess(input));
            }
        }

        #region Memory

        public bool IsLocked()
        {
            return _locks.Count > 0 && _locks.Peek() == _superStack.Count;
        }

        public void Lock()
        {
            if (_superStack.Count <= 1)
                throw new CmdException("Launcher", "Cannot lock bottom stack.");
            if (_locks.Count > 0 && _locks.Peek() == _superStack.Count)
                throw new CmdException("Launcher", "Current stack is already locked");
            _locks.Push(_superStack.Count);
        }

        public void Unlock()
        {
            if (_locks.Count == 0)
                throw new CmdException("Launcher", "No stacks to unlock.");
            if (_locks.Peek() != _superStack.Count)
                throw new CmdException("Launcher", "Current stack is not locked.");
            _locks.Pop();
        }

        public void AddStack(bool tryLock = false)
        {
            _superStack.Push(new());
            if (tryLock)
                Lock();
        }

        public void RemoveStack(bool tryUnlock = false)
        {
            if (_superStack.Count <= 1)
                throw new CmdException("Launcher", "Removing bottom stack is disallowed.");
            if (tryUnlock && IsLocked())
                Unlock();
            if (_locks.Count > 0 && _locks.Peek() == _superStack.Count)
                throw new CmdException("Launcher", "Current stack is locked.");
            _superStack.Pop();
        }

        public void ClearStacks()
        {
            _locks.Clear();
            _superStack.Clear();
            AddStack();
        }

        #endregion

        #region Preprocess

        public string PreProcess(string input)
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

        public void LoadPackage(Package pkg)
        {
            var name = pkg.Name.ToLower();
            if (_packages.ContainsKey(name))
            {
                StrUtils.PrettyErr("PKGLoader", $"Package with name {name} already exists.");
                if (!Utils.BoolPrompt("Would you like to overwrite it? Yes[Y] or No[N]: "))
                    return;
            }
            if (!pkg.IsCompatible(this))
            {
                StrUtils.PrettyErr("PKGLoader", $"{name} v{pkg.Version} " +
                   $"is incompatible with launcher v{Version}");
                if (!Utils.BoolPrompt("Would you still like to load it? Yes[Y] or No[N]: "))
                    return;
            }
            _packages[name] = pkg;
            pkg.Initialize(this);
        }

        public void SLoadPackages(IEnumerable<Package> packages)
        {
            foreach (var pkg in packages)
            {
                try
                {
                    LoadPackage(pkg);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
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
            if (CmdCaching && _cmdCache.TryGetValue(cmd, out var cachedPName))
            {
                if (TryGetPackage(cachedPName, out package) && package.Commands.TryGetValue(cmd, out command))
                    return true;
                _cmdCache.Remove(cmd);
            }
            foreach (var pkg in Packages())
            {
                if (pkg.Commands.TryGetValue(cmd, out command))
                {
                    package = pkg;
                    if (CmdCaching)
                        _cmdCache[cmd] = pkg.Name;
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
            int count = _cmdCache.Count;
            _cmdCache.Clear();
            return count;
        }

        public int CacheSize()
        {
            return _cmdCache.Count;
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
            if (args.Length < cmd.Min)
                throw new CmdException("Launcher", $"Too few args given for command \'{name}\' (mim of {cmd.Min}, got {args.Length}).");
            if (cmd.Max >= 0 && args.Length > cmd.Max)
                throw new CmdException("Launcher", $"Too many args given for command \'{name}\' (max of {cmd.Max}, got {args.Length}).");

            var res = cmd.Func.Invoke(args, this);
            if (res != null && !MemLock && res.Value != null)
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

        public bool SExecuteCommand(string line)
        {
            try
            {
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

        public void ExecuteEveryCommand(IEnumerable<string> lines, bool endOnFail = true)
        {
            foreach (var line in lines)
                if (!string.IsNullOrWhiteSpace(line) && !SExecuteCommand(PreProcess(line)) && endOnFail)
                    throw new CmdException("Launcher", "Ending command chain.");
        }

        #endregion
    }
}