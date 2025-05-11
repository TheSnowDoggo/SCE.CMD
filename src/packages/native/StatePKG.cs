using CSUtils;
namespace SCE
{
    internal class StatePKG : Package
    {
        private const string IFA_USG = "<True/False>:<Num;!=0> " + Cmd.BCHAIN,
            CMP_USG = "<Left> <Right> ?<BothType>:<LeftType,RightType>";

        public StatePKG()
        {
            Name = "State";
            Version = new(0, 3, 1);
            Desc = "Conditional and State manipulation utilities.";
            Commands = new()
            {
                { "while^", WhileGEN(true) },

                { "dowhile^", WhileGEN(false) },

                { "if*", new(IfArgGEN(false)) { Min = 2, Max = -1,
                    Desc = "Runs the command if the argument condition is true.",
                    Usage = IFA_USG } },

                { "!if*", new(IfArgGEN(true)) { Min = 2, Max = -1,
                    Desc = "Runs the command if the argument condition is false.",
                    Usage = IFA_USG } },

                { "elif*", new(ElseIfArgCMD) { Min = 3, Max = 3,
                    Desc = "Runs left cmd if argument condition true; right cmd.",
                    Usage = IFA_USG } },

                { "if^", new(IfGEN(true, false)) { Min = 1, Max = -1,
                    Desc = "Pops the last memory item, runs command if true.",
                    Usage = Cmd.BCHAIN} },

                { "!if^", new(IfGEN(true, true)) { Min = 1, Max = -1,
                    Desc = "Pops the last memory item, runs command if false.",
                    Usage = Cmd.BCHAIN} },

                 { "if", new(IfGEN(false, false)) { Min = 1, Max = -1,
                    Desc = "Peeks the last memory item, runs command if true.",
                    Usage = Cmd.BCHAIN} },

                { "!if", new(IfGEN(false, true)) { Min = 1, Max = -1,
                    Desc = "Peeks the last memory item, runs command if false.",
                    Usage = Cmd.BCHAIN} },

                { "elif", new(ElseIfGEN(false))  { Min = 2, Max = 2,
                    Desc = "Peek | Runs left cmd if last mem item true; right cmd.",
                    Usage = "<Command1> <Command2>"} },

                { "elif^", new(ElseIfGEN(true))  { Min = 2, Max = 2,
                    Desc = "Pop | Runs left cmd if last mem item true; right cmd.",
                    Usage = "<Command1> <Command2>"} },

                { "eql", new(EqualsCMD) { Min = 2, Max = 3,
                    Desc = "Outputs whether the given arguments are equal.",
                    Usage = CMP_USG } },

                { "!eql", new(EqualsNotCMD) { Min = 2, Max = 3,
                    Desc = "Outputs whether the given arguments are not equal.",
                    Usage = CMP_USG } },

                { "not", new(NotGEN(false)) { Max = -1,
                    Desc = "Peek | Performs OR operatoin on the last item in memory after running the given command.",
                    Usage = Cmd.MBCHAIN } },

                { "not^", new(NotGEN(true)) { Max = -1,
                    Desc = "Pop | Performs OR operatoin on the last item in memory after running the given command.",
                    Usage = Cmd.MBCHAIN } },

                { "and^", new(AndCMD) { Max = -1,
                    Desc = "Performs AND operation on the last 2 items in memory after running the given command.",
                    Usage = Cmd.MBCHAIN } },

                { "or^", new(OrCMD) { Max = -1,
                    Desc = "Performs OR operation on the last 2 items in memory after running the given command.",
                    Usage = Cmd.MBCHAIN } },

                { "not*", new(NotArgCMD) { Min = 1, Max = 1,
                    Desc = "Nots the given boolean.",
                    Usage = "<Boolean>" } },

                { "and*", new(AndArgCMD) { Min = 2, Max = 2,
                    Desc = "Ands the given booleans.",
                    Usage = "<Boolean1> <Boolean2>" } },

                { "or*", new(OrArgCMD) { Min = 2, Max = 2,
                    Desc = "Ors the given booleans.",
                    Usage = "<Boolean1> <Boolean2>" } },

                { "cmp=", new(CmpGEN(x => x == 0)) { Min = 2, Max = 3,
                    Desc = "Outputs whether left = right.",
                    Usage = CMP_USG } },

                { "cmp!=", new(CmpGEN(x => x != 0)) { Min = 2, Max = 3,
                    Desc = "Outputs whether left != right.",
                    Usage = CMP_USG } },

                { "cmp<", new(CmpGEN(x => x < 0)) { Min = 2, Max = 3,
                    Desc = "Outputs whether left < right.",
                    Usage = CMP_USG } },

                { "cmp>", new(CmpGEN(x => x > 0)) { Min = 2, Max = 3,
                    Desc = "Outputs whether left > right.",
                    Usage = CMP_USG } },

                { "cmp<=", new(CmpGEN(x => x <= 0)) { Min = 2, Max = 3,
                    Desc = "Outputs whether left <= right.",
                    Usage = CMP_USG } },

                { "cmp>=", new(CmpGEN(x => x >= 0)) { Min = 2, Max = 3,
                    Desc = "Outputs whether left >= right.",
                    Usage = CMP_USG } },

                { "istype*", new(IsTypeArgCMD) { Min = 2, Max = 2,
                    Desc = "Determines if the given argument is a type.",
                    Usage = "<Check> <Type>" } },

                { "istype", new(IsTypeCMD) { Min = 1, Max = 1,
                    Desc = "Determines if the last mem item is a type.",
                    Usage = "<Type>" } },
            };
            AddStateGEN(cl => cl.CmdFeedback, (cl, c) => cl.CmdFeedback = c, "feed", "command feedback");
            AddStateGEN(cl => cl.ErrFeedback, (cl, c) => cl.ErrFeedback = c, "efeed", "error feedback");
            AddStateGEN(cl => cl.NeatErrors, (cl, c) => cl.NeatErrors = c, "neaterr", "neat errors");
            AddStateGEN(cl => cl.MemLock, (cl, c) => cl.MemLock = c, "memlock", "memory lock");
            AddStateGEN(cl => cl.CmdCaching, (cl, c) => cl.CmdCaching = c, "cache", "command caching");
            AddStateGEN(cl => cl.Preprocessing, (cl, c) => cl.Preprocessing = c, "prp", "preprocessing");
            AddStateGEN(cl => cl.InputRendering, (cl, c) => cl.InputRendering = c, "irdr", "input rendering");
        }

