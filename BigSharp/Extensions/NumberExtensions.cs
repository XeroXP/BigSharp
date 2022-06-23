using System.Globalization;

namespace BigSharp.Extensions
{
    internal static class NumberExtensions
    {
        public static string ToExponential(this double value, int? fractionDigits = null)
        {
            if (!fractionDigits.HasValue)
            {
                return value.ToString("0.####################e+0", CultureInfo.InvariantCulture);
            }

            return value.ToString("0." + string.Empty.PadRight(fractionDigits.Value, '0') + "e+0", CultureInfo.InvariantCulture);
        }
    }
}
