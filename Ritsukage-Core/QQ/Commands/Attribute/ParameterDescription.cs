using System;

namespace Ritsukage.QQ.Commands
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ParameterDescriptionAttribute : Attribute
    {
        public int Index { get; init; }
        public string Name { get; init; }
        public string Desc { get; init; }

        public ParameterDescriptionAttribute(int index, string name, string desc = "")
        {
            Index = index;
            Name = name;
            Desc = desc;
        }

        public override string ToString()
            => $"Parameter#{Index} {Name}";
    }
}
