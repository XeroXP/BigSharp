namespace BigSharp
{
    public class BigConfig
    {
        // 0 to MAX_DP
        private int _dp = 20;
        /// <summary>
        /// The maximum number of decimal places (DP) of the results of operations involving division:<br />
        /// div and sqrt, and pow with negative exponents.
        /// </summary>
        public int DP
        {
            get { return _dp; }
            set {
                if (value < 1 || value > MAX_DP)
                    throw new BigException(BigException.INVALID + nameof(DP) + ": " + value);
                _dp = value;
            }
        }

        /// <summary>
        /// The rounding mode (RM) used when rounding to the above decimal places.
        /// </summary>
        public RoundingMode RM { get; set; } = RoundingMode.ROUND_HALF_UP;

        // 0 to 1000000
        private int _max_dp = (int)1E6;
        /// <summary>
        /// The maximum value of DP and Big.DP.
        /// </summary>
        public int MAX_DP
        {
            get { return _max_dp; }
            set {
                if (value < 0 || value > (int)1E6)
                    throw new BigException(BigException.INVALID + nameof(MAX_DP) + ": " + value);
                _max_dp = value;
            }
        }

        // 1 to 1000000
        private int _max_power = (int)1E6;
        /// <summary>
        /// The maximum magnitude of the exponent argument to the pow method.
        /// </summary>
        public int MAX_POWER
        {
            get { return _max_power; }
            set {
                if (value < 0 || value > (int)1E6)
                    throw new BigException(BigException.INVALID + nameof(MAX_POWER) + ": " + value);
                _max_power = value;
            }
        }

        // 0 to -1000000
        private int _ne = -7;
        /// <summary>
        /// The negative exponent (NE) at and beneath which toString returns exponential notation.
        /// <br>(JavaScript numbers: -7)</br>
        /// <br>-1000000 is the minimum recommended exponent value of a Big.</br>
        /// </summary>
        public int NE
        {
            get { return _ne; }
            set
            {
                if (value < 0 || value > (int)-1E6)
                    throw new BigException(BigException.INVALID + nameof(NE) + ": " + value);
                _ne = value;
            }
        }

        // 0 to 1000000
        private int _pe = 21;
        /// <summary>
        /// The positive exponent (PE) at and above which toString returns exponential notation.
        /// <br>(JavaScript numbers: 21)</br>
        /// <br>1000000 is the maximum recommended exponent value of a Big, but this limit is not enforced.</br>
        /// </summary>
        public int PE
        {
            get { return _pe; }
            set
            {
                if (value < 0 || value > (int)1E6)
                    throw new BigException(BigException.INVALID + nameof(PE) + ": " + value);
                _pe = value;
            }
        }

        /// <summary>
        /// When true, an error will be thrown if a primitive number is passed to the Big constructor,
        /// <br>or if valueOf is called, or if toNumber is called on a Big which cannot be converted to a</br>
        /// <br>primitive number without a loss of precision.</br>
        /// </summary>
        public bool STRICT { get; set; } = false;     // true or false

        public BigConfig Clone()
        {
            return new BigConfig
            {
                MAX_DP = MAX_DP,
                DP = DP,
                RM = RM,
                MAX_POWER = MAX_POWER,
                NE = NE,
                PE = PE,
                STRICT = STRICT
            };
        }
    }
}
