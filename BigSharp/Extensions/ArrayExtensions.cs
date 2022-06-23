namespace BigSharp.Extensions
{
    internal static class ArrayExtensions
    {
        internal static void Resize<T>(ref T[] numbers, long newSize)
        {
            var newNumbers = new T[newSize];
            Array.Copy(numbers, 0, newNumbers, 0, Math.Min(numbers.LongLength, newSize));
            numbers = newNumbers;
        }

        internal static void Reverse<T>(ref T[] numbers)
        {
            Array.Reverse(numbers);
            /*for (int i = 0; i < numbers.Length / 2; i++)
            {
                var tmp = numbers[i];
                numbers[i] = numbers[numbers.Length - i - 1];
                numbers[numbers.Length - i - 1] = tmp;
            }*/
        }

        internal static void Pop<T>(ref T[] numbers)
        {
            Resize(ref numbers, numbers.LongLength - 1);
        }

        internal static void Push<T>(ref T[] numbers, T value)
        {
            AddElementAt(ref numbers, numbers.LongLength, value);
        }

        internal static T[] Slice<T>(this T[] numbers, long start = 0, long length = 0)
        {
            if (length == 0) length = numbers.LongLength - start;

            if (length > numbers.LongLength - start) length = numbers.LongLength - start;
            T[] destfoo = new T[length];
            Array.Copy(numbers, start, destfoo, 0, length);
            return destfoo;
        }

        internal static T Shift<T>(ref T[] numbers)
        {
            T[] cloneArray = (T[])numbers.Clone();
            Resize(ref numbers, numbers.LongLength - 1);
            for(long i = 1; i < cloneArray.LongLength; i++)
            {
                numbers[i-1] = cloneArray[i];
            }
            return cloneArray[0];
        }

        internal static void Unshift<T>(ref T[] numbers, params T[] values)
        {
            foreach (var value in values.Reverse())
            {
                AddElementAt(ref numbers, 0, value);
            }
        }

        /// <summary>    
        /// Add element at nth position in array    
        /// </summary>    
        /// <param name="numbers">Source Array</param>    
        /// <param name="index">Position Number</param>    
        /// <param name="value">the value to be entered</param>    
        internal static void AddElementAt<T>(ref T[] numbers, long index, T value)
        {
            //first resize it    
            Resize(ref numbers, numbers.Length + 1);

            long orginalLength = numbers.LongLength;
            //clone the array    
            T[] cloneArray = (T[])numbers.Clone();
            for (long i = 0; i < orginalLength - 1; i++)
            {
                if (i == index)
                {
                    //copy element from the position    
                    var element = cloneArray[index];
                    numbers[index] = value;
                    numbers[index + 1] = element;
                }
                else if (i > index)
                {
                    numbers[i + 1] = cloneArray[i];
                }
                else
                {
                    numbers[i] = cloneArray[i];
                }
            }
        }
    }
}
