using System.Collections.Generic;
using System.Xml.Linq;

namespace CMD
{
    internal class ScriptPKG : Package
    {
        private string directory = string.Empty;

        public ScriptPKG()
        {
            Name = "Script";
            Commands = new()
            {
                { "scrrun", new(RunScriptCMD) { MinArgs = 0, MaxArgs = 2,
                    Description = "Runs the script from the specified relative path." } },
                { "scrload", new(LoadCMD) { MinArgs = 2, MaxArgs = 2,
                    Description = "Loads the script from the specified relative path." } },
                { "scrdel", new(DeleteCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Deletes the specified script." } },
                { "scrrename", new(RenameCMD) { MinArgs = 2, MaxArgs = 2,
                    Description =  "Renames the specified script." } },
                { "scrcompileload", new(LoadDirCMD) { MinArgs = 1, MaxArgs = 2,
                    Description = "Compiles all the scripts in a given directory into one command" } },
                { "cd", new(CDAddCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Adds the specified path to the current directory." } },
                { @"cd\", new(CDSetCMD) { MinArgs = 0, MaxArgs = 1,
                    Description = "Sets the specified path to the current directory." } },
                { "cd<", new(Cmd.Translator(CDRemoveCMD, new[] { typeof(int) })) { MinArgs = 0, MaxArgs = 1,
                    Description = "Exits one layer from the current directory." } },
            };
        }

        private void RenameCMD(string[] args, Cmd.Callback cb)
        {
            if (Commands.ContainsKey(args[1]))
                throw new CmdException("Script", $"Script \'{args[1]}\' already exists.");
            if (!Commands.TryGetValue(args[0], out var command))
                throw new CmdException("Script", $"Script not found \'{args[0]}\'.");
            Commands.Remove(args[0]);
            Commands[args[1]] = command;
            cb.Launcher.FeedbackLine($"Sucessfully renamed \'{args[0]}\' to \'{args[1]}\'.");
        }

        private void DeleteCMD(string[] args, Cmd.Callback cb)
        {
            if (!Commands.ContainsKey(args[0]))
                throw new CmdException("Script", $"Script not found \'{args[0]}\'.");
            Commands.Remove(args[0]);
            cb.Launcher.FeedbackLine($"Sucessfully removed script \'{args[0]}\'.");
        }

        private void LoadAbsolute(string path, string name, Cmd.Callback cb)
        {
            if (!File.Exists(path))
                throw new CmdException("Script", $"File not found {path}.");
            if (Commands.ContainsKey(name))
                throw new CmdException("Script", $"Script \'{name}\' already exists.");
            var lines = File.ReadAllLines(path);
            Commands.Add(name, new(_ => cb.Launcher.ExecuteEveryCommand(lines)));
            cb.Launcher.FeedbackLine($"Script \'{name}\' added sucessfully!");
        }

        private void LoadCMD(string[] args, Cmd.Callback cb)
        {
            LoadAbsolute(CombineDir(args[0]), args[1], cb);
        }

        private void LoadDirCMD(string[] args, Cmd.Callback cb)
        {
            if (Commands.ContainsKey(args[0]))
                throw new CmdException("Script", $"Script \'{args[0]}\' already exists.");
            string relDir = args.Length > 1 ? CombineDir(args[1]) : directory;
            if (!Directory.Exists(relDir))
                throw new CmdException("Script", $"Unknown directory \'{relDir}\'.");
            List<string> lines = new();
            foreach (var path in Directory.EnumerateFiles(relDir))
                lines.AddRange(File.ReadAllLines(path));
            Commands.Add(args[0], new(_ => cb.Launcher.ExecuteEveryCommand(lines)));
            cb.Launcher.FeedbackLine($"Script \'{args[0]}\' added sucessfully!");
        }

        private void RunScriptCMD(string[] args, Cmd.Callback cb)
        {
            string relDir = CombineDir(args[0]);
            if (!File.Exists(relDir))
                throw new CmdException("Script", $"File not found {relDir}.");
            var lines = File.ReadAllLines(relDir);
            cb.Launcher.ExecuteEveryCommand(lines);

            if (args.Length > 1)
            {
                if (args.Length != 2 || args[1] == string.Empty)
                    throw new CmdException("Script", $"Expected 2 Arguments, received {args.Length}.");
                if (Commands.ContainsKey(args[1]))
                    throw new CmdException("Script", $"Script \"{args[1]}\" already exists.");
                Commands.Add(args[1], new(_ => cb.Launcher.ExecuteEveryCommand(lines)));
            }
        }

        private string CombineDir(string relDir)
        {
            if (directory.Length > 0 && relDir.Length > 0 && relDir[0] != '\\' && directory[^1] != '\\')
                return string.Join("", directory, '\\', relDir);
            return directory + relDir;
        }

        private void SetDir(string dir, CommandLauncher launcher)
        {
            if (!Directory.Exists(dir))
                throw new CmdException("Script", $"Unknown directory \"{dir}\".");
            directory = dir;
            launcher.InputRender = () => $"{directory}>";
        }

        private void CDAddCMD(string[] args, Cmd.Callback cb)
        {
            string path = args[0].Replace('/', '\\');
            if (directory.Length > 0 && path.Length > 0 && path[0] != '\\')
                path = '\\' + path;
            SetDir(directory + path, cb.Launcher);
        }

        private void CDSetCMD(string[] args, Cmd.Callback cb)
        {
            if (args.Length > 0)
                SetDir(args[0], cb.Launcher);
            else
            {
                directory = string.Empty;
                cb.Launcher.InputRender = null;
            }
        }

        private void CDRemoveCMD(object[] args, Cmd.Callback cb)
        {
            int n = args.Length > 0 ? (int)args[0] : 1;
            int index = StringUtils.LastNIndexOf(directory, '\\', n);
            if (index == -1)
                throw new CmdException("Script", "No layers to exit from.");
            SetDir(directory[..index], cb.Launcher);
        }
    }
}
