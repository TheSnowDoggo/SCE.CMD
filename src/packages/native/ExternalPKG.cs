using CSUtils;
using System.Xml.Linq;

namespace SCE
{
    internal class ExternalPKG : Package
    {
        private string directory = "";

        public ExternalPKG()
        {
            Name = "External";
            Version = new(0, 1, 0);
            Desc = "File and Directory managment. Also provides script and package loading commands.";
            Commands = new()
            {
                { "pkgload", new(PackageLoadCMD) { Min = 1, Max = 1,
                    Desc = "Loads the packages from the specified relative paths.",
                    Usage = "<FilePath>" } },

                { "pkgloaddir", new(PackageLoadDirCMD) { Max = 1,
                    Desc = "Loads the packaes from all the assemblies in the specified relative paths.",
                    Usage = "?<DirPath->cd>" } },

                { "pkgdel", new(PackageRemoveCMD) { Min = 1, Max = -1,
                    Desc = "Removes the specified packages.",
                    Usage = "<Package1>..." } },
                 
                { "scrrun", new(RunScriptCMD) { Min = 1, Max = 1,
                    Desc = "Runs the script from the specified relative path.",
                    Usage = "<FilePath>" } },

                { "scrload", new(LoadCMD) { Min = 2, Max = 4,
                    Desc = "Loads the script from the specified relative path.",
                    Usage = "<ScriptName> <FilePath> ?<Bake:True/False->True> ?<IgnoreOverwrite->False>" } },

                { "scrrundir", new(RunDirCMD) { Max = 1, 
                    Desc = "Runs all the scripts in the given directory.",
                    Usage = "?<DirPath>" } },

                { "scrloaddir", new(LoadDirCMD) { Max = 3,
                    Desc = "Loads all the scripts in the given directory.",
                    Usage = "?<DirPath> ?<Bake:True/False->True> ?<IgnoreOverwrite->False>" } },

                { "scrdel", new(DeleteCMD) { Min = 1, Max = 1,
                    Desc = "Deletes the specified script.",
                    Usage = "<ScriptName>" } },

                { "scrrename", new(RenameCMD) { Min = 2, Max = 2,
                    Desc =  "Renames the specified script.",
                    Usage = "<OldName> <NewName>" } },

                { "scrcompileload", new(CompileDirCMD) { Min = 1, Max = 2,
                    Desc = "Compiles all the scripts in a given directory into one command.",
                    Usage = "<ScriptName> ?<DirPath>" } },

                { "filecount", new(FileCountCMD) { Max = 1,
                    Desc = "Gets the number of files in a given directory.",
                    Usage = "?<DirPath>" } },

                { "dirdelete", new(DeleteDirCMD) { Max = 1,
                    Desc = "Deletes the given directory.",
                    Usage = "?<DirPath>" } },

                { "filedelete", new(DeleteFileCMD) { Min = 1, Max = 1,
                    Desc = "Deletes the given file.",
                    Usage = "<FilePath>" } },

                { "dircreate", new(CreateDirCMD) { Min = 1, Max = 1,
                    Desc = "Creates a new directory.",
                    Usage = "<DirPath>" } },

                { "filecreate", new(CreateFileCMD) { Min = 1, Max = 1,
                    Desc = "Creates a new file.",
                    Usage = "<FilePath>" } },

                { "dirmove", new(MoveDirCMD) { Min = 2, Max = 2,
                    Desc = "Moves the given directory to a new location.",
                    Usage = "<OldDirPath> <NewDirPath>" } },

                { "filemove", new(MoveFileCMD) { Min = 2, Max = 2,
                    Desc = "Moves the given file to a new location.",
                    Usage = "<OldFilePath> <NewFilePath>" } },

                { "direxists", new(DirExistsCMD) { Max = 1,
                    Desc = "Outputs whether a given directory exists.",
                    Usage = "?<DirPath>" } },

                { "fileexists", new(FileExistsCMD) { Min = 1, Max = 1,
                    Desc = "Outputs whether a given file exists.",
                    Usage = "<FilePath>" } },

                { "filecopy", new(CopyFileCMD) { Min = 2, Max = 2,
                    Desc = "Copies a file to a given destination.",
                    Usage = "<FilePath> <NewFilePath>" } },

                { "filewrite", new(WriteFileCMD) { Min = 1, Max = -1,
                    Desc = "Writes to the given file.",
                    Usage = "<FilePath> ?<Write1>..." } },

                { "fileread", new(ReadFileCMD) { Min = 1, Max = 1,
                    Desc = "Reads the given file.",
                    Usage = "<FilePath>" } },

                { "fileview", new(ViewFileCMD) { Min = 1, Max = 1,
                    Desc = "Outputs the contents of a given file to the console.",
                    Usage = "<FilePath>" } },

                { "curd", new(CurDirCMD) { Min = 1, Max = -1, 
                    Desc = "Replaces % to the current running directory of the arguments of the given command.",
                    Usage = Cmd.BCHAIN } },

                { "chod", new(ChoDirCMD) { Min = 1, Max = -1,
                    Desc = "Replaces % to the chosen directory (cd) of the arguments of the given command.",
                    Usage = Cmd.BCHAIN } },

                { "cd", new(CDAddCMD) { Min = 1, Max = 1,
                    Desc = "Adds the specified path to the current directory.",
                    Usage = "<DirPath>" } },

                { @"cd/", new(CDSetCMD) { Max = 1,
                    Desc = "Sets the specified path to the current directory.",
                    Usage = "?<DirPath>" } },

                { "cd<", new(Cmd.Translator(CDRemoveCMD, new[] { typeof(int) })) { Max = 1,
                    Desc = "Exits one layer from the current directory.",
                    Usage = "?<LayerCount>" } },
            };
        }

