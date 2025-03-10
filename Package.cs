namespace CMD
{
    public class Package
    {
        public Package(Dictionary<string, Command> commands)
        {
            Commands = commands;
        }

        public Package()
        {
            Commands = new();
        }

        public Dictionary<string, Command> Commands { get; init; }

        public string Name { get; init; } = string.Empty;
    }
}
