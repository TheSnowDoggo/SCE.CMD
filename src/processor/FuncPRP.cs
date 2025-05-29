namespace SCE
{
    public class FuncPRP : Preprocessor
    {
        public FuncPRP(Func<string, string> func, int priority)
            : base(priority)
        {
            Func = func;
        }

        public Func<string, string> Func { get; }

        public override string Process(string input)
        {
            return Func.Invoke(input);
        }
    }
}
