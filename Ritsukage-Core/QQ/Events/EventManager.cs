using Ritsukage.Tools.Console;
using Sora.EventArgs.SoraEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.QQ.Events
{
    public static class EventManager
    {
        static bool _init = false;
        public static void Init()
        {
            if (_init) return;
            _init = true;
            RegisterAllEvents();
        }

        private static readonly Dictionary<Type, List<MethodInfo>> Events = new();

        public static void Trigger(object sender, BaseSoraEventArgs args)
        {
            Type type = args.GetType();
            if (Events.TryGetValue(type, out var list))
            {
                foreach (var e in list)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            e.Invoke(null, new object[] { sender, args });
                        }
                        catch (Exception ex)
                        {
                            ConsoleLog.Error("Event Manager", new StringBuilder()
                                .AppendLine("触发Event时发生错误")
                                .AppendLine($"Event Type\t: {type}")
                                .AppendLine($"Method\t\t: {e}")
                                .Append($"Exception\t: {ex.GetFormatString(true)}"));
                        }
                    });
                }
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
