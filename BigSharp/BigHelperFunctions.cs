using BigSharp.Extensions;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BigSharp
{
    internal class BigHelperFunctions
    {
        private static readonly string NUMERIC = @"^-?(\d+(\.\d*)?|\.\d+)(e[+-]?\d+)?$";

        /// <summary>
        /// Parse the number or string value passed to a Big constructor.
        /// </summary>
        /// <param name="x">{Big} A Big number instance.</param>
        /// <param name="n">{string} A numeric value.</param>
        /// <returns></returns>
        /// <exception cref="BigException"></exception>
        internal static Big parse(Big x, string n)
        {
            long e;
            int i, nl;

            if (!Regex.IsMatch(n, NUMERIC, RegexOptions.IgnoreCase))
            {
                throw new BigException(BigException.INVALID + "number");
            }

            // Determine sign.
            if (n.ElementAt(0) == '-')
            {
                n = n.Substring(1);
                x.s = -1;
            }
            else
            {
                x.s = 1;
            }

            // Decimal point?
            if ((e = n.IndexOf('.')) > -1) n = n.Replace(".", "");

            // Exponential form?
            if ((i = n.IndexOf('e', StringComparison.InvariantCultureIgnoreCase)) > 0)
            {

                // Determine exponent.
                if (e < 0) e = i;
                e += long.Parse(n.Substring(i + 1));
                n = n.Substring(0, i);
            }
            else if (e < 0)
            {

                // Integer.
                e = n.Length;
            }

            nl = n.Length;

            // Determine leading zeros.
            for (i = 0; i < nl && n.ElementAt(i) == '0';) ++i;

            if (i == nl)
            {

                // Zero.
                x.e = 0;
                x.c = new long[] { 0 };
            }
            else
            {

                // Determine trailing zeros.
                for (; nl > 0 && n.ElementAt(--nl) == '0';) ;
                x.e = e - i - 1;
                x.c = new long[0];

                // Convert string to array of digits without leading/trailing zeros.
                for (e = 0; i <= nl;)
                {
                    if (x.c.LongLength < e + 1)
                        ArrayExtensions.Resize(ref x.c, e + 1);
                    x.c[e++] = int.Parse(n.ElementAt(i++).ToString(CultureInfo.InvariantCulture));
                }
            }

            return x;
        }

        /// <summary>
        /// Round Big x to a maximum of sd significant digits using rounding mode rm.
        /// </summary>
        /// <param name="x">{Big} The Big to round.</param>
        /// <param name="sd">{number} Significant digits: integer, 0 to MAX_DP inclusive.</param>
        /// <param name="rm">{RoundingMode} Rounding mode.</param>
        /// <param name="more">{boolean} Whether the result of division was truncated.</param>
        /// <returns></returns>
        internal static Big round(Big x, long sd, RoundingMode? rm, bool more = false)
        {
            var xc = x.c;

            if (rm == null) rm = x.Config.RM;

            if (sd < 1)
            {
                more =
                  rm == RoundingMode.ROUND_UP && (more || (xc.LongLength > 0 && xc[0] != 0)) || sd == 0 && (
                  rm == RoundingMode.ROUND_HALF_UP && xc[0] >= 5 ||
                  rm == RoundingMode.ROUND_HALF_EVEN && (xc[0] > 5 || xc[0] == 5 && (more || xc.LongLength > 1))
                );

                ArrayExtensions.Resize(ref xc, 1);

                if (more)
                {

                    // 1, 0.1, 0.01, 0.001, 0.0001 etc.
                    x.e = x.e - sd + 1;
                    xc[0] = 1;
                }
                else
                {

                    // Zero.
                    xc[0] = x.e = 0;
                }
            }
            else if (sd < xc.LongLength)
            {

                // xc[sd] is the digit after the digit that may be rounded up.
                more =
                  rm == RoundingMode.ROUND_HALF_UP && xc[sd] >= 5 ||
                  rm == RoundingMode.ROUND_HALF_EVEN && (xc[sd] > 5 || xc[sd] == 5 &&
                    (more || (xc.LongLength > sd + 1) || (xc[sd - 1] & 1) != 0)) ||
                  rm == RoundingMode.ROUND_UP && (more || (xc.LongLength > 0 && xc[0] != 0));

                // Remove any digits after the required precision.
                ArrayExtensions.Resize(ref xc, sd--);

                // Round up?
                if (more)
                {

                    // Rounding up may mean the previous digit has to be rounded up.
                    for (; sd >= 0 && ++xc[sd] > 9;)
                    {
                        xc[sd] = 0;
                        if (0 == sd--)
                        {
                            ++x.e;
                            ArrayExtensions.Unshift(ref xc, 1);
                        }
                    }
                }

                // Remove trailing zeros.
                for (sd = xc.LongLength; xc[--sd] == 0;) ArrayExtensions.Pop(ref xc);
            }

            x.c = xc;

            return x;
        }

        /// <summary>
        /// Handles P.toExponential, P.toFixed, P.toJSON, P.toPrecision, P.toString and P.valueOf.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="doExponential"></param>
        /// <param name="isNonzero"></param>
        /// <returns>A string representing the value of Big x in normal or exponential notation.</returns>
        internal static string stringify(Big x, bool doExponential, bool isNonzero)
        {
            var e = x.e;
            var s = string.Join("", x.c);
            var n = s.Length;

            // Exponential notation?
            if (doExponential)
            {
                s = int.Parse(s.ElementAt(0).ToString(CultureInfo.InvariantCulture)) + (n > 1 ? "." + s.Substring(1) : "") + (e < 0 ? "e" : "e+") + e;

                // Normal notation.
            }
            else if (e < 0)
            {
                for (; (0 != ++e);) s = "0" + s;
                s = "0." + s;
            }
            else if (e > 0)
            {
                if (++e > n)
                {
                    for (e -= n; (0 != e--);) s += "0";
                }
                else if (e < n)
                {
                    s = s.Substring(0, (int)e) + "." + s.Substring((int)e);
                }
            }
            else if (n > 1)
            {
                s = int.Parse(s.ElementAt(0).ToString(CultureInfo.InvariantCulture)) + "." + s.Substring(1);
            }

            return x.s < 0 && isNonzero ? "-" + s : s;
        }
    }
}
