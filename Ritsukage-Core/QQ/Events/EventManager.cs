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
        struct EventMethod
        {
            public MethodInfo Method { get; init; }

            public bool HandleOthers { get; init; }

            public bool HandleSelf { get; init; }

            public EventMethod(MethodInfo method, bool handleSelf = false, bool handleOthers = true)
            {
                Method = method;
                HandleSelf = handleSelf;
                HandleOthers = handleOthers;
            }
        }

        static bool _init = false;
        public static void Init()
        {
            if (_init) return;
            _init = true;
            RegisterAllEvents();
        }

        private static readonly Dictionary<Type, List<EventMethod>> Events = new();

        public static void Trigger(object sender, BaseSoraEventArgs args, bool fromSelf = false)
        {
            Type type = args.GetType();
            if (Events.TryGetValue(type, out var list))
            {
                foreach (var e in list.Where(x => fromSelf ? x.HandleSelf : x.HandleOthers).ToArray())
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            e.Method?.Invoke(null, new object[] { sender, args });
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
                        list.Add(new(method, attrs.HandleSelf, attrs.HandleOthers));
                    else
                        Events.Add(attrs.Handled, new()
                        {
                            { new(method, attrs.HandleSelf, attrs.HandleOthers) }
                        });
                    ConsoleLog.Debug("Events", new StringBuilder().AppendLine("Register Event")
                        .AppendLine($"Event\t\t: {attrs.Handled}")
                        .AppendLine($"Method\t\t: {method}")
                        .AppendLine($"Handle Others\t: {attrs.HandleOthers}")
                        .Append($"Handle Self\t: {attrs.HandleSelf}"));
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
