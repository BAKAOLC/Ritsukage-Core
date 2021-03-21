using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using static Ritsukage.Tools.CalcTool;

namespace ToolUnitTestProject
{
    [TestClass]
    public class CalcExprTest
    {
        [TestMethod]
        public void TestBaseCalc() {
            Assert.AreEqual(3, GetExprValue("1+2"));
            Assert.AreEqual(3, GetExprValue("4-1"));
            Assert.AreEqual(3, GetExprValue("1.5*2"));
            Assert.AreEqual(3, GetExprValue("9/3"));

            Assert.AreEqual(3, GetExprValue("10%7"));
            Assert.AreEqual(3, GetExprValue("sqrt(9)"));
            Assert.AreEqual(3, GetExprValue("sqrt(3)^2"), 1e-6, "√3平方应该为3");

            Assert.AreEqual(31, GetExprValue("2*3*5+1"));
            Assert.AreEqual(-6, GetExprValue("-1-2-3"));
            Assert.AreEqual(-9.6, GetExprValue("1+2-3*4-0.6"));
            Assert.AreEqual(3, GetExprValue("(1+2)-(7-7)"));

            Assert.AreEqual(6561, GetExprValue("3^2^3"));

            Assert.AreEqual(4, GetExprValue("2sqrt(4)"));
            Assert.AreEqual(16, GetExprValue("2sqrt(4)*2sqrt(4)"));
            Assert.AreEqual(0.2, GetExprValue("1e-1sqrt(4)"));
        }

        [TestMethod]
        public void TestConst() {
            Assert.AreEqual(Math.E, GetExprValue("e"));
            Assert.AreEqual(-Math.E, GetExprValue("-e"));
            Assert.AreEqual(Math.PI, GetExprValue("pi"));
            Assert.AreEqual(-Math.PI, GetExprValue("-pi"));
        }

        [TestMethod]
        public void TestExtraCalc() {
            Assert.AreEqual(2, GetExprValue("1--1"));
            Assert.AreEqual(2, GetExprValue("-(-(-1+3))"));

            Assert.AreEqual(0, GetExprValue("-abs(lg(1))"));
        }
    }
}
