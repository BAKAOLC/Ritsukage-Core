using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Help")]
    public static class Help
    {

        [Command("帮助", "Help")]
        [CommandDescription("获取指定指令的帮助")]
        public static async void GetHelp(SoraMessage e)
        {
            await e.ReplyToOriginal("不支持无参数帮助列表捏");
        }

        [Command("帮助", "Help")]
        [CommandDescription("获取指定指令的帮助")]
        [ParameterDescription(1, "指令名")]
        public static async void GetHelpForCommand(SoraMessage e, string command_str)
        {
            if (string.IsNullOrWhiteSpace(command_str))
            {
                await e.ReplyToOriginal("提供的参数非法捏");
                return;
            }
            var self_attr = Attribute.GetCustomAttributes(typeof(Help).GetMethod("GetHelpForCommand"), true).Where(a => a is CommandAttribute).FirstOrDefault() as CommandAttribute;
            if (self_attr == null) return;
            var header = self_attr.StartHeader;
            var lc = command_str.ToLower();
            var matches = CommandManager.Commands
                .Where(x => x.Key == header)
                .Select(x => x.Value.Where(y => y.Key.Contains(lc)).OrderBy(y => y.Key).Select(y => y.Value));
            List<Command> commands = new();
            foreach (var x in matches)
                foreach (var y in x)
                    foreach (var command in y.OrderByDescending(x => x.ArgTypes.Length))
                        if (await command.CheckPermission(e.Event))
                            commands.Add(command);
            if (commands.Any())
            {
                var methods = commands.Select(x => x.Method);
                var sb = new StringBuilder();
                foreach (var method in methods)
                {
                    var attrs = method.GetCustomAttribute<CommandAttribute>();
                    var ps = method.GetParameters();
                    var ts = new string[ps.Length];
                    for (int i = 0; i < ps.Length; ++i)
                        ts[i] = $"{ps[i].Name}:{ps[i].ParameterType.Name}";
                    var name = attrs.Name;
                    if (name.Length == 0)
                        name = new[] { method.Name };
                    sb.AppendLine("Command: " + string.Join("|", name));
                    var cd = method.GetCustomAttribute<CommandDescriptionAttribute>();
                    if (cd != null)
                        sb.AppendLine(cd.ToString());
                    var param = method.GetParameters();
                    if (param.Length > 1)
                        sb.AppendLine("Parameters:");
                    var pds = method.GetCustomAttributes<ParameterDescriptionAttribute>();
                    foreach (var pm in param.Skip(1))
                    {
                        var pd = pds.Where(x => x.Index == pm.Position).FirstOrDefault();
                        sb.Append("    ");
                        if (pd == null)
                            sb.AppendLine($"Parameter#{pm.Position} {pm.Name}:{pm.ParameterType.Name}{(pm.HasDefaultValue ? $"={(pm.DefaultValue.GetType() == typeof(string) ? $"\"{((string)pm.DefaultValue).Replace("\\", "\\\\").Replace("\"", "\\\"")}\"" : pm.DefaultValue)}" : string.Empty)}");
                        else
                            sb.AppendLine($"{pd}:{pm.ParameterType.Name}{(pm.HasDefaultValue ? $"={(pm.DefaultValue.GetType() == typeof(string) ? $"\"{((string)pm.DefaultValue).Replace("\\", "\\\\").Replace("\"", "\\\"")}\"" : pm.DefaultValue)}" : string.Empty)}{(string.IsNullOrWhiteSpace(pd.Desc) ? string.Empty : (" " + pd.Desc))}");
                    }
                }
                sb.Append($"=== 共找到有权使用的 {methods.Count()} 个方法 ===");
                await e.ReplyToOriginal(sb.ToString());
            }
            else
            {
                await e.ReplyToOriginal("没有匹配到任何指令捏");
            }
        }
    }
}
