namespace SCE
{
    public static class ArrUtils
    {
        public static T[] TrimFirst<T>(T[] arr)
        {
            return TrimFromStart(arr, 1);
        }

        public static T[] TrimFromStart<T>(T[] arr, int start)
        {
            if (start >= arr.Length)
                return Array.Empty<T>();
            var newArr = new T[arr.Length - start];
            int count = 0;
            for (int i = start; i < arr.Length; ++i)
            {
                newArr[count] = arr[i];
                ++count;
            }
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
