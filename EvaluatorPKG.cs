using SCE;
using System.Text;

namespace CMD
{
    internal class EvaluatorPKG : Package
    {
        private readonly Dictionary<char, double> variables;

        public EvaluatorPKG(int capacity = 0)
        {
            variables = new(capacity);

            Name = "Evaluator";
            Commands = new()
            {
                { "eval", new(EvaluateCMD) { MinArgs = 1, MaxArgs = 3,
                    Description = "Evaluates the result of the given expression." } },
                { "setv", new(Command.Translator(SetVariableCMD, new[] { typeof(char), typeof(double) } ))
                    { MinArgs = 2, MaxArgs = 2, Description = "Assigns a value to the given variable." } },
                { "remv", new(Command.Translator(RemoveVariableCMD, new[] { typeof(char) }))
                    { MinArgs = 1, MaxArgs = 1, Description = "Removes a specified variable." } },
                { "viewv", Command.QCommand<char>(c => Console.WriteLine(variables.TryGetValue(c, out var val) ? $"{c} = {val}" : $"{c} is UNDEFINED")) },
                { "viewvars", new(ViewVariablesCMD) {
                    Description = "Displays all the assigned variables." } },
                { "table", new(Command.Translator(TableCMD(8), new[] { typeof(string), typeof(double), typeof(double), typeof(double) })) {
                    MinArgs = 3, MaxArgs = 4, Description = "Performs a series of evaluations. " } },
                { "insertv", new(InsertCMD) { MinArgs = 1, MaxArgs = -1 } },
                { "clearvars", new(args => variables.Clear()) { Description = "Clears all the variables." } },
            };
        }

        #region CMD

        private string ReplaceVariables(string str)
        {
            StringBuilder sb = new();
            bool special = false;
            foreach (var c in str)
            {
                if (c == '$')
                {
                    special = !special;
                    if (special)
                        continue;
                }

                if (special && c != '$')
                {
                    if (variables.TryGetValue(c, out var value))
                    {
                        sb.Append(value);
                        special = false;
                    }
                    else
                        throw new CommandException("Evaluator", $"Undefined variable {c}.");
                }
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public void InsertCMD(string[] args, Command.Callback cb)
        {
            for (int i = 1; i < args.Length; ++i)
                args[i] = ReplaceVariables(args[i]);
            string name = args[0];
            cb.Launcher.ExecuteCommand(name, ArrayUtils.TrimFirst(args));
        }

        public void ViewVariablesCMD(string[] _)
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

        public void SetVariableCMD(object[] args, Command.Callback cb)
        {
            SetVariable((char)args[0], (double)args[1], cb);
        }

        public void RemoveVariableCMD(object[] args, Command.Callback cb)
        {
            char v = char.ToLower((char)args[0]);
            if (variables.Remove(v))
                cb.Launcher.FeedbackLine($"Successfully removed variable \'{v}\'.");
            else
                StringUtils.PrettyErr("Remove Variable", $"Unknown variable \'{v}\'.");
        }

        public void EvaluateCMD(string[] args, Command.Callback cb)
        {
            if (!RPNConverter.BasicDouble.TryEvaluate(LoadVariables(args[0], variables), out var result))
                throw new CommandException("Evaluator", "Unable to evlaute.");
            if (args.Length == 1)
                Console.WriteLine(result);
            else
            {
                switch (args[1].ToLower())
                {
                    case "as":
                        if (args.Length != 3)
                            throw new CommandException("Evaluator", $"Expected 3 Arguments, received {args.Length}.");
                        if (!char.TryParse(args[2], out var name) && !char.IsLetter(name))
                            throw new CommandException("Evaluator", $"Invalid variable name \"{args[2]}\".");
                        SetVariable(name, result, cb);
                        break;
                    default:
                        throw new CommandException("Evaluator", $"Unknown argument \"{args[1]}\".");
                }
            }
        }

        public Action<object[]> TableCMD(int numLen)
        {
            return args =>
            {
                double step = args.Length >= 4 ? (double)args[3] : 1;
                double start = (double)args[1];
                double end = (double)args[2];

                Dictionary<char, double> dict = new();
                StringBuilder sb = new();
                sb.AppendFormat("{0} : {1}\n", StringUtils.PostFitToLength("x", numLen), args[0]);
                for (double x = start; x < end; x += step)
                {
                    dict['x'] = x;
                    string xStr = x.ToString();
                    sb.AppendFormat("{0} : ", xStr.Length <= numLen ? StringUtils.PostFitToLength(xStr, numLen)
                        : StringUtils.PostFitToLength(xStr, numLen - 2) + "..");
                    if (RPNConverter.BasicDouble.TryEvaluate(LoadVariables((string)args[0], dict), out var result))
                        sb.Append(result);
                    else
                        sb.Append("ERROR");
                    sb.AppendLine();
                }
                Console.Write(sb.ToString());
            };
        }

        #endregion

        private bool SetVariable(char variable, double value, Command.Callback cb)
        {
            if (!char.IsLetter(variable))
                throw new CommandException("Evaluator", "Variable must be a letter from a-z.");

            variable = char.ToLower(variable);
            if (variables.TryGetValue(variable, out var oldVal))
                cb.Launcher.FeedbackLine($"{variable}: {oldVal} -> {value}");
            else
                cb.Launcher.FeedbackLine($"{variable} = {value}");
            variables[variable] = value;

            return true;
        }

        private string LoadVariables(string str, Dictionary<char, double> dict)
        {
            StringBuilder sb = new();
            for (int i = 0; i < str.Length; ++i)
            {
                if (dict.TryGetValue(char.ToLower(str[i]), out var store))
                {
                    if (i > 0 && char.IsLetterOrDigit(str[i - 1]))
                        sb.Append('*');
                    sb.Append(store);
                }
                else if (char.IsLetter(str[i]))
                {
                    throw new CommandException("Evaluator", $"Undefined variable {char.ToLower(str[i])}.");
                }
                else
                {
                    sb.Append(str[i]);
                }
            }
            return sb.ToString();
        }
    }
}
