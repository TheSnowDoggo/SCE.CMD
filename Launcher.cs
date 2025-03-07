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
                    { "print", new(Command.Translator(PrintT, new[] { typeof(string), typeof(int), typeof(bool) })) 
                        { MinArgs = 1, MaxArgs = 3, Description = "Prints the string a given amount of times." } },

                    { "fg", new(SetColor(true)) { MinArgs = 1, MaxArgs = 1,
                        Description = "Sets the Foreground color of the Console." } },

                    { "bg", new(SetColor(false)) { MinArgs = 1, MaxArgs = 1,
                        Description = "Sets the Background color of the Console." } },

                    { "clear", new(args => Console.Clear()) { 
                        Description = "Clears the Console." } },

                    { "cursor", Command.QCommand<bool>(c => Console.CursorVisible = c, 
                        "Sets the visible state of the Cursor.") },

                    { "eval", new(Evaluate) { MinArgs = 1, MaxArgs = 3, 
                        Description = "Evaluates the result of the given expression." } },

                    { "setvar", new(Command.Translator(SetVar, new[] { typeof(char), typeof(double) } ))
                        { MinArgs = 2, MaxArgs = 2, Description = "Assigns a value to the given variable." } },

                    { "variables", new(ViewVariables) { 
                        Description = "Displays all the assigned variables." } },
                },
            };

            launcher.Commands.Add("help", Help(launcher.Commands));



            while (true)
            {
                launcher.RunCommand(Console.ReadLine() ?? "");
            }
        }

        private static bool SetVariable(char variable, double value)
        {
            if (!char.IsLetter(variable))
            {
                StringUtils.PrettyErr("SetVariable", "Variable must be a letter from a-z.");
                return false;
            }

            variable = char.ToLower(variable);
            if (variables.TryGetValue(variable, out var oldVal))
                Console.WriteLine($"{variable}: {oldVal} -> {value}");
            else
                Console.WriteLine($"{variable} = {value}");
            variables[variable] = value;

            return true;
        }

        private static void SetVar(object[] oargs)
        {
            SetVariable((char)oargs[0], (double)oargs[1]);
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

        private static Command Help(Dictionary<string, Command> commands)
        {
            return new(args =>
            {
                StringBuilder sb = new("- Commands -\n");
                foreach (var item in commands)
                {
                    var command = item.Value;
                    sb.AppendLine($"{item.Key}[{command.MinArgs}-{command.MaxArgs}]");
                    if (command.Description != string.Empty)
                        sb.AppendLine($"> {command.Description}");
                    sb.AppendLine();
                }
                Console.Write(sb.ToString());
            })
            {
                Description = "Displays every command.",
            };
        }

        private static void ViewVariables(string[] args)
        {
            if (variables.Count == 0)
            {
                Console.WriteLine("No variables defined.");
                return;
            }
            StringBuilder sb = new();
            foreach (var item in 
                from pair in variables
                orderby pair.Key
                select pair)
            {
                sb.AppendLine($"{item.Key} = {item.Value}");
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
                        SetVariable(name, result);
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