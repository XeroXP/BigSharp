using BigSharp.Extensions;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BigSharp
{
    public class Big
    {
        /************************************** EDITABLE DEFAULTS *****************************************/


        // The default values below must be integers within the stated ranges.

        /// <summary>
        /// The maximum number of decimal places (DP) of the results of operations involving division:
        /// <br>div and sqrt, and pow with negative exponents.</br>
        /// </summary>
        public static int DP = 20;            // 0 to MAX_DP

        /// <summary>
        /// The rounding mode (RM) used when rounding to the above decimal places.
        /// <br></br>
        /// <br>0  Towards zero (i.e. truncate, no rounding).       (ROUND_DOWN)</br>
        /// <br>1  To nearest neighbour. If equidistant, round up.  (ROUND_HALF_UP)</br>
        /// <br>2  To nearest neighbour. If equidistant, to even.   (ROUND_HALF_EVEN)</br>
        /// <br>3  Away from zero.                                  (ROUND_UP)</br>
        /// </summary>
        public static RoundingMode RM = RoundingMode.ROUND_HALF_UP;             // 0, 1, 2 or 3

        /// <summary>
        /// The maximum value of DP and Big.DP.
        /// </summary>
        public static int MAX_DP = (int)1E6;       // 0 to 1000000

        /// <summary>
        /// The maximum magnitude of the exponent argument to the pow method.
        /// </summary>
        public static int MAX_POWER = (int)1E6;    // 1 to 1000000

        /// <summary>
        /// The negative exponent (NE) at and beneath which toString returns exponential notation.
        /// <br>(JavaScript numbers: -7)</br>
        /// <br>-1000000 is the minimum recommended exponent value of a Big.</br>
        /// </summary>
        public static int NE = -7;            // 0 to -1000000

        /*
         * The positive exponent (PE) at and above which toString returns exponential notation.
         * (JavaScript numbers: 21)
         * 1000000 is the maximum recommended exponent value of a Big, but this limit is not enforced.
         */
        /// <summary>
        /// The positive exponent (PE) at and above which toString returns exponential notation.
        /// <br>(JavaScript numbers: 21)</br>
        /// <br>1000000 is the maximum recommended exponent value of a Big, but this limit is not enforced.</br>
        /// </summary>
        public static int PE = 21;            // 0 to 1000000

        /// <summary>
        /// When true, an error will be thrown if a primitive number is passed to the Big constructor,
        /// <br>or if valueOf is called, or if toNumber is called on a Big which cannot be converted to a</br>
        /// <br>primitive number without a loss of precision.</br>
        /// </summary>
        public static bool STRICT = false;     // true or false


        /**************************************************************************************************/


        // Error messages.
        private static readonly string NAME = "[BigSharp] ",
        INVALID = NAME + "Invalid ",
        INVALID_DP = INVALID + "decimal places",
        INVALID_RM = INVALID + "rounding mode",
        DIV_BY_ZERO = NAME + "Division by zero";

        private static readonly string NUMERIC = @"^-?(\d+(\.\d*)?|\.\d+)(e[+-]?\d+)?$";

        public int s;
        public long e;
        public long[] c;

        public Big(Big n)
        {
            if (n == null)
                throw new BigException(INVALID + "Big");
            this.s = n.s;
            this.e = n.e;
            this.c = n.c.Slice();
        }

        public Big(string n)
        {
            if (n == null)
                throw new BigException(INVALID + "string");

            parse(this, n);
        }
        public Big(int n)
        {
            if (Big.STRICT == true)
            {
                throw new BigException(INVALID + "value");
            }

            var nString = n == 0 && 1 / (double)n < 0 ? "-0" : n.ToString(CultureInfo.InvariantCulture);

            parse(this, nString);
        }
        public Big(long n)
        {
            var nString = n == 0 && 1 / (double)n < 0 ? "-0" : n.ToString(CultureInfo.InvariantCulture);

            parse(this, nString);
        }

        public Big(double n)
        {
            var nString = n == 0 && 1 / n < 0 ? "-0" : n.ToString(CultureInfo.InvariantCulture);

            parse(this, nString);
        }

        public Big(decimal n)
        {
            var nString = n == 0 && 1 / n < 0 ? "-0" : n.ToString(CultureInfo.InvariantCulture);

            parse(this, nString);
        }

        /// <summary>
        /// Parse the number or string value passed to a Big constructor.
        /// </summary>
        /// <param name="x">{Big} A Big number instance.</param>
        /// <param name="n">{string} A numeric value.</param>
        /// <returns></returns>
        /// <exception cref="BigException"></exception>
        private Big parse(Big x, string n)
        {
            long e;
            int i, nl;

            if (!Regex.IsMatch(n, NUMERIC, RegexOptions.IgnoreCase))
            {
                throw new BigException(INVALID + "number");
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
        private Big round(Big x, long sd, RoundingMode? rm, bool more = false)
        {
            var xc = x.c;

            if (rm == null) rm = Big.RM;

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
        private string stringify(Big x, bool doExponential, bool isNonzero)
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

        // Prototype/instance methods


        /// <summary>
        /// 
        /// </summary>
        /// <returns>A new Big whose value is the absolute value of this Big.</returns>
        public Big Abs()
        {
            var x = new Big(this);
            x.s = 1;
            return x;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns><br>1 if the value of this Big is greater than the value of Big y,</br>
        /// <br>-1 if the value of this Big is less than the value of Big y, or</br>
        /// <br>0 if they have the same value.</br></returns>
        public long Cmp(string y) => _cmp(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns><br>1 if the value of this Big is greater than the value of Big y,</br>
        /// <br>-1 if the value of this Big is less than the value of Big y, or</br>
        /// <br>0 if they have the same value.</br></returns>
        public long Cmp(int y) => _cmp(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns><br>1 if the value of this Big is greater than the value of Big y,</br>
        /// <br>-1 if the value of this Big is less than the value of Big y, or</br>
        /// <br>0 if they have the same value.</br></returns>
        public long Cmp(long y) => _cmp(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns><br>1 if the value of this Big is greater than the value of Big y,</br>
        /// <br>-1 if the value of this Big is less than the value of Big y, or</br>
        /// <br>0 if they have the same value.</br></returns>
        public long Cmp(double y) => _cmp(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns><br>1 if the value of this Big is greater than the value of Big y,</br>
        /// <br>-1 if the value of this Big is less than the value of Big y, or</br>
        /// <br>0 if they have the same value.</br></returns>
        public long Cmp(decimal y) => _cmp(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns><br>1 if the value of this Big is greater than the value of Big y,</br>
        /// <br>-1 if the value of this Big is less than the value of Big y, or</br>
        /// <br>0 if they have the same value.</br></returns>
        public long Cmp(Big y) => _cmp(new Big(y));
        private long _cmp(Big y)
        {
            bool isneg;
            var x = this;
            var xc = x.c;
            var yc = y.c;
            long i = x.s;
            long j = y.s;
            var k = x.e;
            var l = y.e;

            // Either zero?
            if (xc[0] == 0 || yc[0] == 0) return xc[0] == 0 ? yc[0] == 0 ? 0 : -j : i;

            // Signs differ?
            if (i != j) return i;

            isneg = i < 0;

            // Compare exponents.
            if (k != l) return k > l ^ isneg ? 1 : -1;

            j = (k = xc.LongLength) < (l = yc.LongLength) ? k : l;

            // Compare digit by digit.
            for (i = -1; ++i < j;)
            {
                if (xc[i] != yc[i]) return xc[i] > yc[i] ^ isneg ? 1 : -1;
            }

            // Compare lengths.
            return k == l ? 0 : k > l ^ isneg ? 1 : -1;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns><br>A new Big whose value is the value of this Big divided by the value of Big y, rounded,</br>
        /// <br>if necessary, to a maximum of Big.DP decimal places using rounding mode Big.RM.</br></returns>
        public Big Div(string y) => _div(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns><br>A new Big whose value is the value of this Big divided by the value of Big y, rounded,</br>
        /// <br>if necessary, to a maximum of Big.DP decimal places using rounding mode Big.RM.</br></returns>
        public Big Div(int y) => _div(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns><br>A new Big whose value is the value of this Big divided by the value of Big y, rounded,</br>
        /// <br>if necessary, to a maximum of Big.DP decimal places using rounding mode Big.RM.</br></returns>
        public Big Div(long y) => _div(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns><br>A new Big whose value is the value of this Big divided by the value of Big y, rounded,</br>
        /// <br>if necessary, to a maximum of Big.DP decimal places using rounding mode Big.RM.</br></returns>
        public Big Div(double y) => _div(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns><br>A new Big whose value is the value of this Big divided by the value of Big y, rounded,</br>
        /// <br>if necessary, to a maximum of Big.DP decimal places using rounding mode Big.RM.</br></returns>
        public Big Div(decimal y) => _div(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns><br>A new Big whose value is the value of this Big divided by the value of Big y, rounded,</br>
        /// <br>if necessary, to a maximum of Big.DP decimal places using rounding mode Big.RM.</br></returns>
        public Big Div(Big y) => _div(new Big(y));
        private Big _div(Big y)
        {
            var x = this;
            var a = x.c;                  // dividend
            var b = y.c;   // divisor
            long k = x.s == y.s ? 1 : -1;
            var dp = Big.DP;

            if (dp != ~~dp || dp < 0 || dp > MAX_DP)
            {
                throw new BigException(INVALID_DP);
            }

            // Divisor is zero?
            if (b[0] == 0)
            {
                throw new BigException(DIV_BY_ZERO);
            }

            // Dividend is 0? Return +-0.
            if (a[0] == 0)
            {
                y.s = (int)k;
                y.e = 0;
                y.c = new long[] { 0 };
                return y;
            }

            long bl, ri;
            int n, cmp = 0;
            long[] bt;
            var bz = b.Slice();
            var ai = bl = b.LongLength;
            var al = a.LongLength;
            long[] r = a.Slice(0, bl);   // remainder
            var rl = r.LongLength;
            var q = y;                // quotient
            var qc = q.c = new long[0];
            var qi = 0;
            var p = dp + (q.e = x.e - y.e) + 1;    // precision of the result

            q.s = (int)k;
            k = p < 0 ? 0 : p;

            // Create version of divisor with leading zero.
            ArrayExtensions.Unshift(ref bz, 0);

            // Add zeros to make remainder as long as divisor.
            for (; rl++ < bl;) ArrayExtensions.Push(ref r, 0);

            do
            {

                // n is how many times the divisor goes into current remainder.
                for (n = 0; n < 10; n++)
                {

                    // Compare divisor and remainder.
                    if (bl != (rl = r.LongLength))
                    {
                        cmp = bl > rl ? 1 : -1;
                    }
                    else
                    {
                        for (ri = -1, cmp = 0; ++ri < bl;)
                        {
                            if (b[ri] != r[ri])
                            {
                                cmp = b[ri] > r[ri] ? 1 : -1;
                                break;
                            }
                        }
                    }

                    // If divisor < remainder, subtract divisor from remainder.
                    if (cmp < 0)
                    {

                        // Remainder can't be more than 1 digit longer than divisor.
                        // Equalise lengths using divisor with extra leading zero?
                        for (bt = rl == bl ? b : bz; rl != 0;)
                        {
                            if (r[--rl] < bt[rl])
                            {
                                ri = rl;
                                for (; ri != 0 && r[--ri] == 0;) r[ri] = 9;
                                --r[ri];
                                r[rl] += 10;
                            }
                            r[rl] -= bt[rl];
                        }

                        for (; r[0] == 0;) ArrayExtensions.Shift(ref r);
                    }
                    else
                    {
                        break;
                    }
                }

                // Add the digit n to the result array.
                if (qc.LongLength <= qi) ArrayExtensions.Resize(ref qc, qi + 1);
                qc[qi++] = (cmp != 0) ? n : ++n;

                // Update the remainder.
                if (r[0] != 0 && cmp != 0)
                {
                    if (r.LongLength <= rl) ArrayExtensions.Resize(ref r, rl + 1);
                    r[rl] = ai < a.LongLength ? a[ai] : 0;
                }
                else r = ai < a.LongLength ? new long[] { a[ai] } : new long[0];

            } while ((ai++ < al || r.LongLength > 0) && (0 != k--));

            // Leading zero? Do not remove if result is simply zero (qi == 1).
            if (qc[0] == 0 && qi != 1)
            {

                // There can't be more than one zero.
                ArrayExtensions.Shift(ref qc);
                q.e--;
                p--;
            }

            q.c = qc;

            // Round?
            if (qi > p) round(q, p, Big.RM, r.LongLength > 0);

            return q;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is equal to the value of Big y, otherwise return false.</returns>
        public bool Eq(string y)
        {
            return this.Cmp(y) == 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is equal to the value of Big y, otherwise return false.</returns>
        public bool Eq(int y)
        {
            return this.Cmp(y) == 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is equal to the value of Big y, otherwise return false.</returns>
        public bool Eq(long y)
        {
            return this.Cmp(y) == 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is equal to the value of Big y, otherwise return false.</returns>
        public bool Eq(double y)
        {
            return this.Cmp(y) == 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is equal to the value of Big y, otherwise return false.</returns>
        public bool Eq(decimal y)
        {
            return this.Cmp(y) == 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is equal to the value of Big y, otherwise return false.</returns>
        public bool Eq(Big y)
        {
            return this.Cmp(y) == 0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is greater than the value of Big y, otherwise return false.</returns>
        public bool Gt(string y)
        {
            return this.Cmp(y) > 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is greater than the value of Big y, otherwise return false.</returns>
        public bool Gt(int y)
        {
            return this.Cmp(y) > 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is greater than the value of Big y, otherwise return false.</returns>
        public bool Gt(long y)
        {
            return this.Cmp(y) > 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is greater than the value of Big y, otherwise return false.</returns>
        public bool Gt(double y)
        {
            return this.Cmp(y) > 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is greater than the value of Big y, otherwise return false.</returns>
        public bool Gt(decimal y)
        {
            return this.Cmp(y) > 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is greater than the value of Big y, otherwise return false.</returns>
        public bool Gt(Big y)
        {
            return this.Cmp(y) > 0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is greater than or equal to the value of Big y, otherwise return false.</returns>
        public bool Gte(string y)
        {
            return this.Cmp(y) > -1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is greater than or equal to the value of Big y, otherwise return false.</returns>
        public bool Gte(int y)
        {
            return this.Cmp(y) > -1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is greater than or equal to the value of Big y, otherwise return false.</returns>
        public bool Gte(long y)
        {
            return this.Cmp(y) > -1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is greater than or equal to the value of Big y, otherwise return false.</returns>
        public bool Gte(double y)
        {
            return this.Cmp(y) > -1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is greater than or equal to the value of Big y, otherwise return false.</returns>
        public bool Gte(decimal y)
        {
            return this.Cmp(y) > -1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is greater than or equal to the value of Big y, otherwise return false.</returns>
        public bool Gte(Big y)
        {
            return this.Cmp(y) > -1;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is less than the value of Big y, otherwise return false.</returns>
        public bool Lt(string y)
        {
            return this.Cmp(y) < 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is less than the value of Big y, otherwise return false.</returns>
        public bool Lt(int y)
        {
            return this.Cmp(y) < 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is less than the value of Big y, otherwise return false.</returns>
        public bool Lt(long y)
        {
            return this.Cmp(y) < 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is less than the value of Big y, otherwise return false.</returns>
        public bool Lt(double y)
        {
            return this.Cmp(y) < 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is less than the value of Big y, otherwise return false.</returns>
        public bool Lt(decimal y)
        {
            return this.Cmp(y) < 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is less than the value of Big y, otherwise return false.</returns>
        public bool Lt(Big y)
        {
            return this.Cmp(y) < 0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is less than or equal to the value of Big y, otherwise return false.</returns>
        public bool Lte(string y)
        {
            return this.Cmp(y) < 1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is less than or equal to the value of Big y, otherwise return false.</returns>
        public bool Lte(int y)
        {
            return this.Cmp(y) < 1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is less than or equal to the value of Big y, otherwise return false.</returns>
        public bool Lte(long y)
        {
            return this.Cmp(y) < 1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is less than or equal to the value of Big y, otherwise return false.</returns>
        public bool Lte(double y)
        {
            return this.Cmp(y) < 1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is less than or equal to the value of Big y, otherwise return false.</returns>
        public bool Lte(decimal y)
        {
            return this.Cmp(y) < 1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is less than or equal to the value of Big y, otherwise return false.</returns>
        public bool Lte(Big y)
        {
            return this.Cmp(y) < 1;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big minus the value of Big y.</returns>
        public Big Sub(string y) => _sub(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big minus the value of Big y.</returns>
        public Big Sub(int y) => _sub(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big minus the value of Big y.</returns>
        public Big Sub(long y) => _sub(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big minus the value of Big y.</returns>
        public Big Sub(double y) => _sub(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big minus the value of Big y.</returns>
        public Big Sub(decimal y) => _sub(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big minus the value of Big y.</returns>
        public Big Sub(Big y) => _sub(new Big(y));
        private Big _sub(Big y)
        {
            long i, j;
            long[] t;
            bool xlty;
            var x = this;
            long a = x.s;
            long b = y.s;

            // Signs differ?
            if (a != b)
            {
                y.s = (int)-b;
                return x.Plus(y);
            }

            var xc = x.c.Slice();
            var xe = x.e;
            var yc = y.c;
            var ye = y.e;

            // Either zero?
            if (xc[0] == 0 || yc[0] == 0)
            {
                if (yc[0] != 0)
                {
                    y.s = (int)-b;
                }
                else if (xc[0] != 0)
                {
                    y = new Big(x);
                }
                else
                {
                    y.s = 1;
                }
                return y;
            }

            // Determine which is the bigger number. Prepend zeros to equalise exponents.
            if ((a = xe - ye) != 0)
            {
                bool isYc = false;
                if (xlty = a < 0)
                {
                    a = -a;
                    t = xc;
                }
                else
                {
                    ye = xe;
                    t = yc;
                    isYc = true;
                }

                ArrayExtensions.Reverse(ref t);
                for (b = a; (0 != b--);) ArrayExtensions.Push(ref t, 0);
                ArrayExtensions.Reverse(ref t);


                if (isYc)
                {
                    yc = t;
                }
                else
                {
                    xc = t;
                }
            }
            else
            {

                // Exponents equal. Check digit by digit.
                j = ((xlty = xc.LongLength < yc.LongLength) ? xc : yc).LongLength;

                for (a = b = 0; b < j; b++)
                {
                    if (xc[b] != yc[b])
                    {
                        xlty = xc[b] < yc[b];
                        break;
                    }
                }
            }

            // x < y? Point xc to the array of the bigger number.
            if (xlty)
            {
                t = xc;
                xc = yc;
                yc = t;
                y.s = -y.s;
            }

            /*
             * Append zeros to xc if shorter. No need to add zeros to yc if shorter as subtraction only
             * needs to start at yc.LongLength.
             */
            if ((b = (j = yc.LongLength) - (i = xc.LongLength)) > 0)
            {
                for (; (0 != b--);)
                {
                    if (xc.LongLength < i + 1) ArrayExtensions.Resize(ref xc, i + 1);
                    xc[i++] = 0;
                }
            }

            // Subtract yc from xc.
            for (b = i; j > a;)
            {
                if (xc[--j] < yc[j])
                {
                    for (i = j; i != 0 && xc[--i] == 0;) xc[i] = 9;
                    --xc[i];
                    xc[j] += 10;
                }

                xc[j] -= yc[j];
            }

            // Remove trailing zeros.
            for (; b > 0 && xc[--b] == 0;) ArrayExtensions.Pop(ref xc);

            // Remove leading zeros and adjust exponent accordingly.
            for (; xc.LongLength > 0 && xc[0] == 0;)
            {
                ArrayExtensions.Shift(ref xc);
                --ye;
            }

            if (xc.LongLength > 0 && xc[0] == 0 || xc.LongLength == 0)
            {

                // n - n = +0
                y.s = 1;

                // Result must be zero.
                ye = 0;
                xc = new long[] { 0 };
            }

            y.c = xc;
            y.e = ye;

            return y;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big minus the value of Big y.</returns>
        public Big Minus(string y) => Sub(y);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big minus the value of Big y.</returns>
        public Big Minus(int y) => Sub(y);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big minus the value of Big y.</returns>
        public Big Minus(long y) => Sub(y);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big minus the value of Big y.</returns>
        public Big Minus(double y) => Sub(y);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big minus the value of Big y.</returns>
        public Big Minus(decimal y) => Sub(y);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big minus the value of Big y.</returns>
        public Big Minus(Big y) => Sub(y);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big modulo the value of Big y.</returns>
        public Big Mod(string y) => _mod(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big modulo the value of Big y.</returns>
        public Big Mod(int y) => _mod(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big modulo the value of Big y.</returns>
        public Big Mod(long y) => _mod(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big modulo the value of Big y.</returns>
        public Big Mod(double y) => _mod(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big modulo the value of Big y.</returns>
        public Big Mod(decimal y) => _mod(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big modulo the value of Big y.</returns>
        public Big Mod(Big y) => _mod(new Big(y));
        private Big _mod(Big y)
        {
            bool ygtx;
            var x = this;
            var a = x.s;
            var b = y.s;

            if (y.c[0] == 0)
            {
                throw new BigException(DIV_BY_ZERO);
            }

            x.s = y.s = 1;
            ygtx = y.Cmp(x) == 1;
            x.s = a;
            y.s = b;

            if (ygtx) return new Big(x);

            a = Big.DP;
            var rm = Big.RM;
            Big.DP = 0;
            Big.RM = RoundingMode.ROUND_DOWN;
            x = x.Div(y);
            Big.DP = a;
            Big.RM = rm;

            return this.Minus(x.Times(y));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>A new Big whose value is the value of this Big negated.</returns>
        public Big Neg()
        {
            var x = new Big(this);
            x.s = -x.s;
            return x;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big plus the value of Big y.</returns>
        public Big Add(string y) => _add(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big plus the value of Big y.</returns>
        public Big Add(int y) => _add(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big plus the value of Big y.</returns>
        public Big Add(long y) => _add(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big plus the value of Big y.</returns>
        public Big Add(double y) => _add(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big plus the value of Big y.</returns>
        public Big Add(decimal y) => _add(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big plus the value of Big y.</returns>
        public Big Add(Big y) => _add(new Big(y));
        private Big _add(Big y)
        {
            long e, k;
            long[] t;
            var x = this;

            // Signs differ?
            if (x.s != y.s)
            {
                y.s = -y.s;
                return x.Minus(y);
            }

            var xe = x.e;
            var xc = x.c;
            var ye = y.e;
            var yc = y.c;

            // Either zero?
            if (xc[0] == 0 || yc[0] == 0)
            {
                if (yc[0] == 0)
                {
                    if (xc[0] != 0)
                    {
                        y = new Big(x);
                    }
                    else
                    {
                        y.s = x.s;
                    }
                }
                return y;
            }

            xc = xc.Slice();

            // Prepend zeros to equalise exponents.
            // Note: reverse faster than unshifts.
            if ((e = xe - ye) != 0)
            {
                bool isYc = false;
                if (e > 0)
                {
                    ye = xe;
                    t = yc;
                    isYc = true;
                }
                else
                {
                    e = -e;
                    t = xc;
                }

                ArrayExtensions.Reverse(ref t);
                for (; (0 != e--);) ArrayExtensions.Push(ref t, 0);
                ArrayExtensions.Reverse(ref t);

                if (isYc)
                {
                    yc = t;
                }
                else
                {
                    xc = t;
                }
            }

            // Point xc to the longer array.
            if (xc.LongLength - yc.LongLength < 0)
            {
                t = yc;
                yc = xc;
                xc = t;
            }

            e = yc.LongLength;

            // Only start adding at yc.LongLength - 1 as the further digits of xc can be left as they are.
            for (k = 0; (e != 0); xc[e] %= 10) k = (xc[--e] = xc[e] + yc[e] + k) / 10 | 0;

            // No need to check for zero, as +x + +y != 0 && -x + -y != 0

            if (k != 0)
            {
                ArrayExtensions.Unshift(ref xc, k);
                ++ye;
            }

            // Remove trailing zeros.
            for (e = xc.LongLength; xc[--e] == 0;) ArrayExtensions.Pop(ref xc);

            y.c = xc;
            y.e = ye;

            return y;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big plus the value of Big y.</returns>
        public Big Plus(string y) => Add(y);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big plus the value of Big y.</returns>
        public Big Plus(int y) => Add(y);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big plus the value of Big y.</returns>
        public Big Plus(long y) => Add(y);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big plus the value of Big y.</returns>
        public Big Plus(double y) => Add(y);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big plus the value of Big y.</returns>
        public Big Plus(decimal y) => Add(y);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big plus the value of Big y.</returns>
        public Big Plus(Big y) => Add(y);


        /// <summary>
        /// If n is negative, round to a maximum of Big.DP decimal places using rounding mode Big.RM.
        /// </summary>
        /// <param name="n">{number} Integer, -MAX_POWER to MAX_POWER inclusive.</param>
        /// <returns>A Big whose value is the value of this Big raised to the power n.</returns>
        /// <exception cref="BigException"></exception>
        public Big Pow(int n)
        {
            var x = this;
            var one = new Big("1");
            var y = one;
            var isneg = n < 0;

            if (n != ~~n || n < -MAX_POWER || n > MAX_POWER)
            {
                throw new BigException(INVALID + "exponent");
            }

            if (isneg) n = -n;

            for (; ; )
            {
                if ((n & 1) != 0) y = y.Times(x);
                n >>= 1;
                if (n == 0) break;
                x = x.Times(x);
            }

            return isneg ? one.Div(y) : y;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sd">{number} Significant digits: integer, 1 to MAX_DP inclusive.</param>
        /// <param name="rm">{RoundingMode} Rounding mode.</param>
        /// <returns><br>A new Big whose value is the value of this Big rounded to a maximum precision of sd</br>
        /// <br>significant digits using rounding mode rm, or Big.RM if rm is not specified.</br></returns>
        /// <exception cref="BigException"></exception>
        public Big Prec(int sd, RoundingMode? rm = null)
        {
            if (sd != ~~sd || sd < 1 || sd > MAX_DP)
            {
                throw new BigException(INVALID + "precision");
            }
            return round(new Big(this), sd, rm);
        }


        /// <summary>
        /// If dp is negative, round to an integer which is a multiple of 10**-dp.
        /// <br>If dp is not specified, round to 0 decimal places.</br>
        /// </summary>
        /// <param name="dp">{number} Integer, -MAX_DP to MAX_DP inclusive.</param>
        /// <param name="rm">{RoundingMode} Rounding mode.</param>
        /// <returns><br>A new Big whose value is the value of this Big rounded to a maximum of dp decimal places</br>
        /// <br>using rounding mode rm, or Big.RM if rm is not specified.</br></returns>
        /// <exception cref="BigException"></exception>
        public Big Round(int? dp, RoundingMode? rm)
        {
            if (dp == null) dp = 0;
            else if (dp != ~~dp || dp < -MAX_DP || dp > MAX_DP)
            {
                throw new BigException(INVALID_DP);
            }
            return round(new Big(this), dp.Value + this.e + 1, rm);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns><br>Return a new Big whose value is the square root of the value of this Big, rounded, if</br>
        /// <br>necessary, to a maximum of Big.DP decimal places using rounding mode Big.RM.</br></returns>
        /// <exception cref="BigException"></exception>
        public Big Sqrt()
        {
            Big r, t;
            string c;
            var x = this;
            double s = x.s;
            var e = x.e;
            var half = new Big("0.5");

            // Zero?
            if (x.c[0] == 0) return new Big(x);

            // Negative?
            if (s < 0)
            {
                throw new BigException(NAME + "No square root");
            }

            // Estimate.
            s = Math.Sqrt(x.ToNumber());

            // Math.sqrt underflow/overflow?
            // Re-estimate: pass x coefficient to Math.sqrt as integer, then adjust the result exponent.
            if (s == 0 || s == double.PositiveInfinity)
            {
                c = string.Join("", x.c);
                if ((c.Length + e & 1) == 0) c += '0';
                s = Math.Sqrt(double.Parse(c));
                e = ((e + 1) / 2 | 0) - (e < 0 ? 1 : e & 1);
                var sString = s.ToExponential();
                r = new Big((s == double.PositiveInfinity ? "5e" : sString.Substring(0, sString.IndexOf('e') + 1)) + e);
            }
            else
            {
                r = new Big(s.ToString(CultureInfo.InvariantCulture));
            }

            e = r.e + (Big.DP += 4);

            // Newton-Raphson iteration.
            do
            {
                t = r;
                r = half.Times(t.Plus(x.Div(t)));
            } while (string.Join("", t.c.Slice(0, e)) != string.Join("", r.c.Slice(0, e)));

            return round(r, (Big.DP -= 4) + r.e + 1, Big.RM);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big times the value of Big y.</returns>
        public Big Mul(string y) => _mul(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big times the value of Big y.</returns>
        public Big Mul(int y) => _mul(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big times the value of Big y.</returns>
        public Big Mul(long y) => _mul(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big times the value of Big y.</returns>
        public Big Mul(double y) => _mul(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big times the value of Big y.</returns>
        public Big Mul(decimal y) => _mul(new Big(y));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big times the value of Big y.</returns>
        public Big Mul(Big y) => _mul(new Big(y));
        private Big _mul(Big y)
        {
            long[] c;
            var x = this;
            var xc = x.c;
            var yc = y.c;
            var a = xc.LongLength;
            var b = yc.LongLength;
            var i = x.e;
            var j = y.e;

            // Determine sign of result.
            y.s = x.s == y.s ? 1 : -1;

            // Return signed 0 if either 0.
            if (xc[0] == 0 || yc[0] == 0)
            {
                y.e = 0;
                y.c = new long[] { 0 };
                return y;
            }

            // Initialise exponent of result as x.e + y.e.
            y.e = i + j;

            // If array xc has fewer digits than yc, swap xc and yc, and lengths.
            if (a < b)
            {
                c = xc;
                xc = yc;
                yc = c;
                j = a;
                a = b;
                b = j;
            }

            // Initialise coefficient array of result with zeros.
            for (c = new long[j = a + b]; (0 != j--);) c[j] = 0;

            // Multiply.

            // i is initially xc.LongLength.
            for (i = b; (0 != i--);)
            {
                b = 0;

                // a is yc.LongLength.
                for (j = a + i; j > i;)
                {

                    // Current sum of products at this digit position, plus carry.
                    b = c[j] + yc[i] * xc[j - i - 1] + b;
                    c[j--] = b % 10;

                    // carry
                    b = b / 10 | 0;
                }

                c[j] = b;
            }

            // Increment result exponent if there is a final carry, otherwise remove leading zero.
            if (b != 0) ++y.e;
            else ArrayExtensions.Shift(ref c);

            // Remove trailing zeros.
            for (i = c.LongLength; 0 == c[--i];) ArrayExtensions.Pop(ref c);
            y.c = c;

            return y;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big times the value of Big y.</returns>
        public Big Times(string y) => Mul(y);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big times the value of Big y.</returns>
        public Big Times(int y) => Mul(y);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big times the value of Big y.</returns>
        public Big Times(long y) => Mul(y);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big times the value of Big y.</returns>
        public Big Times(double y) => Mul(y);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big times the value of Big y.</returns>
        public Big Times(decimal y) => Mul(y);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big times the value of Big y.</returns>
        public Big Times(Big y) => Mul(y);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dp">{number} Decimal places: integer, 0 to MAX_DP inclusive.</param>
        /// <param name="rm">{RoundingMode} Rounding mode.</param>
        /// <returns><br>A string representing the value of this Big in exponential notation rounded to dp fixed</br>
        /// <br>decimal places using rounding mode rm, or Big.RM if rm is not specified.</br></returns>
        /// <exception cref="BigException"></exception>
        public string ToExponential(int? dp = null, RoundingMode? rm = null)
        {
            var x = this;
            var n = x.c[0];

            if (dp != null)
            {
                if (dp != ~~dp || dp < 0 || dp > MAX_DP)
                {
                    throw new BigException(INVALID_DP);
                }
                x = round(new Big(x), (int)(++dp), rm);
                for (; x.c.LongLength < dp;) ArrayExtensions.Push(ref x.c, 0);
            }

            return stringify(x, true, n != 0);
        }


        /// <summary>
        /// ('-0').toFixed(0) is '0', but (-0.1).toFixed(0) is '-0'.
        /// <br>('-0').toFixed(1) is '0.0', but (-0.01).toFixed(1) is '-0.0'.</br>
        /// </summary>
        /// <param name="dp">{number} Decimal places: integer, 0 to MAX_DP inclusive.</param>
        /// <param name="rm">{RoundingMode} Rounding mode.</param>
        /// <returns><br>A string representing the value of this Big in normal notation rounded to dp fixed</br>
        /// <br>decimal places using rounding mode rm, or Big.RM if rm is not specified.</br></returns>
        /// <exception cref="BigException"></exception>
        public string ToFixed(long? dp = null, RoundingMode? rm = null)
        {
            var x = this;
            var n = x.c[0];

            if (dp != null)
            {
                if (dp != ~~dp || dp < 0 || dp > MAX_DP)
                {
                    throw new BigException(INVALID_DP);
                }
                x = round(new Big(x), dp.Value + x.e + 1, rm);

                // x.e may have changed if the value is rounded up.
                for (dp = dp + x.e + 1; x.c.LongLength < dp;) ArrayExtensions.Push(ref x.c, 0);
            }

            return stringify(x, false, n != 0);
        }


        /// <summary>
        /// Omit the sign for negative zero.
        /// </summary>
        /// <returns><br>A string representing the value of this Big.</br>
        /// <br>Exponential notation if this Big has a positive exponent equal to or greater than</br>
        /// <br>Big.PE, or a negative exponent equal to or less than Big.NE.</br></returns>
        public override string ToString()
        {
            var x = this;
            return stringify(x, x.e <= Big.NE || x.e >= Big.PE, x.c[0] != 0);
        }

        /// <summary>
        /// Same as ToString
        /// </summary>
        /// <returns></returns>
        public string ToJSON() => ToString();


        /// <summary>
        /// 
        /// </summary>
        /// <returns>The value of this Big as a primitve number.</returns>
        /// <exception cref="BigException"></exception>
        public double ToNumber()
        {
            var n = double.Parse(stringify(this, true, true), CultureInfo.InvariantCulture);
            if (Big.STRICT == true && !this.Eq(n.ToString(CultureInfo.InvariantCulture)))
            {
                throw new BigException(NAME + "Imprecise conversion");
            }
            return n;
        }


        /// <summary>
        /// Use exponential notation if sd is less than the number of digits necessary to represent
        /// <br>the integer part of the value in normal notation.</br>
        /// </summary>
        /// <param name="sd">{number} Significant digits: integer, 1 to MAX_DP inclusive.</param>
        /// <param name="rm">{RoundingMode} Rounding mode.</param>
        /// <returns><br>A string representing the value of this Big rounded to sd significant digits using</br>
        /// <br>rounding mode rm, or Big.RM if rm is not specified.</br></returns>
        /// <exception cref="BigException"></exception>
        public string ToPrecision(int? sd, RoundingMode? rm = null)
        {
            var x = this;
            var n = x.c[0];

            if (sd.HasValue)
            {
                if (sd != ~~sd || sd < 1 || sd > MAX_DP)
                {
                    throw new BigException(INVALID + "precision");
                }
                x = round(new Big(x), sd.Value, rm);
                for (; x.c.LongLength < sd;) ArrayExtensions.Push(ref x.c, 0);
            }

            return stringify(x, sd <= x.e || x.e <= Big.NE || x.e >= Big.PE, n != 0);
        }


        /// <summary>
        /// Include the sign for negative zero.
        /// </summary>
        /// <returns><br>A string representing the value of this Big.</br>
        /// <br>Exponential notation if this Big has a positive exponent equal to or greater than</br>
        /// <br>Big.PE, or a negative exponent equal to or less than Big.NE.</br></returns>
        /// <exception cref="BigException"></exception>
        public string ValueOf()
        {
            var x = this;
            if (Big.STRICT == true)
            {
                throw new BigException(NAME + "valueOf disallowed");
            }
            return stringify(x, x.e <= Big.NE || x.e >= Big.PE, true);
        }
    }
}