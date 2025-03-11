namespace CMD
{
    internal class CmdException : Exception
    {
        public CmdException(string source, string message)
            : base(StringUtils.FormatErr(source, message))
        {
            SourceMSG = source;
            MessageMSG = message;
        }

        public string SourceMSG { get; }

        public string MessageMSG { get; }

        public override string ToString()
        {
            return StringUtils.FormatErr(SourceMSG, MessageMSG);
        }
    }
}
