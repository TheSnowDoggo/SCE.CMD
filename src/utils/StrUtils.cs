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

        public static string Build(string[] arr)
        {
            StringBuilder sb = new();
            foreach (var str in arr)
                sb.Append(str);
            return sb.ToString();
        }

        public static string Combine(IEnumerable<string> collection)
        {
            StringBuilder sb = new();
            foreach (var item in collection)
                sb.Append(item);
            return sb.ToString();
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

        public static string InsertEscapeCharacters(string str)
        {
            StringBuilder sb = new(str.Length);
            for (int i = 0; i < str.Length; ++i)
            {
                if (str[i] != '\\' || i == str.Length - 1)
                    sb.Append(str[i]);
                else
                { 
                    switch (str[i + 1])
                    {
                        case 'n': sb.Append('\n'); 
                            break;
                        case 'r': sb.Append('\r'); 
                            break;
                        case 't': sb.Append('\t');
                            break;
                        default: sb.Append('\\');
                            continue;
                    }  
                    ++i;
                }
            }
            return sb.ToString();
        }

        #region FitToLength

        private static string FTL(bool postFit, string str, int length, char fill = ' ')
        {
            if (length < 0)
                throw new ArgumentException("Length cannot be less than 0.");

            int difference = length - str.Length;
            switch (difference)
            {
                case 0: return str;
                case > 0: return postFit ? str + Copy(fill, difference) : Copy(fill, difference) + str;
                case < 0: return str[..length];
            }
        }

        public static string PostFTL(string str, int length, char fill = ' ')
        {
            return FTL(true, str, length, fill);
        }

        public static string PreFTL(string str, int length, char fill = ' ')
        {
            return FTL(false, str, length, fill);
        }

        #endregion

        #region Copy

        public static string Copy(char chr, int copies)
        {
            return new(ArrUtils.Copy(chr, copies));
        }

        public static string Copy(string str, int copies)
        {
            if (copies <= 0)
                return string.Empty;
            StringBuilder sb = new(str.Length * copies);
            for (int i = 0; i < copies; ++i)
                sb.Append(str);
            return sb.ToString();
        }

        #endregion
    }
}
