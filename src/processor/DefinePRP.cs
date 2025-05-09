using System.Text;
using CSUtils;
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

        private string SmartReplace(string str)
        {
            StringBuilder sb = new();
            bool ignore = false;
            int asl = -1;
            for (int i = 0; i < str.Length; ++i)
            {
                if (Utils.Match(str, IGNORE, i))
                {
                    sb.Append(IGNORE);
                    ignore = true;
                    i += IGNORE.Length - 1;
                    continue;
                }
                else if (Utils.Match(str, END_IGNORE, i))
                {
                    sb.Append(END_IGNORE);
                    ignore = false;
                    i += END_IGNORE.Length - 1;
                    continue;
                }
                else if (Utils.Match(str, ASL, i))
                {
                    sb.Append(ASL);
                    asl = i + ASL.Length;
                    i += ASL.Length - 1;
                    continue;
                }
                if (!ignore)
                {
                    bool found = false;
                    foreach (var def in Defines)
                    {
                        if (Utils.Match(str, def.Key, i))
                        {
                            if (i != asl)
                                sb.Append(def.Value.Invoke());
                            else
                            {
                                sb.Remove(sb.Length - ASL.Length, ASL.Length);
                                sb.Append(def.Key);
                            }
                            found = true;
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
                if (Utils.Match(str, IGNORE, i))
                    i += IGNORE.Length - 1;
                else if (Utils.Match(str, END_IGNORE, i))
                    i += END_IGNORE.Length - 1;
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
