using System;

namespace Ritsukage.QQ.Commands
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CommandArgumentErrorCallbackAttribute : Attribute
    {
        public string ArgumentErrorCallbackMethodName { get; init; }

        public CommandArgumentErrorCallbackAttribute(string methodName)
        {
            ArgumentErrorCallbackMethodName = methodName;
        }
    }
}