        #region ConditionCommands

        private static bool Condition(string input)
        {
            if (bool.TryParse(input, out var result))
                return result;
            if (int.TryParse(input, out var num))
                return num != 0;
            throw new CmdException("Launcher", $"Invalid conditional \'{input}\'.");
        }

        private static bool MemBool(CmdLauncher cl, bool pop = true)
        {
            var obj = NativePKG.MemObj(cl, pop);
            if (obj is bool c)
                return c;
            var str = obj.ToString() ??
                throw new CmdException("Native", "Last memory item was not a valid boolean.");
            return !Condition(str);
        }

        private static Cmd WhileGEN(bool onStart)
        {
            return new((args, cl) =>
            {
                bool first = true;
                while ((!onStart && first) || MemBool(cl))
                {
                    if (!cl.SExecuteCommand(args[0], Utils.TrimFirst(args)))
                        throw new CmdException("Launcher", "Loop ended as command failed to execute.");
                    first = false;
                }
            })
            {
                Min = 1, Max = -1,
                Desc = "Runs the command while the last mem item is true.",
                Usage = Cmd.BCHAIN
            };
        }

        private static Action<string[], CmdLauncher> IfArgGEN(bool invert)
        {
            return (args, cl) =>
            {
                if (invert ? !Condition(args[0]) : Condition(args[0]))
                    cl.ExecuteCommand(args[1], Utils.TrimFromStart(args, 2));
            };
        }

        private static void ElseIfArgCMD(string[] args, CmdLauncher cl)
        {
            cl.ExecuteCommand(Condition(args[0]) ? args[1] : args[2]);
        }

        private static Action<string[], CmdLauncher> IfGEN(bool pop, bool invert)
        {
            return (args, cl) =>
            {
                if (invert ? !MemBool(cl, pop) : MemBool(cl, pop))
                    cl.ExecuteCommand(args[0], Utils.TrimFirst(args));
            };
        }

        private static Action<string[], CmdLauncher> ElseIfGEN(bool pop)
        {
            return (args, cl) =>
            {
                cl.ExecuteCommand(MemBool(cl, pop) ? args[0] : args[1]);
            };
        }

        private static (object, object) GetAsTypes(string[] args)
        {
            if (args.Length == 2)
                return (args[0], args[1]);
            var types = args[2].Split(',');
            object o1, o2;
            if (types.Length == 1)
            {
                var t = StrUtils.BetterGetType(types[0]);
                o1 = Convert.ChangeType(args[0], t);
                o2 = Convert.ChangeType(args[1], t);
            }
            else if (types.Length == 2)
            {
                var t1 = StrUtils.BetterGetType(types[0]);
                var t2 = StrUtils.BetterGetType(types[1]);
                o1 = Convert.ChangeType(args[0], t1);
                o2 = Convert.ChangeType(args[1], t2);
            }
            else
            {
                throw new CmdException("Native", $"Invalid number of types given \'{args[0]}\'.");
            }
            return (o1, o2);
        }

        private static bool Equals(string[] args)
        {
            (var o1, var o2) = GetAsTypes(args);
            return o1.Equals(o2);
        }

