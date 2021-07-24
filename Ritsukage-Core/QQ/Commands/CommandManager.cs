using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using Sora.EventArgs.SoraEvent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.QQ.Commands
{
    public class CommandArgs
    {
        readonly List<string> SingleArg;
        private int index = 0;

        public int Length => SingleArg.Count;

        /// <summary>
        /// 将参数部分拆分成args部分
        /// 注意不支持转义
        /// 字符串引用需要为'...'或者"..." 且不能有任意'或者"干扰
        /// 
        /// </summary>
        /// <param name="rawInput">args部分【不带空格情况】</param>
        public CommandArgs(string rawInput)
        {
            this.SingleArg = new();
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
                            this.SingleArg.Add(sb.ToString());
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
                SingleArg.Add(sb.ToString());
        }

        public void Reset() => index = 0;

        public string Next() => SingleArg[index++];

        public bool HasNext() => index < SingleArg.Count;
    }

    public interface ICommandParser
    {
        object Parse(CommandArgs args);
    }

    public delegate void ArgumentErrorCallback(BaseSoraEventArgs e, Exception ex = null);

    public class Command
    {
        public string StartHeader { get; init; } = "+";
        public string Name { get; init; }
        public PreconditionAttribute[] Preconditions { get; init; }
        internal MethodInfo Method;
        internal ParameterInfo[] ArgTypes;

        internal Command(string s, string n, MethodInfo method, ParameterInfo[] args, PreconditionAttribute[] preconditions)
        {
            StartHeader = s;
            Name = n;
            Method = method;
            ArgTypes = args;
            Preconditions = preconditions;
        }

        public async Task<bool> CheckPermission(BaseSoraEventArgs args)
        {
            ConsoleLog.Debug("Commands", $">> Start the precondition check for {Method}");
            foreach (var p in Preconditions)
            {
                if (!await p.CheckPermissionsAsync(args))
                {
                    ConsoleLog.Debug("Commands", $"  >> Precondition: [{p}] × Failed.");
                    ConsoleLog.Debug("Commands", $">> Precondition check failed.");
                    return false;
                }
                else
                    ConsoleLog.Debug("Commands", $"  >> Precondition: [{p}] √ Passed.");
            }
            ConsoleLog.Debug("Commands", $">> Precondition check passed.");
            return true;
        }
    }

    public static class CommandManager
    {
        static bool _init = false;
        public static void Init()
        {
            if (_init) return;
            _init = true;
            RegisterAllCommands();
        }

        public static readonly Dictionary<Type, ICommandParser> Parsers = new();
        public static readonly Dictionary<string, Dictionary<string, List<Command>>> Commands = new();

        private static Dictionary<string, List<Command>> GetFromHeader(string header)
        {
            if (Commands.TryGetValue(header, out var value))
                return value;
            else
            {
                var d = new Dictionary<string, List<Command>>();
                Commands.Add(header, d);
                return d;
            }
        }

        private static List<Command> GetCommandList(string header, string command)
        {
            var head = GetFromHeader(header);
            if (head.TryGetValue(command, out var value))
                return value;
            else
            {
                var d = new List<Command>();
                head.Add(command, d);
                return d;
            }
        }

        private static object ParseArgument(ParameterInfo param, CommandArgs args, bool isArray = false)
        {
            if (!args.HasNext())
            {
                if (param.HasDefaultValue)
                    return param.DefaultValue;
                else
                    throw new IndexOutOfRangeException();
            }
            Exception e = null;
            Type t = isArray ? param.ParameterType.GetElementType() : param.ParameterType;
            if (Parsers.TryGetValue(t, out ICommandParser parser))
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
                if (t == typeof(byte))
                    return byte.Parse(args.Next());
                else if (t == typeof(short))
                    return short.Parse(args.Next());
                else if (t == typeof(ushort))
                    return ushort.Parse(args.Next());
                else if (t == typeof(int))
                    return int.Parse(args.Next());
                else if (t == typeof(uint))
                    return uint.Parse(args.Next());
                else if (t == typeof(long))
                    return long.Parse(args.Next());
                else if (t == typeof(ulong))
                    return ulong.Parse(args.Next());
                else if (t == typeof(float))
                    return float.Parse(args.Next());
                else if (t == typeof(double))
                    return double.Parse(args.Next());
                else if (t == typeof(bool))
                {
                    string original = args.Next();
                    string s = original.ToLower();
                    if (s == "真" || s == "true" || s == "t" || s == "1")
                        return true;
                    else if (s == "假" || s == "false" || s == "f" || s == "0")
                        return false;
                    else throw new ArgumentException($"{original} is not a bool value.");
                }
                else if (t == typeof(DateTime))
                    return DateTimeReader.Parse(args.Next());
                else if (t == typeof(TimeSpan))
                    return TimeSpanReader.Parse(args.Next());
                else if (t == typeof(string))
                    return args.Next();
            }
            throw new ArgumentException($"the type of {param} cannot be parsed from string", e);
        }

        public static void RegisterCommand(MethodInfo method, params PreconditionAttribute[] preconditions)
        {
            var attrs = method.GetCustomAttribute<CommandAttribute>();
            if (attrs != null)
            {
                var list = new List<PreconditionAttribute>();
                foreach (var a in preconditions)
                    list.Add(a);
                var p = method.GetCustomAttributes()?.Where(x => x is PreconditionAttribute)?.ToList();
                if (p != null)
                    foreach (PreconditionAttribute a in p)
                        list.Add(a);
                var name = attrs.Name;
                if (name.Length == 0)
                    name = new[] { method.Name };
                var command = new Command(attrs.StartHeader, name[0], method, method.GetParameters(), list.ToArray());
                foreach (var n in name)
                {
                    ConsoleLog.Debug("Commands", $"Register command: {n} for {command.Method}");
                    GetCommandList(attrs.StartHeader, n.ToLower()).Add(command);
                }
            }
        }

        public static void RegisterAllCommands(Type type, params PreconditionAttribute[] preconditions)
        {
            foreach (var method in type.GetMethods())
                RegisterCommand(method, preconditions);
        }

        public static void RegisterAllCommands()
        {
            ConsoleLog.Debug("Commands", "Start loading...");
            Type[] types = Assembly.GetEntryAssembly().GetExportedTypes();
            Type[] cosType = types.Where(t => Attribute.GetCustomAttributes(t, true).Where(a => a is CommandGroupAttribute).Any()).ToArray();
            foreach (var group in cosType)
            {
                ConsoleLog.Debug("Commands", $"Register commands group: {group.FullName}");
                var list = new List<PreconditionAttribute>();
                var p = group.GetCustomAttributes()?.Where(x => x is PreconditionAttribute)?.ToList();
                if (p != null)
                    foreach (PreconditionAttribute a in p)
                        list.Add(a);
                RegisterAllCommands(group, list.ToArray());
            }
            ConsoleLog.Debug("Commands", "Finish.");
        }

        static bool IsParams(ParameterInfo info) => info.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;

        public static async void ReceiveMessage(BaseSoraEventArgs arg)
        {
            string msg = string.Empty;
            SoraMessage m = null;
            if (arg is GroupMessageEventArgs a1)
            {
                msg = a1.Message.RawText;
                m = new(a1);
            }
            else if (arg is PrivateMessageEventArgs a2)
            {
                msg = a2.Message.RawText;
                m = new(a2);
            }
            if (!string.IsNullOrEmpty(msg))
            {
                ConsoleLog.Debug("Commands", "Parser: " + msg);
                foreach (var node in Commands)
                {
                    if (msg.StartsWith(node.Key))
                    {
                        var caa = msg[node.Key.Length..].Split(" ", 2);
                        if (node.Value.TryGetValue(caa[0].ToLower(), out var commandlist))
                        {
                            ConsoleLog.Debug("Commands", $"found {commandlist.Count} command(s) for {caa[0]}");
                            var args = new CommandArgs(caa.Length == 2 ? caa[1] : "");
                            foreach (var command in commandlist.OrderByDescending(x => x.ArgTypes.Length).ToArray())
                            {
                                if (args.Length >= (command.ArgTypes.Where(a => !a.HasDefaultValue && !IsParams(a)).Count() - 1)
                                    && await command.CheckPermission(arg))
                                {
                                    var ps = new ArrayList
                                    {
                                        m
                                    };
                                    try
                                    {
                                        ConsoleLog.Debug("Commands", $"Try to parse parameters for {command.Method}.");
                                        args.Reset();
                                        bool p = false;
                                        ParameterInfo pt = null;
                                        for (int i = 1; i < command.ArgTypes.Length; ++i)
                                        {
                                            if (i == command.ArgTypes.Length - 1 && IsParams(command.ArgTypes[i]))
                                            {
                                                p = true;
                                                pt = command.ArgTypes[i];
                                            }
                                            else
                                                ps.Add(ParseArgument(command.ArgTypes[i], args));
                                        }
                                        if (p)
                                        {
                                            var pp = new ArrayList();
                                            while (args.HasNext())
                                                pp.Add(ParseArgument(pt, args, true));
                                            ps.Add(pp.ToArray(pt.ParameterType.GetElementType()));
                                        }
                                        ConsoleLog.Debug("Commands", $"Invoke {command.Method}.");
                                        command.Method.Invoke(null, ps.ToArray());
                                        return;
                                    }
                                    catch (Exception ex)
                                    {
                                        ConsoleLog.Debug("Commands", ex.GetFormatString());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
