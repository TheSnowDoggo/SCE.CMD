namespace CMD
{
    public class Command
    {
        public record Callback(Package Package, CommandLauncher Launcher);

        public Command(Func<string[], Callback, MemoryItem?> func)
        {
            Func = func;
        }

        public Command(Func<string[], MemoryItem?> func)
            : this((args, _) => func(args))
        {
        }

        public Command(Action<string[], Callback> action)
            : this((args, cb) => { action(args, cb); return null; })
        {
        }

        public Command(Action<string[]> action)
            : this((args, _) => action(args))
        {
        }

        public int MinArgs { get; init; }

        public int MaxArgs { get; init; }

        public string Description { get; init; } = "";

        public Func<string[], Callback, MemoryItem?> Func { get; }

        public static Command QCommand<T>(Action<T, Callback> action, string description = "")
        {
            return new(Translator((args, cb) => action.Invoke((T)args[0], cb), new[] { typeof(T) }))
            {
                MinArgs = 1,
                MaxArgs = 1,
                Description = description,
            };
        }

        public static Command QCommand<T>(Action<T> action, string description = "")
        {
            return QCommand<T>((t, _) => action.Invoke(t), description);
        }

        public static Action<string[], Callback> Translator(Action<object[], Callback> action, Type[] types)
        {
            return (args, callback) =>
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
                action.Invoke(arr, callback);
            };
        }

        public static Action<string[], Callback> Translator(Action<object[]> action, Type[] types)
        {
            return Translator((args, _) => action(args), types);
        }
    }
}
