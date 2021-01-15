using System;

namespace Ritsukage.QQ.Events
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EventAttribute : Attribute
    {
        public Type Handled { get; init; }

        public EventAttribute(Type events)
        {
            Handled = events;
        }
    }
}
