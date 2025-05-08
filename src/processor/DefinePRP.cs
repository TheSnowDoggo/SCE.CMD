using System.Text;

namespace SCE
{
    public class DefinePRP : Preprocessor
    {
        public const string IGNORE = "@IGNORE";
        public const string END_IGNORE = "@ENDIGNORE";
        public const string ASL = "@ASL";

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

        private string SmartReplace(string str)
        {
            StringBuilder sb = new();
            bool ignore = false;
            int asl = -1;
            for (int i = 0; i < str.Length; ++i)
            {
                if (Match(str, IGNORE, i))
                {
                    sb.Append(IGNORE);
                    ignore = true;
                    i += IGNORE.Length - 1;
                    continue;
                }
                else if (Match(str, END_IGNORE, i))
                {
                    sb.Append(END_IGNORE);
                    ignore = false;
                    i += END_IGNORE.Length - 1;
                    continue;
                }
                else if (Match(str, ASL, i))
                {
                    sb.Append(ASL);
                    asl = ASL.Length;
                    i += asl - 1;
                    continue;
                }
                if (!ignore && asl != i)
                {
                    bool found = false;
                    foreach (var def in Defines)
                    {
                        if (Match(str, def.Key, i))
                        {
                            found = true;
                            sb.Append(def.Value.Invoke());
                            i += def.Key.Length - 1;
                            break;
                        }
                    }
                    if (found)
                        continue;
                }
                sb.Append(str[i]);
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
                else if (Match(str, ASL, i))
                    i += ASL.Length - 1;
                else
                    sb.Append(str[i]);
            }
            return sb.ToString();
        }

        public override string Process(string input)
        {
            input = SmartReplace(input);
            return AutoRemoveIgnore ? RemoveIgnore(input) : input;
        }
    }
}
