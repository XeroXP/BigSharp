using System;
using System.Globalization;

namespace BigSharp.Tests.Extensions
{
    internal static class NumberExtensions
    {
        public static string ToString(this double num, int pe, int ne)
        {
            long i = 0;
            try
            {
                i = BitConverter.GetBytes(decimal.GetBits((decimal)num)[3])[2];
            }
            catch (OverflowException)
            { }
            //i += 1;
            //Console.WriteLine(((num >= 0 && i >= pe) || (num < 0 && i >= -ne) || i == 0) ? "g" : "F" + (num >= 0 ? (i >= pe ? pe : i) : (i >= -ne ? -ne : i)));
            return num.ToString(((num >= 0 && i >= pe) || (num < 0 && i >= -ne) || i == 0) ? "g" : "F" + (num >= 0 ? (i >= pe ? pe : i) : (i >= -ne ? -ne : i)), CultureInfo.InvariantCulture);
        }

        public static string ToString(this float num, int pe, int ne)
        {
            long i = 0;
            try
            {
                i = BitConverter.GetBytes(decimal.GetBits((decimal)num)[3])[2];
            }
            catch (OverflowException)
            { }
            //i += 1;
            //Console.WriteLine(((num >= 0 && i >= pe) || (num < 0 && i >= -ne) || i == 0) ? "g" : "F" + (num >= 0 ? (i >= pe ? pe : i) : (i >= -ne ? -ne : i)));
            return num.ToString(((num >= 0 && i >= pe) || (num < 0 && i >= -ne) || i == 0) ? "g" : "F" + (num >= 0 ? (i >= pe ? pe : i) : (i >= -ne ? -ne : i)), CultureInfo.InvariantCulture);
        }

        public static string ToString(this decimal num, int pe, int ne)
        {
            long i = 0;
            try
            {
                i = BitConverter.GetBytes(decimal.GetBits((decimal)num)[3])[2];
            }
            catch (OverflowException)
            { }
            //i += 1;
            //Console.WriteLine(((num >= 0 && i >= pe) || (num < 0 && i >= -ne) || i == 0) ? "g" : "F" + (num >= 0 ? (i >= pe ? pe : i) : (i >= -ne ? -ne : i)));
            return num.ToString(((num >= 0 && i >= pe) || (num < 0 && i >= -ne) || i == 0) ? "g" : "F" + (num >= 0 ? (i >= pe ? pe : i) : (i >= -ne ? -ne : i)), CultureInfo.InvariantCulture);
        }
    }
}
