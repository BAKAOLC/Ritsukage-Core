using System;

namespace Ritsukage.QQ.Commands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string StartHeader { get; init; } = "+";
        public string[] Name { get; init; }

        public CommandAttribute(params string[] name)
        {
            Name = name;
        }
    }
}
