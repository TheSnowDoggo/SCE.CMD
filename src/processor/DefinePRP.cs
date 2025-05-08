using System.Text;

namespace SCE
{
    public class DefinePRP : Preprocessor
    {
        public const string IGNORE = "@IGNORE";
        public const string END_IGNORE = "@ENDIGNORE";

        public DefinePRP(int priority)
            : base(priority)
        {
        }

        public Dictionary<string, Func<string>> Defines { get; init; } = new();

        public bool AutoRemoveIgnore { get; set; } = true;

        private static bool Match(string str, string match, int index = 0)
        {
            if (index < 0 || index + match.Length > str.Length)
                return false;
            for (int i = 0; i < match.Length; ++i)
                if (str[i + index] != match[i])
                    return false;
            return true;
        }

        private static string SmartReplace(string str, string oldVal, Func<string> newVal)
        {
            if (oldVal.Length == 0)
                return str;
            StringBuilder sb = new();
            bool ignore = false;
            for (int i = 0; i < str.Length; ++i)
            {
                if (Match(str, IGNORE, i))
                {
                    sb.Append(IGNORE);
                    ignore = true;
                    i += IGNORE.Length - 1;
                }
                else if (Match(str, END_IGNORE, i))
                {
                    sb.Append(END_IGNORE);
                    ignore = false;
                    i += END_IGNORE.Length - 1;
                }
                else if (!ignore && Match(str, oldVal, i))
                {
                    sb.Append(newVal.Invoke());
                    i += oldVal.Length - 1;
                }
                else
                {
                    sb.Append(str[i]);
                }
            }
            return sb.ToString();
        }

        private static string RemoveIgnore(string str)
        {
            StringBuilder sb = new();
            for (int i = 0; i < str.Length; ++i)
            {
                if (Match(str, IGNORE, i))
                    i += IGNORE.Length - 1;
                else if (Match(str, END_IGNORE, i))
                    i += END_IGNORE.Length - 1;
                else
                    sb.Append(str[i]);
            }
            return sb.ToString();
        }

        public override string Process(string input)
        {
            foreach (var def in Defines)
                input = SmartReplace(input, def.Key, def.Value);
            return AutoRemoveIgnore ? RemoveIgnore(input) : input;
        }
    }
}
