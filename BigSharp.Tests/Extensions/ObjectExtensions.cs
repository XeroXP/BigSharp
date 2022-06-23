namespace BigSharp.Tests.Extensions
{
    internal static class ObjectExtensions
    {
        public static Big? ToBig(this object? obj)
        {
            Big? big = null;
            if (obj is int intObj)
            {
                big = new Big(intObj);
            }
            if (obj is long longObj)
            {
                big = new Big(longObj);
            }
            if (obj is double doubleObj)
            {
                big = new Big(doubleObj);
            }
            if (obj is decimal decimalObj)
            {
                big = new Big(decimalObj);
            }
            if (obj is Big bigObj)
            {
                big = new Big(bigObj);
            }
            if (obj is string stringObj)
            {
                big = new Big(stringObj);
            }

            if (big == null && double.TryParse(obj?.ToString(), out var obj1))
            {
                big = new Big(obj1);
            }

            return big;
        }

        public static string? ToExpectedString(this object? obj)
        {
            string? expected = obj?.ToString();
            if (obj is double objDouble)
            {
                expected = objDouble.ToString(Big.PE, Big.NE);
            }
            if (obj is float objFloat)
            {
                expected = objFloat.ToString(Big.PE, Big.NE);
            }
            if (obj is float objDecimal)
            {
                expected = objDecimal.ToString(Big.PE, Big.NE);
            }
            return expected;
        }
    }
}
