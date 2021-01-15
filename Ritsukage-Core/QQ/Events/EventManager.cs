using Sora.EventArgs.SoraEvent;
using Sora.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Ritsukage.QQ.Events
{
    public static class EventManager
    {
        static EventManager()
        {
            RegisterAllEvents();
        }

        private static readonly Dictionary<Type, List<MethodInfo>> Events = new();

        public static void Trigger(object sender, BaseSoraEventArgs args)
        {
            Type type = args.GetType();
            if (Events.TryGetValue(type, out var list))
            {
                new Thread(() =>
                {
                    foreach (var e in list)
                        e.Invoke(null, new object[] { sender, args });
                })
                {
                    IsBackground = true
                }.Start();
            }
        }

        public static void RegisterAllEvents(Type type)
        {
            foreach (var method in type.GetMethods())
            {
                var attrs = method.GetCustomAttribute<EventAttribute>();
                if (attrs != null)
                {
                    if (Events.TryGetValue(attrs.Handled, out var list))
                        list.Add(method);
                    else
                        Events.Add(attrs.Handled, new()
                        {
                            { method }
                        });
                    ConsoleLog.Debug("Events", $"Register event: {attrs.Handled} for {method}");
                }
            }
        }

        public static void RegisterAllEvents()
        {
            ConsoleLog.Debug("Events", "Start loading...");
            Type[] types = Assembly.GetEntryAssembly().GetExportedTypes();
            Type[] cosType = types.Where(t => Attribute.GetCustomAttributes(t, true).Where(a => a is EventGroupAttribute).Any()).ToArray();
            foreach (var group in cosType)
            {
                ConsoleLog.Debug("Events", $"Register events group: {group.FullName}");
                RegisterAllEvents(group);
            }
            ConsoleLog.Debug("Events", "Finish.");
        }
    }
}
