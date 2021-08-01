using System;

namespace Ritsukage.QQ.Commands
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CommandDescriptionAttribute : Attribute
    {
        public string[] Desc { get; init; }

        public CommandDescriptionAttribute(params string[] desc)
        {
            Desc = desc;
        }

        public override string ToString()
            => string.Join(Environment.NewLine, Desc);
    }
}
