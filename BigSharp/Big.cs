using BigSharp.Extensions;
using System.Globalization;

namespace BigSharp
{
    public class Big
    {
        public BigConfig Config;

        public int s;
        public long e;
        public long[] c;

        public Big(BigConfig? config = null)
        {
            this.Config = config ?? new BigConfig();

            this.s = 0;
            this.e = 0;
            this.c = new long[0];
        }
        public Big(BigArgument n, BigConfig? config = null) : this(config)
        {
            string nString = "";
            n.Switch(
                @double =>
                {
                    if (this.Config.STRICT == true)
                        throw new BigException(BigException.INVALID + "value");

                    nString = @double == 0 && 1 / @double < 0 ? "-0" : @double.ToString(CultureInfo.InvariantCulture);
                },
                @decimal =>
                {
                    if (this.Config.STRICT == true)
                        throw new BigException(BigException.INVALID + "value");

                    nString = @decimal == 0 && 1 / (double)@decimal < 0 ? "-0" : @decimal.ToString(CultureInfo.InvariantCulture);
                },
                @long =>
                {
                    if (this.Config.STRICT == true)
                        throw new BigException(BigException.INVALID + "value");

                    nString = @long == 0 && 1 / (double)@long < 0 ? "-0" : @long.ToString(CultureInfo.InvariantCulture);
                },
                @int =>
                {
                    if (this.Config.STRICT == true)
                        throw new BigException(BigException.INVALID + "value");

                    nString = @int == 0 && 1 / (double)@int < 0 ? "-0" : @int.ToString(CultureInfo.InvariantCulture);
                },
                @string =>
                {
                    if (string.IsNullOrEmpty(@string))
                        throw new BigException(BigException.INVALID + "string");

                    nString = @string;
                },
                bigInteger =>
                {
                    nString = bigInteger.ToString(CultureInfo.InvariantCulture);
                },
                big =>
                {
                    if (big == null)
                        throw new BigException(BigException.INVALID + "Big");

                    _Big(big);
                    return;
                }
            );
            BigHelperFunctions.parse(this, nString);
        }

        private void _Big(Big n)
        {
            if (n == null)
                throw new BigException(BigException.INVALID + "Big");
            this.s = n.s;
            this.e = n.e;
            this.c = n.c.Slice();
        }

        

        // Prototype/instance methods