        private void SetDir(string dir, CmdLauncher launcher)
        {
            dir = Utils.Capitalize(dir.Replace('\\', '/'));
            if (!Directory.Exists(dir))
                throw new CmdException("External", $"Unknown directory \"{dir}\".");
            directory = dir;
            launcher.InputRender = () => $"{Path.TrimEndingDirectorySeparator(directory)}>";
        }

        #region Commands

        private string GetDir(string[] args)
        {
            var relPath = args.Length > 0 ? Path.Combine(directory, args[0]) : directory;
            if (!Directory.Exists(relPath))
                throw new CmdException("External", $"Unknown directory \'{relPath}\'.");
            return relPath;
        }

        private void WriteFileCMD(string[] args, CmdLauncher cl)
        {
            var relPath = Path.Combine(directory, args[0]);
            var str = Utils.Infill(Utils.TrimFirst(args), " ");
            File.WriteAllText(relPath, str);
            cl.FeedbackLine($"Successfully wrote {str.Length} characters to:\n{relPath}");
        }

        private void CreateFileCMD(string[] args, CmdLauncher cl)
        {
            var relPath = Path.Combine(directory, args[0]);
            File.Create(relPath).Close();
            cl.FeedbackLine("Successfully created file.");
        }

        private void CreateDirCMD(string[] args, CmdLauncher cl)
        {
            var relPath = Path.Combine(directory, args[0]);
            Directory.CreateDirectory(relPath);
            cl.FeedbackLine("Successfully created directory.");
        }

        private void CopyFileCMD(string[] args, CmdLauncher cl)
        {
            var path = Path.Combine(directory, args[0]);
            if (!File.Exists(path))
                throw new CmdException("External", $"Unknown file \'{path}\'.");
            var dest = Path.Combine(directory, args[1]);
            File.Copy(path, dest);
            cl.FeedbackLine($"Successfully copied file to:\n{dest}");
        }

        private Cmd.MItem DirExistsCMD(string[] args, CmdLauncher cl)
        {
            var relPath = GetDir(args);
            bool exists = Directory.Exists(relPath);
            cl.FeedbackLine($"Directory does {(exists ? "" : "not")} exist.");
            return new(exists);
        }

        private Cmd.MItem FileExistsCMD(string[] args, CmdLauncher cl)
        {
            var relPath = Path.Combine(directory, args[0]);
            bool exists = File.Exists(relPath);
            cl.FeedbackLine($"File does {(exists ? "" : "not")} exist.");
            return new(exists);
        }

        private void MoveDirCMD(string[] args, CmdLauncher cl)
        {
            var path = Path.Combine(directory, args[0]);
            if (!Directory.Exists(path))
                throw new CmdException("External", $"Unknown directory \'{path}\'.");
            var dest = Path.Combine(directory, args[1]);
            Directory.Move(path, dest);
            cl.FeedbackLine($"Successfully moved directory to:\n{dest}");
        }

        private void MoveFileCMD(string[] args, CmdLauncher cl)
        {
            var path = Path.Combine(directory, args[0]);
            if (!File.Exists(path))
                throw new CmdException("External", $"Unknown file \'{path}\'.");
            var dest = Path.Combine(directory, args[1]);
            File.Move(path, dest);
            cl.FeedbackLine($"Successfully moved file to:\n{dest}");
        }

