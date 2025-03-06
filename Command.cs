namespace CMD
{
    internal class Command
    {
        public Command(Action<string[]> action)
        {
            Action = action;
        }

        public int MinArgs { get; init; }

        public int MaxArgs { get; init; }

        public Action<string[]> Action { get; }
    }
}
