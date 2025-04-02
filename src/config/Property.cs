namespace SCE
{
    internal class Property
    {
        public Property(string name, Type? type = null)
        {
            Name = name;
            Type = type ?? typeof(object);
        }

        public string Name { get; }

        public Type Type { get; }
    }
}
