using BigSharp.Tests.Extensions;
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
            Big.NE = -7;
            Big.PE = 21;
            Big.STRICT = false;

            var t = (object value0) =>
            {
                Big? value = value0.ToBig();

                if (value == null)
                    Assert.Fail();

                BigTests.IsTrue(1 / new Big(value).ToNumber() == double.PositiveInfinity);
            };

            t(0);
            t("0");
            t("0.0");
            t("0.000000000000");
            t("0e+0");
            t("0e-0");

            // Negative zero
            t = (object value0) =>
            {
                Big? value = value0.ToBig();

                if (value == null)
                    Assert.Fail();

                BigTests.IsTrue(1 / new Big(value).ToNumber() == double.NegativeInfinity);
            };

            t("-0");
            t("-0.0");
            t("-0.000000000000");
            t("-0e+0");
            t("-0e-0");

            var t2 = (object value0, double expected0) =>
            {
                Big? value = value0.ToBig();

                if (value == null)
                    Assert.Fail();

                BigTests.AreEqual(new Big(value).ToNumber(), expected0);
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

            Big.STRICT = true;

            BigTests.IsException(() => { new Big(n).ToNumber(); }, "new Big(n).toNumber()");

            BigTests.IsException(() => { new Big(0).ToNumber(); }, "new Big(0).toNumber()");
            BigTests.IsException(() => { new Big(1).ToNumber(); }, "new Big(1).toNumber()");
            BigTests.IsException(() => { new Big(-1).ToNumber(); }, "new Big(-1).toNumber()");

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

            Big.STRICT = false;

            Assert.Pass();
        }
    }
}
