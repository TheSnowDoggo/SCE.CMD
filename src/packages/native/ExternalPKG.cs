using System.IO;
using System.Text;

namespace SCE
{
    internal class ExternalPKG : Package
    {
        private string directory = string.Empty;

        public ExternalPKG()
        {
            Name = "External";
            Commands = new()
            {
                { "pkgload", new(PackageLoadCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Loads the packages from the specified relative paths." } },

                { "pkgloaddir", new(PackageLoadDirCMD) { MinArgs = 0, MaxArgs = 1,
                    Description = "Loads the packaes from all the assemblies in the specified relative paths." } },

                { "pkgdel", new(PackageRemoveCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Removes the specified packages." } },
                 
                { "scrrun", new(RunScriptCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Runs the script from the specified relative path." } },

                { "scrload", new(LoadCMD) { MinArgs = 2, MaxArgs = 2,
                    Description = "Loads the script from the specified relative path." } },

                { "scrrundir", new(RunDirCMD) { MinArgs = 0, MaxArgs = 1,
                    Description = "Runs all the scripts in the given directory." } },

                { "scrloaddir", new(LoadDirCMD) { MinArgs = 0, MaxArgs = 1,
                    Description = "Loads all the scripts in the given directory." } },

                { "scrdel", new(DeleteCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Deletes the specified script." } },

                { "scrrename", new(RenameCMD) { MinArgs = 2, MaxArgs = 2,
                    Description =  "Renames the specified script." } },

                { "scrcompileload", new(CompileDirCMD) { MinArgs = 1, MaxArgs = 2,
                    Description = "Compiles all the scripts in a given directory into one command." } },

                { "filecount", new(FileCountCMD) { MaxArgs = 1,
                    Description = "Gets the number of files in a given directory." } },

                { "dirdelete", new(DeleteDirCMD) { MaxArgs = 1,
                    Description = "Deletes the given directory." } },

                { "filedelete", new(DeleteFileCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Deletes the given file." } },

                { "dircreate", new(CreateDirCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Creates a new directory." } },

                { "filecreate", new(CreateFileCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Creates a new file." } },

                { "dirmove", new(MoveDirCMD) { MinArgs = 2, MaxArgs = 2,
                    Description = "Moves the given directory to a new location." } },

                { "filemove", new(MoveFileCMD) { MinArgs = 2, MaxArgs = 2,
                    Description = "Moves the given file to a new location." } },

                { "direxists", new(DirExistsCMD) { MaxArgs = 1,
                    Description = "Outputs whether a given directory exists." } },

                { "fileexists", new(FileExistsCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Outputs whether a given file exists." } },

                { "filecopy", new(CopyFileCMD) { MinArgs = 2, MaxArgs = 2,
                    Description = "Copies a file to a given destination." } },

                { "filewrite", new(WriteFileCMD) { MinArgs = 1, MaxArgs = -1,
                    Description = "Writes to the given file." } },

                { "fileread", new(ReadFileCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Reads the given file." } },

                { "fileview", new(ViewFileCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Outputs the contents of a given file to the console." } },

                { "curd", new(CurDirCMD) { MinArgs = 1, MaxArgs = -1, 
                    Description = "Prepends the current running directory to the given command to %." } },

                { "cd", new(CDAddCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Adds the specified path to the current directory." } },

                { @"cd/", new(CDSetCMD) { MinArgs = 0, MaxArgs = 1,
                    Description = "Sets the specified path to the current directory." } },

                { "cd<", new(Cmd.Translator(CDRemoveCMD, new[] { typeof(int) })) { MinArgs = 0, MaxArgs = 1,
                    Description = "Exits one layer from the current directory." } },
            };
        }

        private void SetDir(string dir, CmdLauncher launcher)
        {
            if (!Directory.Exists(dir))
                throw new CmdException("External", $"Unknown directory \"{dir}\".");
            directory = dir;
            launcher.InputRender = () => $"{directory}>";
        }

        #region Commands

        private string GetDir(string[] args)
        {
            var relPath = args.Length > 0 ? Path.Combine(directory, args[0]) : directory;
            if (!Directory.Exists(relPath))
                throw new CmdException("External", $"Unknown directory \'{relPath}\'.");
            return relPath;
        }

        private void WriteFileCMD(string[] args, Cmd.Callback cb)
        {
            var relPath = Path.Combine(directory, args[0]);
            StringBuilder sb = new();
            for (int i = 1; i < args.Length; ++i)
            {
                if (i != 1)
                    sb.Append(' ');
                sb.Append(args[i]);
            }
            var str = sb.ToString();
            File.WriteAllText(relPath, str);
            cb.Launcher.FeedbackLine($"Successfully wrote {str.Length} characters to:\n{relPath}");
        }

        private void CreateFileCMD(string[] args, Cmd.Callback cb)
        {
            var relPath = Path.Combine(directory, args[0]);
            File.Create(relPath).Close();
            cb.Launcher.FeedbackLine("Successfully created file.");
        }

        private void CreateDirCMD(string[] args, Cmd.Callback cb)
        {
            var relPath = Path.Combine(directory, args[0]);
            Directory.CreateDirectory(relPath);
            cb.Launcher.FeedbackLine("Successfully created directory.");
        }

        private void CopyFileCMD(string[] args, Cmd.Callback cb)
        {
            var path = Path.Combine(directory, args[0]);
            if (!File.Exists(path))
                throw new CmdException("External", $"Unknown file \'{path}\'.");
            var dest = Path.Combine(directory, args[1]);
            File.Copy(path, dest);
            cb.Launcher.FeedbackLine($"Successfully copied file to:\n{dest}");
        }

        private Cmd.MemItem DirExistsCMD(string[] args, Cmd.Callback cb)
        {
            var relPath = GetDir(args);
            bool exists = Directory.Exists(relPath);
            cb.Launcher.FeedbackLine($"Directory does {(exists ? "" : "not")} exist.");
            return new(exists);
        }

        private Cmd.MemItem FileExistsCMD(string[] args, Cmd.Callback cb)
        {
            var relPath = Path.Combine(directory, args[0]);
            bool exists = File.Exists(relPath);
            cb.Launcher.FeedbackLine($"File does {(exists ? "" : "not")} exist.");
            return new(exists);
        }

        private void MoveDirCMD(string[] args, Cmd.Callback cb)
        {
            var path = Path.Combine(directory, args[0]);
            if (!Directory.Exists(path))
                throw new CmdException("External", $"Unknown directory \'{path}\'.");
            var dest = Path.Combine(directory, args[1]);
            Directory.Move(path, dest);
            cb.Launcher.FeedbackLine($"Successfully moved directory to:\n{dest}");
        }

        private void MoveFileCMD(string[] args, Cmd.Callback cb)
        {
            var path = Path.Combine(directory, args[0]);
            if (!File.Exists(path))
                throw new CmdException("External", $"Unknown file \'{path}\'.");
            var dest = Path.Combine(directory, args[1]);
            File.Move(path, dest);
            cb.Launcher.FeedbackLine($"Successfully moved file to:\n{dest}");
        }

        private void DeleteDirCMD(string[] args, Cmd.Callback cb)
        {
            var relPath = GetDir(args);
            Directory.Delete(relPath);
            cb.Launcher.FeedbackLine("Successfully deleted directory.");
        }

        private void DeleteFileCMD(string[] args, Cmd.Callback cb)
        {
            var relPath = Path.Combine(directory, args[0]);
            if (!File.Exists(relPath))
                throw new CmdException("External", $"Unknown file \'{relPath}\'.");
            File.Delete(relPath);
            cb.Launcher.FeedbackLine("Successfully deleted file.");
        }

        private Cmd.MemItem FileCountCMD(string[] args, Cmd.Callback cb)
        {
            var relPath = GetDir(args);
            int count = Directory.GetFiles(relPath).Length;
            if (count == 0)
                cb.Launcher.FeedbackLine("Directory is empty.");
            else
                cb.Launcher.FeedbackLine($"Directory contains {count} file(s).");
            return new(count);
        }

        private void CurDirCMD(string[] args, Cmd.Callback cb)
        {
            for (int i = 1; i < args.Length; ++i)
                args[i] = args[i].Replace("%", AppDomain.CurrentDomain.BaseDirectory);
            cb.Launcher.ExecuteCommand(args[0], ArrUtils.TrimFirst(args));
        }

        private Cmd.MemItem ReadFileCMD(string[] args, Cmd.Callback cb)
        {
            var path = Path.Combine(directory, args[0]);
            if (!File.Exists(path))
                throw new CmdException("External", $"File does not exist \'{path}\'.");
            var lines = File.ReadAllLines(path);
            if (cb.Launcher.CommandFeedback)
            {
                StringBuilder sb = new();
                foreach (var line in lines)
                    sb.AppendLine(line);
                Console.Write(sb.ToString());
            }
            return new(lines);
        }

        private void ViewFileCMD(string[] args)
        {
            var path = Path.Combine(directory, args[0]);
            if (!File.Exists(path))
                throw new CmdException("External", $"File does not exist \'{path}\'.");
            Console.Write(File.ReadAllText(path));
        }

        private void PackageRemoveCMD(string[] args, Cmd.Callback cb)
        {
            int removed = 0;
            foreach (var arg in args)
            {
                string search = arg.ToLower();
                int res = cb.Launcher.Packages.RemoveWhere(p => p.Name.ToLower() == search);
                if (res == 0)
                    StrUtils.PrettyErr("External", $"No packages with name \'{search}\' found.");
                removed += res;
            }
            if (removed > 0)
                cb.Launcher.FeedbackLine($"Sucessfully removed {removed} package(s).");
        }

        private void PackageLoadDirCMD(string[] args, Cmd.Callback cb)
        {
            string path = args.Length > 0 ? Path.Combine(directory, args[0]) : directory;
            int i = 0;
            foreach (var pkg in PkgLoadUtils.DiscoverAllPackages(path))
            {
                cb.Launcher.Packages.Add(pkg);
                ++i;
            }
            cb.Launcher.FeedbackLine($"Sucessfully loaded {i} package(s).");
        }

        private void PackageLoadCMD(string[] args, Cmd.Callback cb)
        {
            string path = Path.Combine(directory, args[0]);
            int i = 0;
            foreach (var pkg in PkgLoadUtils.DiscoverPackages(path))
            {
                cb.Launcher.Packages.Add(pkg);
                ++i;
            }
            cb.Launcher.FeedbackLine($"Sucessfully loaded {i} package(s).");
        }

        private void RenameCMD(string[] args, Cmd.Callback cb)
        {
            if (Commands.ContainsKey(args[1]))
                throw new CmdException("External", $"Script \'{args[1]}\' already exists.");
            if (!Commands.TryGetValue(args[0], out var command))
                throw new CmdException("External", $"Script not found \'{args[0]}\'.");

            Commands.Remove(args[0]);
            Commands[args[1]] = command;
            cb.Launcher.FeedbackLine($"Sucessfully renamed \'{args[0]}\' to \'{args[1]}\'.");
        }

        private void DeleteCMD(string[] args, Cmd.Callback cb)
        {
            if (!Commands.ContainsKey(args[0]))
                throw new CmdException("External", $"Script not found \'{args[0]}\'.");

            Commands.Remove(args[0]);
            cb.Launcher.FeedbackLine($"Sucessfully removed script \'{args[0]}\'.");
        }

        private void LoadAbsolute(string name, string path, Cmd.Callback cb)
        {
            if (Commands.ContainsKey(name))
                throw new CmdException("External", $"Script \'{name}\' already exists.");
            if (!File.Exists(path))
                throw new CmdException("External", $"File not found {path}.");

            var lines = File.ReadAllLines(path);
            Commands.Add(name, new(_ => cb.Launcher.ExecuteEveryCommand(lines)));
            cb.Launcher.FeedbackLine($"Script \'{name}\' added sucessfully!");
        }

        private void LoadCMD(string[] args, Cmd.Callback cb)
        {
            LoadAbsolute(args[0], Path.Combine(directory, args[1]), cb);
        }

        private void RunDirCMD(string[] args, Cmd.Callback cb)
        {
            string relDir = Path.Combine(directory, args[0]);
            if (!Directory.Exists(relDir))
                throw new CmdException("External", $"Unknown directory \'{relDir}\'.");

            foreach (var filePath in Directory.EnumerateFiles(relDir))
                cb.Launcher.ExecuteEveryCommand(File.ReadAllLines(filePath));
        }

        private void LoadDirCMD(string[] args, Cmd.Callback cb)
        {
            string relDir = Path.Combine(directory, args[0]);
            if (!Directory.Exists(relDir))
                throw new CmdException("External", $"Unknown directory \'{relDir}\'.");

            foreach (var filePath in Directory.EnumerateFiles(relDir))
                LoadAbsolute(Path.GetFileNameWithoutExtension(filePath), filePath, cb);
        }

        private void CompileDirCMD(string[] args, Cmd.Callback cb)
        {
            if (Commands.ContainsKey(args[0]))
                throw new CmdException("External", $"Script \'{args[0]}\' already exists.");

            string relDir = args.Length > 1 ? Path.Combine(directory, args[1]) : directory;
            if (!Directory.Exists(relDir))
                throw new CmdException("External", $"Unknown directory \'{relDir}\'.");

            List<string> lines = new();
            foreach (var path in Directory.EnumerateFiles(relDir))
                lines.AddRange(File.ReadAllLines(path));
            Commands.Add(args[0], new(_ => cb.Launcher.ExecuteEveryCommand(lines)));
            cb.Launcher.FeedbackLine($"Script \'{args[0]}\' added sucessfully!");
        }

        private void RunScriptCMD(string[] args, Cmd.Callback cb)
        {
            string relDir = Path.Combine(directory, args[0]);
            if (!File.Exists(relDir))
                throw new CmdException("External", $"File not found {relDir}.");

            var lines = File.ReadAllLines(relDir);
            cb.Launcher.ExecuteEveryCommand(lines);
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
            int index = StrUtils.LastNIndexOf(directory, '\\', n);
            if (index == -1)
                throw new CmdException("External", "No layers to exit from.");
            SetDir(directory[..index], cb.Launcher);
        }

        #endregion
    }
}
