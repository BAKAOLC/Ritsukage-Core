using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Ritsukage.Library.FFXIV;
using System;
using System.Diagnostics;
using static Ritsukage.Library.FFXIV.StatusCalculator;

namespace ToolUnitTestProject
{
    [TestClass]
    public class CalcExprTest
    {
        [TestMethod]
        public void Test() {
            Console.WriteLine(Speed(1000).ToString());
        }
    }
}
