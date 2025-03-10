namespace CMD
{
    internal class Script : Package
    {
        private string directory = string.Empty;

        public Script()
        {
            Name = "Script";
            Commands = new()
            {
                { "runscr", new(RunScriptCMD) { MinArgs = 0, MaxArgs = 3,
                    Description = "Runs the script from the specified relative path." } },
                { "loadscr", new(LoadCMD) { MinArgs = 2, MaxArgs = 2,
                    Description = "Loads the script from the specified relative path." } },
                { "delscr", Command.QCommand<string>(DeleteCMD, "Deletes the specified script.") },
                { "renamescr", new(RenameCMD) { MinArgs = 2, MaxArgs = 2,
                    Description =  "Renames the specified script." } },
                { "cd", new(CDAddCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Adds the specified path to the current directory." } },
                { @"cd\", new(CDSetCMD) { MinArgs = 0, MaxArgs = 1,
                    Description = "Sets the specified path to the current directory." } },
                { "cd<", new(Command.Translator(CDRemoveCMD, new[] { typeof(int) })) { MinArgs = 0, MaxArgs = 1,
                    Description = "Exits one layer from the current directory." } },
            };
        }

        private void RenameCMD(string[] args, Command.Callback cb)
        {
            if (Commands.ContainsKey(args[1]))
                throw new CommandException("Script", $"Script \'{args[1]}\' already exists.");
            if (!Commands.TryGetValue(args[0], out var command))
                throw new CommandException("Script", $"Script not found \'{args[0]}\'.");
            Commands.Remove(args[0]);
            Commands[args[1]] = command;
            cb.Launcher.FeedbackLine($"Sucessfully renamed \'{args[0]}\' to \'{args[1]}\'.");
        }

        private void DeleteCMD(string name)
        {
            if (!Commands.ContainsKey(name))
                throw new CommandException("Script", $"Script not found \'{name}\'.");
            Commands.Remove(name);
        }

        private void LoadCMD(string[] args, Command.Callback cb)
        {
            string relDir = CombineDir(args[0]);
            if (!File.Exists(relDir))
                throw new CommandException("Script", $"File not found {relDir}.");
            if (Commands.ContainsKey(args[1]))
                throw new CommandException("Script", $"Script \'{args[1]}\' already exists.");
            var lines = File.ReadAllLines(relDir);
            Commands.Add(args[1], new(_ => cb.Launcher.RunEveryCommand(lines)));
            Console.WriteLine($"Script \'{args[1]}\' added sucessfully!");
        }

        private void RunScriptCMD(string[] args, Command.Callback cb)
        {
            string relDir = CombineDir(args[0]);
            if (!File.Exists(relDir))
                throw new CommandException("Script", $"File not found {relDir}.");
            var lines = File.ReadAllLines(relDir);
            cb.Launcher.RunEveryCommand(lines);

            if (args.Length > 1)
            {
                switch (args[1])
                {
                    case "as":
                        if (args.Length != 3 || args[2] == string.Empty)
                            throw new CommandException("Script", $"Expected 3 Arguments, received {args.Length}.");
                        if (Commands.ContainsKey(args[2]))
                            throw new CommandException("Script", $"Script \"{args[2]}\" already exists.");
                        Commands.Add(args[2], new(_ => cb.Launcher.RunEveryCommand(lines)));
                        break;
                }
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
                throw new CommandException("Script", $"Unknown directory \"{dir}\".");
            directory = dir;
            launcher.InputRender = () => $"{directory}>";
        }

        private void CDAddCMD(string[] args, Command.Callback cb)
        {
            string path = args[0].Replace('/', '\\');
            if (directory.Length > 0 && path.Length > 0 && path[0] != '\\')
                path = '\\' + path;
            SetDir(directory + path, cb.Launcher);
        }

        private void CDSetCMD(string[] args, Command.Callback cb)
        {
            if (args.Length > 0)
                SetDir(args[0], cb.Launcher);
            else
            {
                directory = string.Empty;
                cb.Launcher.InputRender = null;
            }
        }

        private void CDRemoveCMD(object[] args, Command.Callback cb)
        {
            int n = args.Length > 0 ? (int)args[0] : 1;
            int index = StringUtils.LastNIndexOf(directory, '\\', n);
            if (index == -1)
                throw new CommandException("Script", "No layers to exit from.");
            SetDir(directory[..index], cb.Launcher);
        }
    }
}
