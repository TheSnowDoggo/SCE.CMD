namespace CMD
{
    internal static class StringUtils
    {
        public static void PrettyErr(string source, string error)
        {
            Console.WriteLine("<!> {0} | {1} <!>", source, error);
        }
    }
}
