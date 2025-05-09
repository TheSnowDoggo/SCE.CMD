namespace SCE
{
    public class Package
    {
        public Package(Dictionary<string, Cmd> commands)
        {
            Commands = commands;
        }

        public Package()
        {
            Commands = new();
        }

        public Dictionary<string, Cmd> Commands { get; init; }

        public string Name { get; init; } = "UNNAMED";

        public string Version { get; init; } = "UNDEFINED";

        public virtual void Initialize(CmdLauncher launcher)
        {
        }
    }
}
