using System.Text;

namespace SCE
{
    /// <summary>
    /// Additional string manipulation utilities.
    /// </summary>
    public static class StrUtils
    {
        private static readonly Dictionary<string, Type> _pretype = new()
        {
            { "int", typeof(int) },
            { "uint", typeof(uint) },
            { "long", typeof(long) },
            { "ulong", typeof(ulong) },
            { "short", typeof(short) },
            { "ushort", typeof(ushort) },
            { "bool", typeof(bool) },
        };

        public static Type BetterGetType(string input)
        {
            if (_pretype.TryGetValue(input.ToLower(), out var t1))
                return t1;
            if (Type.GetType(input, false, true) is Type t2)
                return t2;
            if (Type.GetType($"System.{input}", false, true) is Type t3)
                return t3;
            throw new ArgumentException($"Unknown type \'{input}\'.");
        }

        public static int LastNIndexOf(string str, char chr, int n)
        {
            int count = 0;
            int index = -1;
            for (int i = str.Length - 1; i >= 0; --i)
            {
                if (str[i] == chr)
                {
                    index = i;
                    if (++count >= n)
                        break;
                }
            }
            return index;
        }

        public static string FormatErr(string source, string message)
        {
            return $"<!> {source} | {message} <!>";
        }

        public static void PrettyErr(string source, string message)
        {
            Console.WriteLine(FormatErr(source, message));
        }

        public static string BuildWhile(string str, Predicate<char> predicate)
        {
            StringBuilder sb = new(str.Length);
            foreach (var c in str)
            {
                if (!predicate.Invoke(c))
                    break;
                sb.Append(c);
            }
            return sb.ToString();
        }

        public static int Longest<T>(IEnumerable<T> collection, Func<T, int> lengthFunc)
        {
            int longest = int.MinValue;
            foreach (var item in collection)
            {
                var len = lengthFunc(item);
                if (len > longest)
                    longest = len;
            }
            return longest;
        }

        public static string[] TrimArgs(string str)
        {
            if (str.Length == 0)
                return Array.Empty<string>();
            Stack<char> layerStack = new();
            List<string> args = new(str.Length);
            StringBuilder sb = new(str.Length);
            foreach (var c in str)
            {
                if (c == '\"' || c == '\'')
                {
                    if (layerStack.Count == 0 || layerStack.Peek() != c)
                    {
                        layerStack.Push(c);
                        if (layerStack.Count == 1)
                            continue;
                    }
                    else
                    {
                        layerStack.Pop();
                        if (layerStack.Count == 0)
                            continue;
                    }
                }

                if (c == ' ' && layerStack.Count == 0)
                {
                    args.Add(sb.ToString());
                    sb.Clear();
                }
                else
                    sb.Append(c);
            }
            if (sb.Length > 0)
                args.Add(sb.ToString());
            return args.ToArray();
        }
    }
}
