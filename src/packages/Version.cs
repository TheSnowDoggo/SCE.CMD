using System.Diagnostics.CodeAnalysis;

namespace SCE
{
    public record Version(int Major = 0, int Minor = 0, int Patch = 0)
    {
        public static Version Parse(string str)
        {
            var split = str.Split('.');
            if (split.Length != 3)
                throw new ArgumentException("String was in an invalid format. Expected \'MAJOR.MINOR.PATCH\'");
            return new(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]));
        }

        public static bool TryParse(string str, [NotNullWhen(true)] out Version? version)
        {
            try
            {
                version = Parse(str);
                return true;
            }
            catch
            {
                version = null;
                return false;
            }
        }
    }
}
