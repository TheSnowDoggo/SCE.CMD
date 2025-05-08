namespace SCE
{
    public abstract class Preprocessor : IComparable<Preprocessor>
    {
        public Preprocessor(int priority)
        {
            Priority = priority;
        }

        public int Priority { get; set; }

        public abstract string Process(string input);

        public int CompareTo(Preprocessor? other)
        { 
            return other != null ? Priority - other.Priority : 1;
        }
    }
}
