using NUnit.Framework;
using System;
using System.Text.RegularExpressions;

namespace BigSharp.Tests
{
    [TestFixture, Category("Big")]
    public class BigTests
    {
        [SetUp]
        public void Setup()
        {
        }

        internal static long count = 0, passed = 0;

        /*internal static void Write(string str)
        {
            Console.WriteLine(str);
        }*/

        internal static void Fail(long count, object expected, object actual)
        {
            //Write("\n  Test number " + count + " failed" + "\n  Expected: " + expected.ToString() + "\n  Actual:   " + actual.ToString());
            Assert.Fail("Test number " + count + " failed" + "\nExpected: " + expected.ToString() + "\nActual:   " + actual.ToString());
        }

        internal static bool IsBigZero(Big n)
        {
            return n != null && n.c != null && n.c.Length == 1 && n.c[0] == 0 && n.e == 0 && (n.s == 1 || n.s == -1);
        }

        internal static void AreEqual(string? expected, string? actual)
        {
            ++count;

            // If expected and actual are both NaN, consider them equal.
            if (expected == actual || expected != expected && actual != actual)
            {
                ++passed;
                //Write("\n Expected and actual: " + actual);
            }
            else
            {
                Fail(count, expected, actual);
            }
        }
        internal static void AreEqual(double expected, double actual)
        {
            ++count;

            // If expected and actual are both NaN, consider them equal.
            if (expected == actual || expected != expected && actual != actual)
            {
                ++passed;
                //Write("\n Expected and actual: " + actual);
            }
            else
            {
                Fail(count, expected, actual);
            }
        }

        internal static void IsException(Action func, string msg)
        {
            BigException? actual = null;
            ++count;
            try
            {
                func();
            }
            catch (BigException e)
            {
                actual = e;
            }
            if (actual != null && Regex.IsMatch(actual.Message, @"\[BigSharp\]"))
            {
                ++passed;
            }
            else
            {
                Fail(count, (msg + " to raise an fail."), (actual != null ? actual : "no exception"));
            }
        }

        internal static void IsNegativeZero(Big actual)
        {
            ++count;
            if (IsBigZero(actual) && actual.s == -1)
            {
                ++passed;
            }
            else
            {
                Fail(count, "negative zero", actual);
            }
        }

        internal static void IsPositiveZero(Big actual)
        {
            ++count;
            if (IsBigZero(actual) && actual.s == 1)
            {
                ++passed;
            }
            else
            {
                Fail(count, "positive zero", actual);
            }
        }

        internal static void IsTrue(bool actual)
        {
            ++count;
            if (actual == true)
            {
                ++passed;
            }
            else
            {
                Fail(count, "true", actual);
            }
        }
    }
}