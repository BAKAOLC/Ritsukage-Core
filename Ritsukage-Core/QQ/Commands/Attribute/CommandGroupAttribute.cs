using System;

namespace Ritsukage.QQ.Commands
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandGroupAttribute : Attribute
    {
        public string Name { get; init; } = string.Empty;

        public CommandGroupAttribute() { }
        public CommandGroupAttribute(string name) => Name = name;

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
                return $"[Command Group]";
            else
                return $"[Command Group: {Name}]";
        }
    }
}
