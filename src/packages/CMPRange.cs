namespace SCE
{
    public class CMPRange
    {
        public enum CMode
        {
            Greater,
            GreaterEq,
            Less,
            LessEq,
        }

        public interface IComp
        {
            bool Valid(int v);
        }

        public record SComp(int V, CMode M) : IComp
        {
            public bool Valid(int v)
            {
                return GetCompare(M).Invoke(v, V);
            }
        }

        public record DComp(int V1, CMode M1, int V2, CMode M2) : IComp
        {
            public bool Valid(int v)
            {
                return !Less(M1) && Less(M2) ?
                    (GetCompare(M1).Invoke(v, V1) && GetCompare(M2).Invoke(v, V2)) :
                    (GetCompare(M1).Invoke(v, V1) || GetCompare(M1).Invoke(v, V2));
            }
        }

        private string lastViolation = "";

        private readonly List<IComp> _comp = new();

        private readonly HashSet<int> _allow = new();

        private readonly HashSet<int> _disallow = new();

        public CMPRange(string str)
        {
            var split = str.Split(',');
            for (int i = 0; i < split.Length; ++i)
            {
                int k1i = NextKey(split[i]);
                if (k1i == -1)
                {
                    _allow.Add(int.Parse(split[i]));
                    continue;
                }
                int k1Len = KeyLength(split[i], k1i);
                var k1 = split[i].Substring(k1i, k1Len);
                int k2i = NextKey(split[i], k1i + k1Len);
                if (k2i == -1)
                {
                    if (k1 != "-")
                    {
                        int val = int.Parse(split[i].Remove(k1i, k1Len));
                        switch (k1)
                        {
                            case "=": _allow.Add(val);
                                break;
                            case "!":
                            case "!=":
                                _disallow.Add(val);
                                break;
                            default:
                                var m = CompareModeOf(k1);
                                _comp.Add(new SComp(val, m));
                                break;
                        }
                    }
                    else
                    {
                        var range = split[i].Split('-');
                        if (range.Length != 2)
                            throw new ArgumentException("Invalid number of ranges.");
                        int r1 = Convert.ToInt32(range[0]);
                        int r2 = Convert.ToInt32(range[1]);
                        if (r1 == r2)
                            _allow.Add(r1);
                        else
                        {
                            _comp.Add(new DComp(int.Parse(range[0]),
                               CMode.GreaterEq, int.Parse(range[1]), CMode.LessEq));
                        }
                    }
                    continue;
                }
                int k2Len = KeyLength(split[i], k2i);
                var k2 = split[i].Substring(k2i, k2Len);

                var m1 = CompareModeOf(k1);
                var m2 = CompareModeOf(k2);

                int v1 = int.Parse(split[i][..k1i]);
                int v2 = int.Parse(split[i][(k2i + k2Len)..]);
                _comp.Add(new DComp(v1, m1, v2, m2));
            }
        }

        public bool Validate(int v)
        {
            if (_allow.Contains(v))
                return true;
            if (_disallow.Contains(v))
                return false;
            foreach (var comp in _comp)
                if (comp.Valid(v))
                    return true;
            return false;
        }

        private static int NextKey(string str, int index = 0)
        {
            if (index < 0)
                throw new IndexOutOfRangeException("Index was negative.");
            int start = -1;
            for (int i = index; i < str.Length; ++i)
            {
                if (IsKey(str[i]))
                {
                    if (start != -1)
                        break;
                    start = i;
                }
            }
            return start;
        }

        private static Func<int, int, bool> GetCompare(CMode mode)
        {
            return mode switch
            {
                CMode.Greater   => (a, b) => a > b,
                CMode.GreaterEq => (a, b) => a >= b,
                CMode.Less      => (a, b) => a < b,
                CMode.LessEq    => (a, b) => a <= b,
                _ => throw new NotImplementedException("Invalid CMode.")
            };
        }

        private static bool Less(CMode mode)
        {
            return mode is CMode.Less or CMode.LessEq;
        }

        private static int KeyLength(string str, int index = 0)
        {
            int len = 0;
            for (int i = index; i < str.Length; ++i)
            {
                if (!IsKey(str[i]))
                    break;
                ++len;
            }
            return len;
        }

        private static bool IsKey(char c)
        {
            return c is '>' or '<' or '=' or '-' or '!';
        }

        private static CMode CompareModeOf(string str)
        {
            return str switch
            {
                ">"  => CMode.Greater,
                ">=" => CMode.GreaterEq,
                "<"  => CMode.Less,
                "<=" => CMode.LessEq,
                _ => throw new ArgumentException("Invalid compare mode.")
            };
        }
    }
}
