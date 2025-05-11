using CSUtils;
namespace SCE
{
    internal class StatePKG : Package
    {
        public StatePKG()
        {
            Name = "State";
            Version = "0.0.0";
            Commands = new()
            {
                { "ifarg", new(IfArgGEN(false)) { MinArgs = 2, MaxArgs = -1,
                    Description = "Runs the command if the argument condition is true.",
                    Usage = "<True/False:!=0->True;False> <Command> ?<Arg1>..."} },

                { "!ifarg", new(IfArgGEN(true)) { MinArgs = 2, MaxArgs = -1,
                    Description = "Runs the command if the argument condition is false.",
                    Usage = "<True/False:!=0->True;False> <Command> ?<Arg1>..."} },

                { "elifarg", new(ElseIfArgCMD) { MinArgs = 3, MaxArgs = 3,
                    Description = "Runs left cmd if argument condition true; right cmd.",
                    Usage = "<True/False:!=0->True;False> <Command1> <Command2>"} },

                { "if^", new(IfGEN(true, false)) { MinArgs = 1, MaxArgs = -1,
                    Description = "Pops the last memory item, runs command if true.",
                    Usage = "<Command> ?<Arg1>..."} },

                { "!if^", new(IfGEN(true, true)) { MinArgs = 1, MaxArgs = -1,
                    Description = "Pops the last memory item, runs command if false.",
                    Usage = "<Command> ?<Arg1>..."} },

                 { "if", new(IfGEN(false, false)) { MinArgs = 1, MaxArgs = -1,
                    Description = "Peeks the last memory item, runs command if true.",
                    Usage = "<Command> ?<Arg1>..."} },

                { "!if", new(IfGEN(false, true)) { MinArgs = 1, MaxArgs = -1,
                    Description = "Peeks the last memory item, runs command if false.",
                    Usage = "<Command> ?<Arg1>..."} },

                { "elif", new(ElseIfCMD)  { MinArgs = 2, MaxArgs = 2,
                    Description = "Runs left cmd if last mem item true; right cmd.",
                    Usage = "<Command1> <Command2>"} },

                { "eql", new(EqualsCMD) { MinArgs = 2, MaxArgs = 3,
                    Description = "Outputs whether the given arguments are equal.",
                    Usage = "<Left> <Right> ?<Type:LeftType,RightType>" } },

                { "!eql", new(EqualsNotCMD) { MinArgs = 2, MaxArgs = 3,
                    Description = "Outputs whether the given arguments are not equal.",
                    Usage = "<Left> <Right> ?<Type:LeftType,RightType>" } },

                { "!last", new(NotResCMD) { MaxArgs = -1,
                    Description = "Nots and outputs the last item in memory after running the given command.",
                    Usage = "?<CommandName> ?<Arg1>..." } },

                { "cmp=", new(CmpGEN(x => x == 0)) { MinArgs = 2, MaxArgs = 3,
                    Description = "Outputs whether left = right.",
                    Usage = "<Left> <Right> ?<Type:LeftType,RightType>" } },

                { "cmp!=", new(CmpGEN(x => x != 0)) { MinArgs = 2, MaxArgs = 3,
                    Description = "Outputs whether left != right.",
                    Usage = "<Left> <Right> ?<Type:LeftType,RightType>" } },

                { "cmp<", new(CmpGEN(x => x < 0)) { MinArgs = 2, MaxArgs = 3,
                    Description = "Outputs whether left < right.",
                    Usage = "<Left> <Right> ?<Type:LeftType,RightType>" } },

                { "cmp>", new(CmpGEN(x => x > 0)) { MinArgs = 2, MaxArgs = 3,
                    Description = "Outputs whether left > right.",
                    Usage = "<Left> <Right> ?<Type:LeftType,RightType>" } },

                { "cmp<=", new(CmpGEN(x => x <= 0)) { MinArgs = 2, MaxArgs = 3,
                    Description = "Outputs whether left <= right.",
                    Usage = "<Left> <Right> ?<Type:LeftType,RightType>" } },

                { "cmp>=", new(CmpGEN(x => x >= 0)) { MinArgs = 2, MaxArgs = 3,
                    Description = "Outputs whether left >= right.",
                    Usage = "<Left> <Right> ?<Type:LeftType,RightType>" } },

                { "notarg", new(NotArgCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Nots the given boolean.",
                    Usage = "<Boolean>" } },

                { "istypearg", new(IsTypeArgCMD) { MinArgs = 2, MaxArgs = 2,
                    Description = "Determines if the given argument is a type.",
                    Usage = "<Check> <Type>" } },

                { "istype", new(IsTypeCMD) { MinArgs = 1, MaxArgs = 1,
                    Description = "Determines if the last mem item is a type.",
                    Usage = "<Type>" } },
            };
            AddStateGEN(cb => cb.Launcher.CmdFeedback, (cb, c) => cb.Launcher.CmdFeedback = c, "feed", "command feedback");
            AddStateGEN(cb => cb.Launcher.ErrFeedback, (cb, c) => cb.Launcher.ErrFeedback = c, "efeed", "error feedback");
            AddStateGEN(cb => cb.Launcher.NeatErrors, (cb, c) => cb.Launcher.NeatErrors = c, "neaterr", "neat errors");
            AddStateGEN(cb => cb.Launcher.MemLock, (cb, c) => cb.Launcher.MemLock = c, "memlock", "memory lock");
            AddStateGEN(cb => cb.Launcher.CmdCaching, (cb, c) => cb.Launcher.CmdCaching = c, "cache", "command caching");
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

        private static bool MemBool(Cmd.Callback cb, bool pop = true)
        {
            var obj = NativePKG.MemObj(cb, pop);
            if (obj is bool c)
                return c;
            var str = obj.ToString() ??
                throw new CmdException("Native", "Last memory item was not a valid boolean.");
            return !Condition(str);
        }

        private static Action<string[], Cmd.Callback> IfArgGEN(bool invert)
        {
            return (args, cb) =>
            {
                if (invert ? !Condition(args[0]) : Condition(args[0]))
                    cb.Launcher.ExecuteCommand(args[1], Utils.TrimFromStart(args, 2));
            };
        }

        private static void ElseIfArgCMD(string[] args, Cmd.Callback cb)
        {
            cb.Launcher.ExecuteCommand(Condition(args[0]) ? args[1] : args[2]);
        }

        private static Action<string[], Cmd.Callback> IfGEN(bool pop, bool invert)
        {
            return (args, cb) =>
            {
                if (invert ? !MemBool(cb, pop) : MemBool(cb, pop))
                    cb.Launcher.ExecuteCommand(args[0], Utils.TrimFirst(args));
            };
        }

        private static void ElseIfCMD(string[] args, Cmd.Callback cb)
        {
            cb.Launcher.ExecuteCommand(MemBool(cb) ? args[0] : args[1]);
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

        private static Cmd.MemItem EqualsCMD(string[] args)
        {
            return new(Equals(args));
        }

        private static Cmd.MemItem EqualsNotCMD(string[] args)
        {
            return new(!Equals(args));
        }

        private static Cmd.MemItem NotResCMD(string[] args, Cmd.Callback cb)
        {
            if (args.Length > 0)
                cb.Launcher.ExecuteCommand(args[0], Utils.TrimFirst(args));
            if (cb.Launcher.MemoryStack.Count == 0)
                throw new CmdException("Native", "Memory stack is empty.");
            var obj = cb.Launcher.MemoryStack.Pop() ??
                throw new CmdException("Native", "Memory item is null.");
            if (obj is bool c)
                return new(!c);
            var str = obj.ToString() ??
                throw new CmdException("Native", "Last memory item was not a valid boolean.");
            return new(!Condition(str));
        }

        private static Func<string[], Cmd.MemItem> CmpGEN(Predicate<int> predicate)
        {
            return args => new(predicate.Invoke(Compare(args)));
        }

        private static Cmd.MemItem NotArgCMD(string[] args, Cmd.Callback cb)
        {
            return new(!Condition(args[0]));
        }

        private static Cmd.MemItem IsTypeArgCMD(string[] args)
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

        private static Cmd.MemItem IsTypeCMD(string[] args, Cmd.Callback cb)
        {
            var t = StrUtils.BetterGetType(args[0]);
            return new(NativePKG.MemObj(cb, false).GetType() == t);
        }

        #endregion

        #region StateCommands

        private static Cmd GetStateGEN(Func<Cmd.Callback, bool> get, string name)
        {
            return new((_, cb) => new(get.Invoke(cb))) { Description = $"Adds the {name} state into memory." };
        }

        private static Cmd SetStateArgGEN(Func<Cmd.Callback, bool> get, Action<Cmd.Callback, bool> set, string name)
        {
            return new((args, cb) =>
            {
                var c = args.Length > 0 ? Condition(args[0]) : !get.Invoke(cb);
                cb.Launcher.FeedbackLine($"{name} = {c}");
                set.Invoke(cb, c);
            })
            {
                MinArgs = 1,
                MaxArgs = 1,
                Description = $"Sets the {name} state to the given argument.",
                Usage = $"?<True/False->Toggle>:<Num!=0>",
            };
        }

        private static Cmd SetStateGEN(Action<Cmd.Callback, bool> set, string name, bool pop)
        {
            return new((args, cb) =>
            {
                if (args.Length > 0)
                    cb.Launcher.ExecuteCommand(args[0], Utils.TrimFirst(args));
                var c = MemBool(cb, pop);
                cb.Launcher.FeedbackLine($"{name} = {c}");
                set.Invoke(cb, c);
            })
            {
                MaxArgs = -1,
                Description = $"{(pop ? "Pops" : "Peeks")} the last item in memory and set the {name} state.",
                Usage = $"?<CommandName> ?<Arg1>...",
            };
        }

        private static IEnumerable<(string Name, Cmd Com)> StateAllGEN(Func<Cmd.Callback, bool> get,
            Action<Cmd.Callback, bool> set,
            string name,
            string longName = "")
        {
            yield return ($"is{name}", GetStateGEN(get, longName));
            yield return ($"_{name}<", SetStateArgGEN(get, set, longName));
            yield return ($"_{name}", SetStateGEN(set, longName, false));
            yield return ($"_{name}^", SetStateGEN(set, longName, true));
        }

        private void AddStateGEN(Func<Cmd.Callback, bool> get,
            Action<Cmd.Callback, bool> set,
            string name,
            string longName = "")
        {
            foreach (var p in StateAllGEN(get, set, name, longName))
                Commands[p.Name] = p.Com;
        }

        #endregion
    }
}
