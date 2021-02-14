using System;
using System.Reflection;

namespace CommandDocumentGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Type[] types = Assembly.GetAssembly(typeof(Ritsukage.QQ.Commands.Command)).GetExportedTypes();
        }
    }
}
