namespace BigSharp
{
    public class BigException : Exception
    {
        // Error messages.
        internal static readonly string NAME = "[BigSharp] ",
        INVALID = NAME + "Invalid ",
        INVALID_DP = INVALID + "decimal places",
        DIV_BY_ZERO = NAME + "Division by zero";

        public BigException() : base()
        {

        }

        public BigException(string message) : base(message)
        {

        }
    }
}
