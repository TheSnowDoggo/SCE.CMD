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

        public static Command QCommand<T>(Action<T> action)
        {
            return new(Translator(oargs => action((T)oargs[0]), new[] { typeof(T) })) { MinArgs = 1, MaxArgs = 1 };
        }

        public static Action<string[]> Translator(Action<object[]> action, Type[] types)
        {
            return args =>
            {
                var arr = new object[args.Length];
                for (int i = 0; i < args.Length; ++i)
                {
                    if (i >= types.Length || types[i] == typeof(string))
                        arr[i] = args[i];
                    else
                    {
                        try
                        {
                            arr[i] = Convert.ChangeType(args[i], types[i]);
                        }
                        catch
                        {
                            StringUtils.PrettyErr("Translator", $"Failed to convert '{args[i]}' to type {types[i]}.");
                            return;
                        }
                    }
                }
                action(arr);
            };
        }
    }
}
