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

        public Version Version { get; init; } = Version.Zero;

        public string Desc { get; init; } = "";

        public virtual bool IsCompatible(Version version)
        {
            return true;
        }

        public virtual void Initialize(CmdLauncher launcher)
        {
        }
    }
}
