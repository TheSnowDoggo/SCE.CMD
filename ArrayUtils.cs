namespace CMD
{
    internal static class ArrayUtils
    {
        public static T[] TrimFirst<T>(T[] arr)
        {
            if (arr.Length == 0)
                return arr;
            var newArr = new T[arr.Length - 1];
            for (int i = 0; i < newArr.Length; ++i)
                newArr[i] = arr[i + 1];
            return newArr;
        }

        public static T[] Copy<T>(T part, int copies)
        {
            if (copies <= 0)
                return Array.Empty<T>();
            var arr = new T[copies];
            for (int i = 0; i < copies; ++i)
                arr[i] = part;
            return arr;
        }
    }
}
