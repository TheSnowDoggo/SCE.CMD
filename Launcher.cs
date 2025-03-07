using SCE;
using System.Text;

namespace CMD
{
    internal static class Launcher
    {
        private static readonly Dictionary<char, double> variables = new();

        internal static void Main()
        {
            CommandLauncher launcher = new()
            {
                Commands = new()
                {
                    { "print", new(Command.Translator(PrintT,
                        new[] { typeof(string), typeof(int), typeof(bool) })) { MinArgs = 1, MaxArgs = 3 } },
                    { "fg", new(SetColor(true)) { MinArgs = 1, MaxArgs = 1 } },
                    { "bg", new(SetColor(false)) { MinArgs = 1, MaxArgs = 1 } },
                    { "clear", new(args => Console.Clear()) },
                    { "cursor", Command.QCommand<bool>(c => Console.CursorVisible = c) },
                    { "eval", new(Evaluate) { MinArgs = 1, MaxArgs = 3 } },
                    { "variables", new(ViewVariables) }
                },
            };

            while (true)
            {
                string input = Console.ReadLine() ?? "";

                string name = ArrayUtils.BuildWhile(input, (c) => c != ' ');
                var args = ArrayUtils.TrimArgs(input);

                launcher.RunCommand(name, args);
            }
        }

        private static Action<string[]> SetColor(bool foreground)
        {
            return args =>
            {
                if (!Enum.TryParse(args[0], true, out ConsoleColor result))
                {
                    StringUtils.PrettyErr("SetColor", $"Invalid color '{args[0]}'.");
                    return;
                }
                if (foreground)
                {
                    Console.ForegroundColor = result;
                    Console.WriteLine($"Foreground set to {result}.");
                }
                else
                {
                    Console.BackgroundColor = result;
                    Console.WriteLine($"Background set to {result}.");
                }
            };
        }

        private static void ViewVariables(string[] args)
        {
            StringBuilder sb = new();
            for (char v = 'a'; v <= 'z'; ++v)
            {
                if (variables.TryGetValue(v, out var store))
                    sb.AppendLine($"{v} = {store}");
                else
                    sb.AppendLine($"{v} is undefined");
            }
            Console.Write(sb.ToString());
        }

        private static string LoadVariables(string str)
        {
            StringBuilder sb = new();
            for (int i = 0; i < str.Length; ++i)
            {
                if (variables.TryGetValue(char.ToLower(str[i]), out var store))
                {
                    if (i > 0 && char.IsLetterOrDigit(str[i - 1]))
                        sb.Append('*');
                    sb.Append(store);
                }
                else if (char.IsLetter(str[i]))
                {
                    StringUtils.PrettyErr("Load Variables", $"Undefined variable {char.ToLower(str[i])}."); 
                }
                else
                {
                    sb.Append(str[i]);
                }
            }
            return sb.ToString();
        }

        private static void Evaluate(object[] args)
        {
            if (!RPNConverter.BasicDouble.TryEvaluate(LoadVariables((string)args[0]), out var result))
            {
                StringUtils.PrettyErr("Evaluate", "Unable to evlaute.");
                return;
            }
            if (args.Length == 1)
                Console.WriteLine(result);
            else
            {
                switch (((string)args[1]).ToLower())
                {
                    case "as":
                        if (args.Length != 3)
                        {
                            StringUtils.PrettyErr("Evaluate", $"Expected 3 Arguments, received {args.Length}.");
                            break;
                        }
                        if (!char.TryParse((string)args[2], out var name) && !char.IsLetter(name))
                        {
                            StringUtils.PrettyErr("Evaluate", $"Invalid variable name \"{args[2]}\".");
                            break;
                        }
                        name = char.ToLower(name);
                        variables[name] = result;
                        Console.WriteLine($"{name} = {result}");
                        break;
                    default:
                        StringUtils.PrettyErr("Evaluate", $"Unknown argument \"{args[1]}\".");
                        break;
                }
            }
        }

        private static void PrintT(object[] args)
        {
            int count = args.Length >= 2 ? (int)args[1] : 1;
            bool newLine = args.Length >= 3 ? (bool)args[2] : true;
            for (int i = 0; i < count; ++i)
            {
                if (newLine || i == count - 1)
                    Console.WriteLine(args[0]);
                else
                    Console.Write(args[0]);
            }
        }
    }
}