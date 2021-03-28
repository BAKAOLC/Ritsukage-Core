using Ritsukage.QQ.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CommandDocumentGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            StringWriter file = new();
            file.NewLine = Environment.NewLine;

            Type[] types = Assembly.GetAssembly(typeof(CommandAttribute)).GetExportedTypes();
            Type[] cosType = types.Where(t => Attribute.GetCustomAttributes(t, true).Where(a => a is CommandGroupAttribute).Any()).ToArray();
            foreach (var type in cosType.OrderBy(t=>t.GetCustomAttribute<CommandGroupAttribute>().Name))
            {
                file.WriteLine(type.GetCustomAttribute<CommandGroupAttribute>().ToString());
                var preconditions = new List<PreconditionAttribute>();
                var gp = type.GetCustomAttributes()?.Where(x => x is PreconditionAttribute)?.ToList();
                if (gp != null)
                    foreach (PreconditionAttribute a in gp)
                        preconditions.Add(a);
                foreach (var method in type.GetMethods()
                    .Where(t => Attribute.GetCustomAttributes(t, true).Where(a => a is CommandAttribute).Any())
                    .OrderBy(t => t.GetCustomAttribute<CommandAttribute>().StartHeader)
                    .ThenBy(t => {
                        var n = t.GetCustomAttribute<CommandAttribute>().Name;
                        if (n.Length > 0)
                            return n[0];
                        else
                            return t.Name;
                    })
                    .ThenByDescending(t => t.GetParameters().Length))
                {
                    var attrs = method.GetCustomAttribute<CommandAttribute>();
                    var list = new List<PreconditionAttribute>();
                    foreach (var a in preconditions)
                        list.Add(a);
                    var p = method.GetCustomAttributes()?.Where(x => x is PreconditionAttribute)?.ToList();
                    if (p != null)
                        foreach (PreconditionAttribute a in p)
                            list.Add(a);
                    if (list.Count > 0)
                        file.WriteLine(string.Join(string.Empty, list));
                    var ps = method.GetParameters();
                    var ts = new string[ps.Length];
                    for (int i = 0; i < ps.Length; ++i)
                        ts[i] = $"{ps[i].Name}:{ps[i].ParameterType.Name}";
                    var name = attrs.Name;
                    if (name.Length == 0)
                        name = new[] { method.Name };
                    //file.WriteLine("Header: " + attrs.StartHeader);
                    file.WriteLine("Command: " + string.Join("|", name));
                    var param = method.GetParameters();
                    if (param.Length > 1)
                    file.WriteLine("Parameters:");
                    foreach (var pm in param.Skip(1))
                    {
                        file.Write("    ");
                        file.WriteLine($"{pm.Name}:{pm.ParameterType.Name}{(pm.HasDefaultValue ? $"={pm.DefaultValue}" : string.Empty)}");
                    }
                }
                file.WriteLine(Environment.NewLine);
            }

            File.WriteAllText("qq.txt", file.ToString());
        }
    }
}
