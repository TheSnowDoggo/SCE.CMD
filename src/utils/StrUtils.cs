using System.Text;

namespace SCE
{
    internal static class StrUtils
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

        public static string[] TrimArgs(string input)
        {
            if (input.Length == 0)
                return Array.Empty<string>();
            bool inSpeech = false;
            List<string> args = new(input.Length);
            StringBuilder sb = new(input.Length);
            foreach (var c in input)
            {
                if (c == '"')
                    inSpeech = !inSpeech;
                else if (c != ' ' || inSpeech)
                    sb.Append(c);
                else
                {
                    args.Add(sb.ToString());
                    sb.Clear();
                }
            }
            if (sb.Length > 0)
                args.Add(sb.ToString());
            return args.ToArray();
        }

        private static string FitToLength(bool postFit, string str, int length, char fill = ' ')
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

        public static string PostFitToLength(string str, int length, char fill = ' ')
        {
            return FitToLength(true, str, length, fill);
        }

        public static string PreFitToLength(string str, int length, char fill = ' ')
        {
            return FitToLength(false, str, length, fill);
        }

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
    }
}