        private static int Compare(string[] args)
        {
            (var o1, var o2) = GetAsTypes(args);
            if (o1 is not IComparable c1)
                throw new CmdException("Native", "Left item is not comparable.");
            return c1.CompareTo(o2);
        }

        private static Cmd.MItem EqualsCMD(string[] args)
        {
            return new(Equals(args));
        }

        private static Cmd.MItem EqualsNotCMD(string[] args)
        {
            return new(!Equals(args));
        }

        private static bool MemGenCon(CmdLauncher cl, bool pop)
        {
            var obj = NativePKG.MemObj(cl, pop);
            if (obj is bool c)
                return c;
            var str = obj.ToString() ??
                throw new CmdException("Native", "Last memory item was not a valid boolean.");
            return Condition(str);
        }

        private static bool MaybeRun(string[] args, CmdLauncher cl)
        {
            if (args.Length == 0)
                return false;
            cl.ExecuteCommand(args[0], Utils.TrimFirst(args));
            return true;
        }

        private static Func<string[], CmdLauncher, Cmd.MItem> NotGEN(bool pop)
        {
            return (args, cl) =>
            {
                MaybeRun(args, cl);
                return new(!MemGenCon(cl, pop));
            };
        }

        private static Cmd.MItem AndCMD(string[] args, CmdLauncher cl)
        {
            MaybeRun(args, cl);
            return new(MemGenCon(cl, true) && MemGenCon(cl, true));
        }

        private static Cmd.MItem OrCMD(string[] args, CmdLauncher cl)
        {
            MaybeRun(args, cl);
            return new(MemGenCon(cl, true) || MemGenCon(cl, true));
        }

        private static Cmd.MItem NotArgCMD(string[] args)
        {
            return new(!Condition(args[0]));
        }

        private static Cmd.MItem AndArgCMD(string[] args)
        {
            return new(Condition(args[0]) && Condition(args[1]));
        }

        private static Cmd.MItem OrArgCMD(string[] args)
        {
            return new(Condition(args[0]) || Condition(args[1]));
        }

        private static Func<string[], Cmd.MItem> CmpGEN(Predicate<int> predicate)
        {
            return args => new(predicate.Invoke(Compare(args)));
        }

        private static Cmd.MItem IsTypeArgCMD(string[] args)
        {
            var t = StrUtils.BetterGetType(args[1]);
            try
            {
                var res = Convert.ChangeType(args[0], t);
                return new(res != null);
            }
            catch
            {
                return new(false);
            }
        }

        private static Cmd.MItem IsTypeCMD(string[] args, CmdLauncher cl)
        {
            var t = StrUtils.BetterGetType(args[0]);
            return new(NativePKG.MemObj(cl, false).GetType() == t);
        }

        #endregion

        #region StateCommands

        private static Cmd GetStateGEN(Func<CmdLauncher, bool> get, string name)
        {
            return new((_, cl) => new(get.Invoke(cl))) { Desc = $"Adds the {name} state into memory." };
        }

        private static Cmd SetStateArgGEN(Func<CmdLauncher, bool> get, Action<CmdLauncher, bool> set, string name)
        {
            return new((args, cl) =>
            {
                var c = args.Length > 0 ? Condition(args[0]) : !get.Invoke(cl);
                set.Invoke(cl, c);
            })
            {
                Min = 1,
                Max = 1,
                Desc = $"Sets the {name} state to the given argument.",
                Usage = $"?<True/False->Toggle>:<Num!=0>",
            };
        }

        private static Cmd SetStateGEN(Action<CmdLauncher, bool> set, string name, bool pop)
        {
            return new((args, cl) =>
            {
                MaybeRun(args, cl);
                var c = MemBool(cl, pop);
                set.Invoke(cl, c);
            })
            {
                Max = -1,
                Desc = $"{(pop ? "Pops" : "Peeks")} the last item in memory and set the {name} state.",
                Usage = Cmd.MBCHAIN,
            };
        }

        private static IEnumerable<(string Name, Cmd Com)> StateAllGEN(Func<CmdLauncher, bool> get,
            Action<CmdLauncher, bool> set,
            string name,
            string longName = "")
        {
            yield return ($"is{name}", GetStateGEN(get, longName));
            yield return ($"_{name}<", SetStateArgGEN(get, set, longName));
            yield return ($"_{name}", SetStateGEN(set, longName, false));
            yield return ($"_{name}^", SetStateGEN(set, longName, true));
        }

        private void AddStateGEN(Func<CmdLauncher, bool> get,
            Action<CmdLauncher, bool> set,
            string name,
            string longName = "")
        {
            foreach (var p in StateAllGEN(get, set, name, longName))
                Commands[p.Name] = p.Com;
        }

        #endregion
    }
}