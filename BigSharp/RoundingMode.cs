namespace BigSharp
{
    public enum RoundingMode
    {
        /// <summary>
        /// Towards zero (i.e. truncate, no rounding).
        /// </summary>
        ROUND_DOWN, //0
        /// <summary>
        /// To nearest neighbour. If equidistant, round up.
        /// </summary>
        ROUND_HALF_UP, //1
        /// <summary>
        /// To nearest neighbour. If equidistant, to even.
        /// </summary>
        ROUND_HALF_EVEN, //2
        /// <summary>
        /// Away from zero.
        /// </summary>
        ROUND_UP //3
    }
}
