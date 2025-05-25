namespace SCE
{
    public class Cmd
    {
        public const string BCHAIN = "<CommandName> ?<Arg1>...", MBCHAIN = "?" + BCHAIN;

        // MemItem
        public record MItem(object? Value);

        public Cmd(Func<string[], CmdLauncher, MItem?> func)
        {
            Func = func;
        }

        public Cmd(Func<string[], MItem?> func)
            : this((args, _) => func(args))
        {
        }

        public Cmd(Action<string[], CmdLauncher> action)
            : this((args, cl) => { action(args, cl); return null; })
        {
        }

        public Cmd(Action<string[]> action)
            : this((args, _) => action(args))
        {
        }

        public int Min { get; init; }

        public int Max { get; init; }

        public string Desc { get; init; } = "";

        public string Usage { get; init; } = "";

        public string Version { get; init; } = "";

        public Func<string[], CmdLauncher, MItem?> Func { get; }

        #region Utilites

        public static Cmd QCommand<T>(Action<T, CmdLauncher> action, string description = "")
        {
            return new(Translator((args, cl) => action.Invoke((T)args[0], cl), new[] { typeof(T) }))
            {
                Min = 1,
                Max = 1,
                Desc = description,
            };
        }

        public static Cmd QCommand<T>(Action<T> action, string description = "")
        {
            return QCommand<T>((t, _) => action.Invoke(t), description);
        }

        public static Action<string[], CmdLauncher> Translator(Action<object[], CmdLauncher> action, Type[] types)
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
                            StrUtils.PrettyErr("Translator", $"Failed to convert '{args[i]}' to type {types[i]}.");
                            return;
                        }
                    }
                }
                action.Invoke(arr, callback);
            };
        }

        public static Action<string[], CmdLauncher> Translator(Action<object[]> action, Type[] types)
        {
            return Translator((args, _) => action(args), types);
        }

        #endregion
    }
}
