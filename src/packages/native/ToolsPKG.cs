using System.Text;

namespace SCE
{
    internal class ToolsPKG : Package
    {
        private readonly Random _rand = new();

        public ToolsPKG()
        {
            Name = "Tools";
            Version = "0.0.0";
            Commands = new()
            {
                { "roll", new(DiceCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Rolls a dice given the number of sides.",
                    Usage = "<Sides>" } },

                { "fib", new(FibCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Calculates fibonaci numbers.",
                    Usage = "<UpTo>" } },
            };
        }

        private Cmd.MemItem DiceCMD(string[] args, Cmd.Callback cb)
        {
            if (!int.TryParse(args[0], out var d) || d <= 0)
                throw new CmdException("Tools", $"Invalid sides \'{args[0]}\'.");
            var roll = _rand.Next(d) + 1;
            cb.Launcher.FeedbackLine(roll);
            return new(roll);
        }

        private Cmd.MemItem FibCMD(string[] args, Cmd.Callback cb)
        {
            if (!int.TryParse(args[0], out var n) || n < 1)
                throw new CmdException("Tools", $"Invalid max \'{args[0]}\'.");
            var arr = new int[n];
            if (arr.Length > 1)
            {
                arr[1] = 1;
                for (int i = 2; i < n; ++i)
                    arr[i] = arr[i - 1] + arr[i - 2];
            }
            if (cb.Launcher.CommandFeedback)
            {
                StringBuilder sb = new();
                bool first = true;
                for (int i = 0; i < arr.Length; ++i)
                {
                    if (!first)
                        sb.Append(',');
                    first = false;
                    sb.Append(arr[i]);
                }
                cb.Launcher.FeedbackLine(sb.ToString());
            }
            return new(arr);
        }
    }
}
