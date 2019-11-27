using FluentAssertions;
using NUnit.Framework;
using WSharp.Utilities.Math;

namespace WSharp.Utilities.Tests.Tests.Math
{
    [TestFixture]
    public class PythagorasTests
    {
        [Test]
        public void SolveHypotenus()
        {
            var a = 3;
            var b = 4;
            Pythagoras.Solve(a, b)
                .Should()
                .Be(5, "3^2 + 4^2 = 5^2");

            a = 8;
            b = 6;
            Pythagoras.Solve(a, b)
                .Should()
                .Be(10, "8^2 + 6^2 = 10^2");
        }

        [Test]
        public void SolveA()
        {
            double? a = null;
            double? b = 6;
            double? c = 10;

            Pythagoras.Solve(ref a, ref b, ref c)
                .Should()
                .Be(8, "8^2 + 6^2 = 10^2");

            a.Should().Be(8, "8^2 + 6^2 = 10^2");
        }

        [Test]
        public void SolveB()
        {
            double? a = 3;
            double? b = null;
            double? c = 5;

            Pythagoras.Solve(ref a, ref b, ref c)
                .Should()
                .Be(4, "3^2 + 4^2 = 5^2");

            b.Should().Be(4, "3^2 + 4^2 = 5^2");
        }

        [Test]
        public void SolveC()
        {
            double? a = 3;
            double? b = 4;
            double? c = null;

            Pythagoras.Solve(ref a, ref b, ref c)
                .Should()
                .Be(5, "3^2 + 4^2 = 5^2");

            c.Should().Be(5, "3^2 + 4^2 = 5^5");
        }
    }
}