        private void DeleteDirCMD(string[] args, CmdLauncher cl)
        {
            var relPath = GetDir(args);
            Directory.Delete(relPath);
            cl.FeedbackLine("Successfully deleted directory.");
        }

        private void DeleteFileCMD(string[] args, CmdLauncher cl)
        {
            var relPath = Path.Combine(directory, args[0]);
            if (!File.Exists(relPath))
                throw new CmdException("External", $"Unknown file \'{relPath}\'.");
            File.Delete(relPath);
            cl.FeedbackLine("Successfully deleted file.");
        }

        private Cmd.MItem FileCountCMD(string[] args, CmdLauncher cl)
        {
            var relPath = GetDir(args);
            int count = Directory.GetFiles(relPath).Length;
            if (count == 0)
                cl.FeedbackLine("Directory is empty.");
            else
                cl.FeedbackLine($"Directory contains {count} file(s).");
            return new(count);
        }

        private void CurDirCMD(string[] args, CmdLauncher cl)
        {
            for (int i = 1; i < args.Length; ++i)
                args[i] = args[i].Replace("%", AppDomain.CurrentDomain.BaseDirectory);
            cl.ExecuteCommand(args[0], Utils.TrimFirst(args));
        }

        private void ChoDirCMD(string[] args, CmdLauncher cl)
        {
            for (int i = 1; i < args.Length; ++i)
                args[i] = args[i].Replace("%", directory);
            cl.ExecuteCommand(args[0], Utils.TrimFirst(args));
        }

        private Cmd.MItem ReadFileCMD(string[] args, CmdLauncher cl)
        {
            var path = Path.Combine(directory, args[0]);
            if (!File.Exists(path))
                throw new CmdException("External", $"File does not exist \'{path}\'.");
            var lines = File.ReadAllLines(path);
            if (cl.CmdFeedback)
                Console.WriteLine(Utils.Infill(lines, "\n"));
            return new(lines);
        }

        private void ViewFileCMD(string[] args)
        {
            var path = Path.Combine(directory, args[0]);
            if (!File.Exists(path))
                throw new CmdException("External", $"File does not exist \'{path}\'.");
            Console.Write(File.ReadAllText(path));
        }

        private void PackageRemoveCMD(string[] args, CmdLauncher cl)
        {
            int count = 0;
            foreach (var arg in args)
            {
                var name = arg.ToLower();
                if (!cl.RemovePackage(name))
                    StrUtils.PrettyErr("External", $"Package \'{name}\' not found.");
                else
                    ++count;
            }
            if (count == 0)
                throw new CmdException("External", "No package found.");
            cl.FeedbackLine($"Successfully removed {count} package(s).");
        }

        private void PackageLoadDirCMD(string[] args, CmdLauncher cl)
        {
            var path = args.Length > 0 ? Path.Combine(directory, args[0]) : directory;
            int count = 0;
            foreach (var pkg in PkgUtils.DiscoverAllPackages(path))
            {
                cl.LoadPackage(pkg);
                ++count;
            }
            cl.FeedbackLine($"Successfully loaded {count} package(s).");
        }

        private void PackageLoadCMD(string[] args, CmdLauncher cl)
        {
            var path = Path.Combine(directory, args[0]);
            int count = 0;
            foreach (var pkg in PkgUtils.DiscoverPackages(path))
            {
                cl.LoadPackage(pkg);
                ++count;
            }
            cl.FeedbackLine($"Successfully loaded {count} package(s).");
        }

        private void RenameCMD(string[] args, CmdLauncher cl)
        {
            if (!Commands.TryGetValue(args[0], out var command))
                throw new CmdException("External", $"Script not found \'{args[0]}\'.");
            if (Commands.ContainsKey(args[1]))
            {
                StrUtils.PrettyErr("External", $"Script \'{args[1]}\' already exists.");
                if (!Utils.BoolPrompt("Would you like to overwrite it? Yes[Y] or No[N]: "))
                    return;
            }

            Commands.Remove(args[0]);
            Commands[args[1]] = command;
            cl.FeedbackLine($"Sucessfully renamed \'{args[0]}\' to \'{args[1]}\'.");
        }

        private void DeleteCMD(string[] args, CmdLauncher cl)
        {
            if (!Commands.ContainsKey(args[0]))
                throw new CmdException("External", $"Script not found \'{args[0]}\'.");

            Commands.Remove(args[0]);
            cl.FeedbackLine($"Sucessfully removed script \'{args[0]}\'.");
        }

