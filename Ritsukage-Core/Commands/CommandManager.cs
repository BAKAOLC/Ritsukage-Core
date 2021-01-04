using Ritsukage.Tools;
using Sora.EventArgs.SoraEvent;
using Sora.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ritsukage.Commands
{

    public class CommandArgs
    {
        string rawInput;
        List<string> singleArg;
        private int index = 0;

        /// <summary>
        /// 将参数部分拆分成args部分
        /// 注意不支持转义
        /// 字符串引用需要为'...'或者"..." 且不能有任意'或者"干扰
        /// 
        /// </summary>
        /// <param name="rawInput">args部分【不带空格情况】</param>
        public CommandArgs(string rawInput)
        {
            this.rawInput = rawInput;
            this.singleArg = new();
            int len = rawInput.Length;
            int i = 0;
            var sb = new StringBuilder();
            while (i < len)
            {
                char c = rawInput[i];
                switch (c)
                {
                    case ' ':
                        {
                            this.singleArg.Add(sb.ToString());
                            sb = new();
                            break;
                        }
                    case '\'':
                        {
                            if (sb.Length == 0)
                            {
                                i += 1;
                                while (i < len && rawInput[i] != '\'')
                                {
                                    sb.Append(rawInput[i]);
                                    i += 1;
                                }
                            }
                            else sb.Append(c);

                            break;
                        }
                    case '"':
                        {
                            if (sb.Length == 0)
                            {
                                i += 1;
                                while (i < len && rawInput[i] != '"')
                                {
                                    sb.Append(rawInput[i]);
                                    i += 1;
                                }
                            }
                            else sb.Append(c);
                            break;
                        }
                    default:
                        {
                            sb.Append(c);
                            break;
                        }
                }
                i += 1;
            }
            if (sb.Length > 0)
            {
                singleArg.Add(sb.ToString());
            }
        }

        public string Next()
        {
            string s = this.singleArg[this.index];
            this.index += 1;
            return s;
        }
    }

    public interface ICommandParser
    {
        object Parse(CommandArgs args);
    }

    public delegate void ArgumentErrorCallback(BaseSoraEventArgs e, Exception ex = null);

    public class Command
    {
        string startHeader = "+";
        string name;
        internal MethodInfo method;
        internal Type[] argTypes;
        internal ArgumentErrorCallback failedCallback;

        internal Command(string s, string n, MethodInfo method, Type[] args, ArgumentErrorCallback cb = null)
        {
            this.startHeader = s;
            this.name = n;
            this.method = method;
            this.argTypes = args;
            this.failedCallback = cb;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandGroupAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        internal string startHeader = "+";
        internal string[] name;

        public string StartHeader { get { return startHeader; } set { startHeader = value; } }
        public string[] Name { get { return name; } set { name = value; } }

        public CommandAttribute(params string[] name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CommandArgumentErrorCallbackAttribute : Attribute
    {
        string callbackMethodName;
        public string ArgumentErrorCallbackMethodName { get => callbackMethodName; set { callbackMethodName = value; } }

        public CommandArgumentErrorCallbackAttribute(string methodName)
        {
            callbackMethodName = methodName;
        }
    }

    public static class CommandManager
    {
        public static readonly Dictionary<Type, ICommandParser> Parsers = new();
        public static readonly Dictionary<string, Dictionary<string, Command>> Commands = new();

        private static Dictionary<string, Command> getFromHeader(string header)
        {
            if (Commands.TryGetValue(header, out var value))
            {
                return value;
            }
            else
            {
                var d = new Dictionary<string, Command>();
                Commands.Add(header, d);
                return d;
            }
        }

        private static object ParseArgument(Type type, CommandArgs args)
        {
            Exception e = null;
            if (Parsers.TryGetValue(type, out ICommandParser parser))
            {
                try
                {
                    return parser.Parse(args);
                }
                catch (Exception ee)
                {
                    e = ee;
                }
            }
            else
            {
                if (type == typeof(byte))
                    return byte.Parse(args.Next());
                else if (type == typeof(short))
                    return short.Parse(args.Next());
                else if (type == typeof(ushort))
                    return ushort.Parse(args.Next());
                else if (type == typeof(int))
                    return int.Parse(args.Next());
                else if (type == typeof(uint))
                    return uint.Parse(args.Next());
                else if (type == typeof(long))
                    return long.Parse(args.Next());
                else if (type == typeof(ulong))
                    return ulong.Parse(args.Next());
                else if (type == typeof(float))
                    return float.Parse(args.Next());
                else if (type == typeof(double))
                    return double.Parse(args.Next());
                else if (type == typeof(bool))
                {
                    string original = args.Next();
                    string s = original.ToLower();
                    if (s == "真" || s == "true" || s == "t" || s == "1")
                    {
                        return true;
                    }
                    else if (s == "假" || s == "false" || s == "f" || s == "0")
                    {
                        return false;
                    }
                    else throw new ArgumentException($"{original} is not a bool value.");
                }
                else if (type == typeof(DateTime))
                {
                    return DateTimeReader.Parse(args.Next());
                }
                else if (type == typeof(TimeSpan))
                {
                    return TimeSpanReader.Parse(args.Next());
                }
                else if (type == typeof(string))
                    return args.Next();
            }
            throw new ArgumentException($"the type of {type} cannot be parsed from string", e);
        }

        public static void RegisterAllCommands(Type type)
        {
            foreach (var method in type.GetMethods())
            {
                var attrs = method.GetCustomAttribute<CommandAttribute>();

                var fcbn = method.GetCustomAttribute<CommandArgumentErrorCallbackAttribute>();

                ArgumentErrorCallback fcb = null;

                if (fcbn != null)
                    fcb = type.GetMethods().Where(x => x.Name == fcbn.ArgumentErrorCallbackMethodName)?
                        .FirstOrDefault()?.CreateDelegate<ArgumentErrorCallback>();

                if (attrs != null)
                {
                    var ps = method.GetParameters();
                    var ts = new Type[ps.Length];
                    for (int i = 0; i < ps.Length; ++i)
                    {
                        ts[i] = ps[i].ParameterType;
                    }

                    if (attrs.name.Length == 0)
                        attrs.name = new[] { method.Name };

                    var command = new Command(attrs.startHeader, attrs.name[0], method, ts, fcb);

                    var d = getFromHeader(attrs.startHeader);
                    foreach (var n in attrs.name)
                    {
                        d.Add(n.ToLower(), command);
                    }
                    ConsoleLog.Debug("Commands", $"Register command: {attrs.name} {command.method}");
                }
            }
        }

        public static void RegisterAllCommands()
        {
            ConsoleLog.Debug("Commands", "Start loading...");
            Type[] types = Assembly.GetEntryAssembly().GetExportedTypes();
            Type[] cosType = types.Where(t => Attribute.GetCustomAttributes(t, true).Where(a => a is CommandGroupAttribute).Any()).ToArray();
            foreach (var group in cosType)
            {
                ConsoleLog.Debug("Commands", $"Register commands group: {group.FullName}");
                RegisterAllCommands(group);
            }
            ConsoleLog.Debug("Commands", "Finish.");
        }

        public static void ReceiveMessage(BaseSoraEventArgs arg)
        {
            string msg = string.Empty;
            if (arg is GroupMessageEventArgs a1)
                msg = a1.Message.RawText;
            else if (arg is PrivateMessageEventArgs a2)
                msg = a2.Message.RawText;
            if (!string.IsNullOrEmpty(msg))
            {
                ConsoleLog.Debug("Commands", "Parser: " + msg);
                foreach (var node in Commands)
                {
                    if (msg.StartsWith(node.Key))
                    {
                        var caa = msg[node.Key.Length..].Split(" ", 2);
                        if (node.Value.TryGetValue(caa[0].ToLower(), out var command))
                        {
                            var args = new CommandArgs(caa.Length == 2 ? caa[1] : "");
                            object[] ps = new object[command.argTypes.Length];
                            ps[0] = arg;

                            try
                            {
                                for (int i = 1; i < ps.Length; ++i)
                                {
                                    ps[i] = ParseArgument(command.argTypes[i], args);
                                }
                            }
                            catch (Exception e)
                            {
                                ConsoleLog.ErrorLogBuilder(e);
                                ConsoleLog.Debug("Commands", $"Failed to parse {command.method} arguments.");
                                if (command.failedCallback != null)
                                {
                                    try
                                    {
                                        ConsoleLog.Debug("Commands", $"Try to execute {command.method} failed callback.");
                                        command.failedCallback.Invoke(arg, e);
                                    }
                                    catch (Exception ex)
                                    {
                                        ConsoleLog.ErrorLogBuilder(ex);
                                        ConsoleLog.Debug("Commands", $"Failed to execute {command.method} failed callback.");
                                    }
                                }
                                return;
                            }

                            ConsoleLog.Debug("Commands", $"Invoke {command.method}.");
                            command.method.Invoke(null, ps);
                            return;
                        }
                    }
                }
            }
        }

        static CommandManager()
        {
            Parsers = new();
        }
    }
}
