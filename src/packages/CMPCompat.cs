namespace SCE
{
    public class CMPCompat
    {
        private readonly CMPRange _major, _minor, _patch;

        public CMPCompat(string str)
        {
            var split = str.Split('.');
            if (split.Length != 3)
                throw new ArgumentException("String was in an invalid format. Expected \'MAJOR.MINOR.PATCH\'");
            _major = new(split[0]);
            _minor = new(split[1]);
            _patch = new(split[2]);
        }

        public bool Validate(Version version)
        {
            return _major.Validate(version.Major) &&
                _minor.Validate(version.Minor) && 
                _patch.Validate(version.Patch);
        }
    }
}
