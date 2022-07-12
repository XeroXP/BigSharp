using BigSharp.Tests.Extensions;
using NUnit.Framework;

namespace BigSharp.Tests
{
    [TestFixture, Category("BigNeg")]
    public class BigNegTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Neg()
        {
            var bigFactory = new BigFactory(new BigConfig());

            var t = (object expected0, BigArgument value) =>
            {
                string expected = expected0.ToExpectedString(bigFactory.Config.PE, bigFactory.Config.NE);

                BigTests.AreEqual(expected.ToString(), bigFactory.Big(value).Neg().ToString());
            };

            bigFactory.Config.NE = -7;
            bigFactory.Config.PE = 21;

            BigTests.IsNegativeZero(bigFactory.Big("0").Neg());
            BigTests.IsNegativeZero(bigFactory.Big("-0").Neg().Neg());

            t("0", "0");
            t("-1", "1");
            t("-11.121", "11.121");
            t("0.023842", "-0.023842");
            t("1.19", "-1.19");
            t("-3838.2", "3838.2");
            t("-127", "127");
            t("4.23073", "-4.23073");
            t("2.5469", "-2.5469");
            t("-2.0685908346593874980567875e+25", "20685908346593874980567875");

            Assert.Pass();
        }
    }
}
