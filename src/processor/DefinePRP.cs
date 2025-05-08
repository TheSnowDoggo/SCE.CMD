namespace SCE
{
    public class DefinePRP : Preprocessor
    {
        public DefinePRP(int priority)
            : base(priority)
        {
        }

        public Dictionary<string, Func<string>> Defines { get; } = new();

        public override string Process(string input)
        {
            foreach (var def in Defines)
                input = input.Replace(def.Key, def.Value.Invoke());
            return input;
        }
    }
}
