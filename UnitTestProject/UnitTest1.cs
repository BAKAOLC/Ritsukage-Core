using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ritsukage.Commands;

namespace UnitTestProject
{

    static class CommandClass
    {
        [CommandInfo(Name = "AZWKAWAII", Alias = new string[]{"azw"} )]
        public static int AzureZengIsSOCuteAndKawaiiiiiiiiiiii(string arg1)
        {
            Assert.AreEqual("ljyys is not kawaii", arg1);
            return 0;
        }

    }

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            CommandManager.RegisterAllCommands(typeof(CommandClass));
            var a = CommandManager.ReceiveMessage("/azw 'ljyys is not kawaii'");
            Assert.AreEqual(0, a);
        }
    }
}
