namespace BigSharp.Tests.Extensions
{
    internal static class ObjectExtensions
    {
        public static string? ToExpectedString(this object? obj, int pe, int ne)
        {
            string? expected = obj?.ToString();
            if (obj is double objDouble)
            {
                expected = objDouble.ToString(pe, ne);
            }
            if (obj is float objFloat)
            {
                expected = objFloat.ToString(pe, ne);
            }
            if (obj is float objDecimal)
            {
                expected = objDecimal.ToString(pe, ne);
            }
            return expected;
        }
    }
}
