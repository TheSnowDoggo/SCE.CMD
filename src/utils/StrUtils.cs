using System.Text;

namespace SCE
{
    public static class StrUtils
    {
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
            foreach (char c in str)
            {
                if (!predicate(c))
                    return sb.ToString();
                sb.Append(c);
            }
            return sb.ToString();
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
