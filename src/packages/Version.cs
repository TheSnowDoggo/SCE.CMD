using System.Diagnostics.CodeAnalysis;
namespace SCE
{
    public record Version(int Major, int Minor, int Patch)
    {
        public static Version Zero { get => new(0, 0, 0); }

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

        public override string ToString()
        {
            return $"{Major}.{Minor}.{Patch}";
        }

        #region Operators

        public static bool operator >(Version lhs, Version rhs) =>
            lhs.Major > rhs.Major && lhs.Minor > rhs.Minor && lhs.Patch > rhs.Patch;

        public static bool operator <(Version lhs, Version rhs) => rhs > lhs;

        public static bool operator >=(Version lhs, Version rhs) => !(lhs < rhs);

        public static bool operator <=(Version lhs, Version rhs) => !(lhs > rhs);

        #endregion
    }
}
