using NUnit.Framework;

namespace BigSharp.Tests
{
    [TestFixture, Category("BigToNumber")]
    public class BigToNumberTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ToNumber()
        {
            var bigFactory = new BigFactory(new BigConfig()
            {
                NE = -7,
                PE = 21,
                STRICT = false
            });

            var t = (BigArgument value) =>
            {
                BigTests.IsTrue(1 / bigFactory.Big(value).ToNumber() == double.PositiveInfinity);
            };

            t(0);
            t("0");
            t("0.0");
            t("0.000000000000");
            t("0e+0");
            t("0e-0");

            // Negative zero
            t = (BigArgument value) =>
            {
                BigTests.IsTrue(1 / bigFactory.Big(value).ToNumber() == double.NegativeInfinity);
            };

            t("-0");
            t("-0.0");
            t("-0.000000000000");
            t("-0e+0");
            t("-0e-0");

            var t2 = (BigArgument value, double expected0) =>
            {
                BigTests.AreEqual(bigFactory.Big(value).ToNumber(), expected0);
            };

            t2(0, 0);
            t2(-0, -0);
            t2("0", 0);
            t2("-0", -0);

            t2(1, 1);
            t2("1", 1);
            t2("1.0", 1);
            t2("1e+0", 1);
            t2("1e-0", 1);

            t2(-1, -1);
            t2("-1", -1);
            t2("-1.0", -1);
            t2("-1e+0", -1);
            t2("-1e-0", -1);


            t2("123.456789876543", 123.456789876543);
            t2("-123.456789876543", -123.456789876543);

            t2("1.1102230246251565e-16", 1.1102230246251565e-16);
            t2("-1.1102230246251565e-16", -1.1102230246251565e-16);

            t2("9007199254740991", 9007199254740991);
            t2("-9007199254740991", -9007199254740991);

            t2("5e-324", 5e-324);
            t2("1.7976931348623157e+308", 1.7976931348623157e+308);

            t2("0.00999", 0.00999);
            t2("123.456789", 123.456789);
            t2("1.23456789876543", 1.23456789876543);

            t2(long.MaxValue, long.MaxValue);

            var n = "1.000000000000000000001";

            t2(n, 1);

            bigFactory.Config.STRICT = true;

            BigTests.IsException(() => { bigFactory.Big(n).ToNumber(); }, "bigFactory.Big(n).toNumber()");

            BigTests.IsException(() => { bigFactory.Big(0).ToNumber(); }, "bigFactory.Big(0).toNumber()");
            BigTests.IsException(() => { bigFactory.Big(1).ToNumber(); }, "bigFactory.Big(1).toNumber()");
            BigTests.IsException(() => { bigFactory.Big(-1).ToNumber(); }, "bigFactory.Big(-1).toNumber()");

            t2("0", 0);
            t2("-0", -0);
            t2("1", 1);
            t2("1.0", 1);
            t2("1e+0", 1);
            t2("1e-0", 1);
            t2("-1", -1);
            t2("-1.0", -1);
            t2("-1e+0", -1);
            t2("-1e-0", -1);

            t2("123.456789876543", 123.456789876543);
            t2("-123.456789876543", -123.456789876543);

            t2("1.1102230246251565e-16", 1.1102230246251565e-16);
            t2("-1.1102230246251565e-16", -1.1102230246251565e-16);

            t2("9007199254740991", 9007199254740991);
            t2("-9007199254740991", -9007199254740991);

            t2("5e-324", 5e-324);
            t2("1.7976931348623157e+308", 1.7976931348623157e+308);

            t2("0.00999", 0.00999);
            t2("123.456789", 123.456789);
            t2("1.23456789876543", 1.23456789876543);

            bigFactory.Config.STRICT = false;

            Assert.Pass();
        }
    }
}
