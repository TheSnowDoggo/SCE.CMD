using System.Text;

namespace CMD
{
    internal static class ArrayUtils
    {
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
            bool first = true;
            bool inSpeech = false;
            List<string> args = new(input.Length);
            StringBuilder sb = new(input.Length);
            foreach (var c in input)
            {
                if (first)
                {
                    if (c == ' ')
                        first = false;
                    continue;
                }

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
    }
}
