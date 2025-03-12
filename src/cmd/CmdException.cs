namespace SCE
{
    internal class CmdException : Exception
    {
        public CmdException(string source, string message)
            : base(StrUtils.FormatErr(source, message))
        {
            SourceMSG = source;
            MessageMSG = message;
        }

        public string SourceMSG { get; }

        public string MessageMSG { get; }

        public override string ToString()
        {
            return StrUtils.FormatErr(SourceMSG, MessageMSG);
        }
    }
}
