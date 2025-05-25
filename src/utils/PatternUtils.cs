using System.Text;
namespace SCE
{
    // DO NOT USE | WILL BE REPLACED
    internal static class PatternUtils
    {
        public static void ViewGeneric<T>(Dictionary<string, T> dict, string[] args, string source, string article)
        {
            if (dict.Count == 0)
                throw new CmdException(source, "Nothing to view.");
            StringBuilder sb = new();
            if (args.Length > 0 && args[0] == "*")
            {
                foreach (var item in dict)
                {
                    sb.AppendLine($"{item.Key} > {item.Value}");
                }
            }
            else
            {
                foreach (var name in args)
                {
                    if (!dict.TryGetValue(name, out var store))
                        throw new CmdException(source, $"Unknown {article} \'{name}\'.");
                    sb.AppendLine($"{name} > {store}");
                }
            }
            Console.Write(sb.ToString());
        }
    }
}