        private void LoadAbsolute(string name, string path, CmdLauncher cl, bool bake = true, bool ignoreOverwrite = false)
        {
            if (!File.Exists(path))
                throw new CmdException("External", $"File not found {path}.");
            if (!ignoreOverwrite && Commands.ContainsKey(name))
            {
                StrUtils.PrettyErr("External", $"Script \'{name}\' already exists.");
                if (!Utils.BoolPrompt("Would you like to overwrite it? Yes[Y] or No[N]: "))
                    return;
            }

            if (!bake)
                Commands[name] = new(_ => cl.ExecuteEveryCommand(File.ReadAllLines(path)));
            else
            {
                var lines = File.ReadAllLines(path);
                Commands[name] = new(_ => cl.ExecuteEveryCommand(lines));
            }
            cl.FeedbackLine($"Script \'{name}\' added sucessfully!");
        }

        private void LoadCMD(string[] args, CmdLauncher cl)
        {
            bool bake = true;
            if (args.Length >= 3 && !bool.TryParse(args[2], out bake))
                throw new CmdException("External", $"Invalid bool \'{args[2]}\'.");
            bool ignoreOverwrite = false;
            if (args.Length >= 4 && !bool.TryParse(args[3], out ignoreOverwrite))
                throw new CmdException("External", $"Invalid bool \'{args[3]}\'.");
            LoadAbsolute(args[0], Path.Combine(directory, args[1]), cl, ignoreOverwrite, bake);
        }

        private void LoadDirCMD(string[] args, CmdLauncher cl)
        {
            bool bake = true;
            if (args.Length >= 2 && !bool.TryParse(args[1], out bake))
                throw new CmdException("External", $"Invalid bool \'{args[1]}\'.");
            bool ignoreOverwrite = false;
            if (args.Length >= 3 && !bool.TryParse(args[2], out ignoreOverwrite))
                throw new CmdException("External", $"Invalid bool \'{args[2]}\'.");         

            string relDir = Path.Combine(directory, args[0]);
            if (!Directory.Exists(relDir))
                throw new CmdException("External", $"Unknown directory \'{relDir}\'.");

            foreach (var filePath in Directory.EnumerateFiles(relDir))
                LoadAbsolute(Path.GetFileNameWithoutExtension(filePath), filePath, cl, bake, ignoreOverwrite, );
        }

        private void RunDirCMD(string[] args, CmdLauncher cl)
        {
            string relDir = Path.Combine(directory, args[0]);
            if (!Directory.Exists(relDir))
                throw new CmdException("External", $"Unknown directory \'{relDir}\'.");

            foreach (var filePath in Directory.EnumerateFiles(relDir))
                cl.ExecuteEveryCommand(File.ReadAllLines(filePath));
        }

        private void CompileDirCMD(string[] args, CmdLauncher cl)
        {
            if (Commands.ContainsKey(args[0]))
                throw new CmdException("External", $"Script \'{args[0]}\' already exists.");

            string relDir = args.Length > 1 ? Path.Combine(directory, args[1]) : directory;
            if (!Directory.Exists(relDir))
                throw new CmdException("External", $"Unknown directory \'{relDir}\'.");

            List<string> lines = new();
            foreach (var path in Directory.EnumerateFiles(relDir))
                lines.AddRange(File.ReadAllLines(path));
            Commands.Add(args[0], new(_ => cl.ExecuteEveryCommand(lines)));
            cl.FeedbackLine($"Script \'{args[0]}\' added sucessfully!");
        }

        private void RunScriptCMD(string[] args, CmdLauncher cl)
        {
            string relDir = Path.Combine(directory, args[0]);
            if (!File.Exists(relDir))
                throw new CmdException("External", $"File not found {relDir}.");

            var lines = File.ReadAllLines(relDir);
            cl.ExecuteEveryCommand(lines);
        }

        private void CDAddCMD(string[] args, CmdLauncher cl)
        {
            SetDir(Path.Combine(directory, args[0]), cl);
        }

        private void CDSetCMD(string[] args, CmdLauncher cl)
        {
            if (args.Length > 0)
                SetDir(args[0], cl);
            else
            {
                directory = "";
                cl.InputRender = null;
            }
        }

        private void CDRemoveCMD(object[] args, CmdLauncher cl)
        {
            int n = args.Length > 0 ? (int)args[0] : 1;
            int index = StrUtils.LastNIndexOf(directory, '\\', n);
            if (index == -1)
                throw new CmdException("External", "No layers to exit from.");
            SetDir(directory[..index], cl);
        }

        #endregion
    }
}
