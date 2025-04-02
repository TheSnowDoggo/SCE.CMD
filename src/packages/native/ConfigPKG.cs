namespace SCE
{
    internal class ConfigPKG : Package
    {
        private readonly Config _config;

        private readonly string _path;

        public ConfigPKG(string path)
        {
            _path = path;
            _config = new()
            {
                Values = new()
                {
                    { "CommandFeedback", true },
                    { "ErrorFeedback", true },
                    { "PrettyErros", true },
                },
            };

            if (!_config.CreateAsFile(_path))
                _config.RebuildFromFile(_path);

            Name = "Config";
            Commands = new()
            {
                { "cfgreload", new(ReloadCMD) { MaxArgs = 1, 
                    Description = "Reloads the config." } },
            };
        }

        private void ReloadCMD(string[] args, Cmd.Callback cb)
        {
            var lnch = cb.Launcher;
            lnch.CommandFeedback = _config.GetOrDefault("CommandFeedback", true);
            lnch.ErrorFeedback = _config.GetOrDefault("ErrorFeedback", true);
            lnch.PrettyErrors = _config.GetOrDefault("PrettyErrors", true);

            bool output = true;
            if (args.Length > 0 && !bool.TryParse(args[0], out output))
                throw new CmdException("Config", $"Cannot convert \'{args[0]}\' to bool.");
            if (output)
                lnch.FeedbackLine("Sucessfully reloaded config!");
        }
    }
}
