namespace BigSharp
{
    public class BigFactory
    {
        public BigConfig Config { get; set; }

        public BigFactory(BigConfig? config = null)
        {
            this.Config = config ?? new BigConfig();
        }

        public Big Big(BigArgument v)
        {
            return new Big(v, this.Config);
        }

        // BigFactory methods


        /*
         *  clone
         */


        /// <summary>
        /// 
        /// </summary>
        /// <returns>A new BigDecimalLightFactory with the same configuration properties as this BigDecimalLightFactory.</returns>
        public BigFactory Clone()
        {
            return new BigFactory(this.Config.Clone());
        }
    }
}
