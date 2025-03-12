using SCE;
using System.Text;

namespace SCE
{
    internal class EvaluatorPKG : Package
    {
        private readonly Dictionary<char, double> _variables;

        public EvaluatorPKG(int capacity = 0)
        {
            _variables = new(capacity);

            Name = "Evaluator";
            Commands = new()
            {
                { "eval", new(EvaluateCMD) { MinArgs = 1, MaxArgs = 3,
                    Description = "Evaluates the result of the given expression." } },

                { "round", new(RoundCMD) { MinArgs = 1, MaxArgs = 2,
                    Description = "Rounds the number." } },

                { "vset", new(Cmd.Translator(SetVariableCMD, new[] { typeof(char), typeof(double) } ))
                    { MinArgs = 2, MaxArgs = 2, Description = "Assigns a value to the given variable." } },

                { "vrem", new(Cmd.Translator(RemoveVariableCMD, new[] { typeof(char) }))
                    { MinArgs = 1, MaxArgs = 1, Description = "Removes a specified variable." } },

                { "vview", Cmd.QCommand<char>(c => Console.WriteLine(_variables.TryGetValue(c, out var val) ? $"{c} = {val}" : $"{c} is UNDEFINED")) },

                { "varsview", new(ViewVariablesCMD) {
                    Description = "Displays all the assigned variables." } },

                { "vload", new(VarLoadCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Loads the given variable into memory." } },

                { "table", new(Cmd.Translator(TableCMD(8), new[] { typeof(string), typeof(double), typeof(double), typeof(double) })) {
                    MinArgs = 3, MaxArgs = 4, Description = "Performs a series of evaluations. " } },

                { "vinsert", new(InsertCMD) { MinArgs = 1, MaxArgs = -1 } },

                { "varsclear", new(args => _variables.Clear()) { Description = "Clears all the variables." } },
            };
        }

        #region CMD

        private Cmd.MemItem RoundCMD(string[] args, Cmd.Callback cb)
        {
            if (!double.TryParse(args[0], out var value))
                throw new CmdException("Evaluator", $"Unable to convert \'{args[0]}\' to double.");
            int digits = 0;
            if (args.Length > 1 && !int.TryParse(args[1], out digits))
                throw new CmdException("Evaluator", $"Invalid digits \'{args[1]}\'.");
            var result = Math.Round(value, digits);
            cb.Launcher.FeedbackLine(result);
            return new(result);
        }

        private void VarLoadCMD(string[] args, Cmd.Callback cb)
        {
            if (!char.TryParse(args[0], out char c) || !_variables.TryGetValue(c, out var value))
                throw new CmdException("Evaluator", $"Undefined variable {c}.");
            cb.Launcher.MemoryStack.Push(value);
        }

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
                    if (_variables.TryGetValue(c, out var value))
                    {
                        sb.Append(value);
                        special = false;
                    }
                    else
                        throw new CmdException("Evaluator", $"Undefined variable {c}.");
                }
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public void InsertCMD(string[] args, Cmd.Callback cb)
        {
            for (int i = 1; i < args.Length; ++i)
                args[i] = ReplaceVariables(args[i]);
            string name = args[0];
            cb.Launcher.ExecuteCommand(name, ArrUtils.TrimFirst(args));
        }

        public void ViewVariablesCMD(string[] _)
        {
            if (_variables.Count == 0)
            {
                Console.WriteLine("No variables defined.");
                return;
            }
            StringBuilder sb = new();
            foreach (var item in
                from pair in _variables
                orderby pair.Key
                select pair)
            {
                sb.AppendLine($"{item.Key} = {item.Value}");
            }
            Console.Write(sb.ToString());
        }

        public void SetVariableCMD(object[] args, Cmd.Callback cb)
        {
            SetVariable((char)args[0], (double)args[1], cb);
        }

        public void RemoveVariableCMD(object[] args, Cmd.Callback cb)
        {
            char v = char.ToLower((char)args[0]);
            if (_variables.Remove(v))
                cb.Launcher.FeedbackLine($"Successfully removed variable \'{v}\'.");
            else
                StrUtils.PrettyErr("Remove Variable", $"Unknown variable \'{v}\'.");
        }

        public Cmd.MemItem? EvaluateCMD(string[] args, Cmd.Callback cb)
        {
            if (!RPNConverter.BasicDouble.TryEvaluate(LoadVariables(args[0], _variables), out var result))
                throw new CmdException("Evaluator", "Unable to evlaute.");
            if (args.Length == 1)
                cb.Launcher.FeedbackLine(result);
            else
            {
                switch (args[1].ToLower())
                {
                    case "as":
                        if (args.Length != 3)
                            throw new CmdException("Evaluator", $"Expected 3 Arguments, received {args.Length}.");
                        if (!char.TryParse(args[2], out var name) && !char.IsLetter(name))
                            throw new CmdException("Evaluator", $"Invalid variable name \"{args[2]}\".");
                        SetVariable(name, result, cb);
                        break;
                    default:
                        throw new CmdException("Evaluator", $"Unknown argument \"{args[1]}\".");
                }
            }
            return new(result);
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
                sb.AppendFormat("{0} : {1}\n", StrUtils.PostFitToLength("x", numLen), args[0]);
                for (double x = start; x < end; x += step)
                {
                    dict['x'] = x;
                    string xStr = x.ToString();
                    sb.AppendFormat("{0} : ", xStr.Length <= numLen ? StrUtils.PostFitToLength(xStr, numLen)
                        : StrUtils.PostFitToLength(xStr, numLen - 2) + "..");
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

        private bool SetVariable(char variable, double value, Cmd.Callback cb)
        {
            if (!char.IsLetter(variable))
                throw new CmdException("Evaluator", "Variable must be a letter from a-z.");

            variable = char.ToLower(variable);
            if (_variables.TryGetValue(variable, out var oldVal))
                cb.Launcher.FeedbackLine($"{variable}: {oldVal} -> {value}");
            else
                cb.Launcher.FeedbackLine($"{variable} = {value}");
            _variables[variable] = value;

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
                    throw new CmdException("Evaluator", $"Undefined variable {char.ToLower(str[i])}.");
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