        /// <summary>
        /// 
        /// </summary>
        /// <returns>A new Big whose value is the absolute value of this Big.</returns>
        public Big Abs()
        {
            var x = new Big(this, this.Config);
            x.s = 1;
            return x;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>1 if the value of this Big is greater than the value of Big y,<br />
        /// -1 if the value of this Big is less than the value of Big y, or<br />
        /// 0 if they have the same value.</returns>
        public long Cmp(BigArgument y) => _cmp(new Big(y, this.Config));
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
        /// <returns>A new Big whose value is the value of this Big divided by the value of Big y, rounded,<br />
        /// if necessary, to a maximum of Big.Config.DP decimal places using rounding mode Big.Config.RM.</returns>
        public Big Div(BigArgument y) => _div(new Big(y, this.Config));
        private Big _div(Big y)
        {
            var x = this;
            var a = x.c;                  // dividend
            var b = y.c;   // divisor
            long k = x.s == y.s ? 1 : -1;
            var dp = x.Config.DP;

            if (dp != ~~dp || dp < 0 || dp > x.Config.MAX_DP)
            {
                throw new BigException(BigException.INVALID_DP);
            }

            // Divisor is zero?
            if (b[0] == 0)
            {
                throw new BigException(BigException.DIV_BY_ZERO);
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
            if (qi > p) BigHelperFunctions.round(q, p, x.Config.RM, r.LongLength > 0);

            return q;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is equal to the value of Big y, otherwise false.</returns>
        public bool Eq(BigArgument y)
        {
            return this.Cmp(y) == 0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is greater than the value of Big y, otherwise false.</returns>
        public bool Gt(BigArgument y)
        {
            return this.Cmp(y) > 0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is greater than or equal to the value of Big y, otherwise false.</returns>
        public bool Gte(BigArgument y)
        {
            return this.Cmp(y) > -1;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is less than the value of Big y, otherwise false.</returns>
        public bool Lt(BigArgument y)
        {
            return this.Cmp(y) < 0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>True if the value of this Big is less than or equal to the value of Big y, otherwise false.</returns>
        public bool Lte(BigArgument y)
        {
            return this.Cmp(y) < 1;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big minus the value of Big y.</returns>
        public Big Sub(BigArgument y) => _sub(new Big(y, this.Config));
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
                    y = new Big(x, x.Config);
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
        /// <inheritdoc cref="Sub" />
        public Big Minus(BigArgument y) => Sub(y);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big modulo the value of Big y.</returns>
        public Big Mod(BigArgument y) => _mod(new Big(y, this.Config));
        private Big _mod(Big y)
        {
            bool ygtx;
            var x = this;
            var a = x.s;
            var b = y.s;

            if (y.c[0] == 0)
            {
                throw new BigException(BigException.DIV_BY_ZERO);
            }

            x.s = y.s = 1;
            ygtx = y.Cmp(x) == 1;
            x.s = a;
            y.s = b;

            if (ygtx) return new Big(x, x.Config);

            a = x.Config.DP;
            var rm = x.Config.RM;
            x.Config.DP = 0;
            x.Config.RM = RoundingMode.ROUND_DOWN;
            x = x.Div(y);
            x.Config.DP = a;
            x.Config.RM = rm;

            return this.Minus(x.Times(y));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>A new Big whose value is the value of this Big negated.</returns>
        public Big Neg()
        {
            var x = new Big(this, this.Config);
            x.s = -x.s;
            return x;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big plus the value of Big y.</returns>
        public Big Add(BigArgument y) => _add(new Big(y, this.Config));
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
                        y = new Big(x, x.Config);
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
        /// <inheritdoc cref="Add" />
        public Big Plus(BigArgument y) => Add(y);


        /// <summary>
        /// If n is negative, round to a maximum of Big.Config.DP decimal places using rounding mode Big.Config.RM.
        /// </summary>
        /// <param name="n">{number} Integer, -MAX_POWER to MAX_POWER inclusive.</param>
        /// <returns>A Big whose value is the value of this Big raised to the power n.</returns>
        /// <exception cref="BigException"></exception>
        public Big Pow(int n)
        {
            var x = this;
            var one = new Big("1", x.Config);
            var y = one;
            var isneg = n < 0;

            if (n != ~~n || n < -x.Config.MAX_POWER || n > x.Config.MAX_POWER)
            {
                throw new BigException(BigException.INVALID + "exponent");
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
        /// <returns>A new Big whose value is the value of this Big rounded to a maximum precision of sd<br />
        /// significant digits using rounding mode rm, or Big.Config.RM if rm is not specified.</returns>
        /// <exception cref="BigException"></exception>
        public Big Prec(int sd, RoundingMode? rm = null)
        {
            if (sd != ~~sd || sd < 1 || sd > this.Config.MAX_DP)
            {
                throw new BigException(BigException.INVALID + "precision");
            }
            return BigHelperFunctions.round(new Big(this, this.Config), sd, rm);
        }


        /// <summary>
        /// If dp is negative, round to an integer which is a multiple of 10**-dp.<br />
        /// If dp is not specified, round to 0 decimal places.
        /// </summary>
        /// <param name="dp">{number} Integer, -MAX_DP to MAX_DP inclusive.</param>
        /// <param name="rm">{RoundingMode} Rounding mode.</param>
        /// <returns>A new Big whose value is the value of this Big rounded to a maximum of dp decimal places<br />
        /// using rounding mode rm, or Big.Config.RM if rm is not specified.</returns>
        /// <exception cref="BigException"></exception>
        public Big Round(int? dp, RoundingMode? rm)
        {
            if (dp == null) dp = 0;
            else if (dp != ~~dp || dp < -this.Config.MAX_DP || dp > this.Config.MAX_DP)
            {
                throw new BigException(BigException.INVALID_DP);
            }
            return BigHelperFunctions.round(new Big(this, this.Config), dp.Value + this.e + 1, rm);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>Return a new Big whose value is the square root of the value of this Big, rounded, if<br />
        /// necessary, to a maximum of Big.Config.DP decimal places using rounding mode Big.Config.RM.</returns>
        /// <exception cref="BigException"></exception>
        public Big Sqrt()
        {
            Big r, t;
            string c;
            var x = this;
            double s = x.s;
            var e = x.e;
            var half = new Big("0.5", x.Config);

            // Zero?
            if (x.c[0] == 0) return new Big(x, x.Config);

            // Negative?
            if (s < 0)
            {
                throw new BigException(BigException.NAME + "No square root");
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
                r = new Big((s == double.PositiveInfinity ? "5e" : sString.Substring(0, sString.IndexOf('e') + 1)) + e, x.Config);
            }
            else
            {
                r = new Big(s.ToString(CultureInfo.InvariantCulture), x.Config);
            }

            e = r.e + (x.Config.DP += 4);

            // Newton-Raphson iteration.
            do
            {
                t = r;
                r = half.Times(t.Plus(x.Div(t)));
            } while (string.Join("", t.c.Slice(0, e)) != string.Join("", r.c.Slice(0, e)));

            return BigHelperFunctions.round(r, (x.Config.DP -= 4) + r.e + 1, x.Config.RM);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns>A new Big whose value is the value of this Big times the value of Big y.</returns>
        public Big Mul(BigArgument y) => _mul(new Big(y, this.Config));
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
        /// <inheritdoc cref="Mul" />
        public Big Times(BigArgument y) => Mul(y);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dp">{number} Decimal places: integer, 0 to MAX_DP inclusive.</param>
        /// <param name="rm">{RoundingMode} Rounding mode.</param>
        /// <returns>A string representing the value of this Big in exponential notation rounded to dp fixed<br />
        /// decimal places using rounding mode rm, or Big.Config.RM if rm is not specified.</returns>
        /// <exception cref="BigException"></exception>
        public string ToExponential(int? dp = null, RoundingMode? rm = null)
        {
            var x = this;
            var n = x.c[0];

            if (dp != null)
            {
                if (dp != ~~dp || dp < 0 || dp > x.Config.MAX_DP)
                {
                    throw new BigException(BigException.INVALID_DP);
                }
                x = BigHelperFunctions.round(new Big(x, x.Config), (int)(++dp), rm);
                for (; x.c.LongLength < dp;) ArrayExtensions.Push(ref x.c, 0);
            }

            return BigHelperFunctions.stringify(x, true, n != 0);
        }


        /// <summary>
        /// ('-0').toFixed(0) is '0', but (-0.1).toFixed(0) is '-0'.<br />
        /// ('-0').toFixed(1) is '0.0', but (-0.01).toFixed(1) is '-0.0'.
        /// </summary>
        /// <param name="dp">{number} Decimal places: integer, 0 to MAX_DP inclusive.</param>
        /// <param name="rm">{RoundingMode} Rounding mode.</param>
        /// <returns>A string representing the value of this Big in normal notation rounded to dp fixed<br />
        /// decimal places using rounding mode rm, or Big.RM if rm is not specified.</returns>
        /// <exception cref="BigException"></exception>
        public string ToFixed(long? dp = null, RoundingMode? rm = null)
        {
            var x = this;
            var n = x.c[0];

            if (dp != null)
            {
                if (dp != ~~dp || dp < 0 || dp > x.Config.MAX_DP)
                {
                    throw new BigException(BigException.INVALID_DP);
                }
                x = BigHelperFunctions.round(new Big(x, x.Config), dp.Value + x.e + 1, rm);

                // x.e may have changed if the value is rounded up.
                for (dp = dp + x.e + 1; x.c.LongLength < dp;) ArrayExtensions.Push(ref x.c, 0);
            }

            return BigHelperFunctions.stringify(x, false, n != 0);
        }


        /// <summary>
        /// Omit the sign for negative zero.
        /// </summary>
        /// <returns>A string representing the value of this Big.<br />
        /// Exponential notation if this Big has a positive exponent equal to or greater than<br />
        /// Big.Config.PE, or a negative exponent equal to or less than Big.Config.NE.</returns>
        public override string ToString()
        {
            var x = this;
            return BigHelperFunctions.stringify(x, x.e <= x.Config.NE || x.e >= x.Config.PE, x.c[0] != 0);
        }
        /// <inheritdoc cref="ToString" />
        public string ToJSON() => ToString();


        /// <summary>
        /// 
        /// </summary>
        /// <returns>The value of this Big as a primitve number.</returns>
        /// <exception cref="BigException"></exception>
        public double ToNumber()
        {
            var n = double.Parse(BigHelperFunctions.stringify(this, true, true), CultureInfo.InvariantCulture);
            if (this.Config.STRICT == true && !this.Eq(n.ToString(CultureInfo.InvariantCulture)))
            {
                throw new BigException(BigException.NAME + "Imprecise conversion");
            }
            return n;
        }


        /// <summary>
        /// Use exponential notation if sd is less than the number of digits necessary to represent<br />
        /// the integer part of the value in normal notation.
        /// </summary>
        /// <param name="sd">{number} Significant digits: integer, 1 to MAX_DP inclusive.</param>
        /// <param name="rm">{RoundingMode} Rounding mode.</param>
        /// <returns>A string representing the value of this Big rounded to sd significant digits using<br />
        /// rounding mode rm, or Big.Config.RM if rm is not specified.</returns>
        /// <exception cref="BigException"></exception>
        public string ToPrecision(int? sd, RoundingMode? rm = null)
        {
            var x = this;
            var n = x.c[0];

            if (sd.HasValue)
            {
                if (sd != ~~sd || sd < 1 || sd > x.Config.MAX_DP)
                {
                    throw new BigException(BigException.INVALID + "precision");
                }
                x = BigHelperFunctions.round(new Big(x, x.Config), sd.Value, rm);
                for (; x.c.LongLength < sd;) ArrayExtensions.Push(ref x.c, 0);
            }

            return BigHelperFunctions.stringify(x, sd <= x.e || x.e <= x.Config.NE || x.e >= x.Config.PE, n != 0);
        }


        /// <summary>
        /// Include the sign for negative zero.
        /// </summary>
        /// <returns>A string representing the value of this Big.<br />
        /// Exponential notation if this Big has a positive exponent equal to or greater than<br />
        /// Big.Config.PE, or a negative exponent equal to or less than Big.Config.NE.</returns>
        /// <exception cref="BigException"></exception>
        public string ValueOf()
        {
            var x = this;
            if (x.Config.STRICT == true)
            {
                throw new BigException(BigException.NAME + "valueOf disallowed");
            }
            return BigHelperFunctions.stringify(x, x.e <= x.Config.NE || x.e >= x.Config.PE, true);
        }
    }
}