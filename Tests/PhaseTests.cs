using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Z_nthCommon;

namespace Tests
{
    [TestClass]
    public class PhaseTests
    {
        [TestMethod]
        public void TriangleTest()
        {
            double[] values = new double[20];

            int i = 0;

            for (double phase = 0; phase < Math.PI * 2; phase += Math.PI / 10)
            {
                values[i] = Math.Round(Phase.Triangle(phase), 2);
                i++;
            }

            Assert.AreEqual(values[0], 0);
            Assert.AreEqual(values[1], 0.2);
            Assert.AreEqual(values[2], 0.4);
            Assert.AreEqual(values[3], 0.6);
            Assert.AreEqual(values[4], 0.8);
            Assert.AreEqual(values[5], 1);
            Assert.AreEqual(values[6], 0.8);
            Assert.AreEqual(values[7], 0.6);
            Assert.AreEqual(values[8], 0.4);
            Assert.AreEqual(values[9], 0.2);
            Assert.AreEqual(values[10], 0);
            Assert.AreEqual(values[11], -0.2);
            Assert.AreEqual(values[12], -0.4);
            Assert.AreEqual(values[13], -0.6);
            Assert.AreEqual(values[14], -0.8);
            Assert.AreEqual(values[15], -1);
            Assert.AreEqual(values[16], -0.8);
            Assert.AreEqual(values[17], -0.6);
            Assert.AreEqual(values[18], -0.4);
            Assert.AreEqual(values[19], -0.2);

        }

        [TestMethod]
        public void SawTest()
        {
            double[] values = new double[10];

            int i = 0;

            for (double phase = 0; phase < Math.PI * 2; phase += Math.PI / 5)
            {
                values[i] = Math.Round(Phase.Saw(phase), 2);
                i++;
            }

            Assert.AreEqual(values[0], 0);
            Assert.AreEqual(values[1], 0.2);
            Assert.AreEqual(values[2], 0.4);
            Assert.AreEqual(values[3], 0.6);
            Assert.AreEqual(values[4], 0.8);
            Assert.AreEqual(values[5], 1);
            Assert.AreEqual(values[6], -0.8);
            Assert.AreEqual(values[7], -0.6);
            Assert.AreEqual(values[8], -0.4);
            Assert.AreEqual(values[9], -0.2);

        }

        [TestMethod]
        public void SquareTest()
        {
            double[] values = new double[10];

            int i = 0;

            for (double phase = 0; phase < Math.PI * 2; phase += Math.PI / 5)
            {
                values[i] = Math.Round(Phase.Square(phase), 2);
                i++;
            }

            Assert.AreEqual(values[0], 1);
            Assert.AreEqual(values[1], 1);
            Assert.AreEqual(values[2], 1);
            Assert.AreEqual(values[3], 1);
            Assert.AreEqual(values[4], 1);
            Assert.AreEqual(values[5], -1);
            Assert.AreEqual(values[6], -1);
            Assert.AreEqual(values[7], -1);
            Assert.AreEqual(values[8], -1);
            Assert.AreEqual(values[9], -1);

        }
    }
}
