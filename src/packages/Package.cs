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

        public string Name { get; init; } = string.Empty;
    }
}
