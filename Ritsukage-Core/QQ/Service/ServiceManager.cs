using Ritsukage.Tools.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.QQ.Service
{
    public static class ServiceManager
    {
        static bool _init = false;
        public static void Init()
        {
            if (_init) return;
            _init = true;
            RegisterAllServices();
        }

        public static void RegisterAllServices()
        {
            ConsoleLog.Debug("QQ Service", "Start loading...");
            Type[] types = Assembly.GetEntryAssembly().GetExportedTypes();
            Type[] cosType = types.Where(t => Attribute.GetCustomAttributes(t, true).Where(a => a is ServiceAttribute).Any()).ToArray();
            foreach (var group in cosType)
            {
                ConsoleLog.Debug("QQ Service", $"Register service group: {group.FullName}");
                var method = group.GetMethod("Init");
                method?.Invoke(null, null);
            }
            ConsoleLog.Debug("QQ Service", "Finish.");
        }
    }
}
