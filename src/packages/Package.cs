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

        public PVersion Version { get; init; } = PVersion.Zero;

        public string Desc { get; init; } = "";

        public virtual bool IsCompatible(CmdLauncher launcher)
        {
            return true;
        }

        public virtual void Initialize(CmdLauncher launcher)
        {
        }
    }
}
