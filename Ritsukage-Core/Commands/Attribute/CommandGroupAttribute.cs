using System;

namespace Ritsukage.Commands
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandGroupAttribute : Attribute
    {
    }
}
