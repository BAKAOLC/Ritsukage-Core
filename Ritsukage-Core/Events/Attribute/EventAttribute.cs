using System;

namespace Ritsukage.Events
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EventAttribute : Attribute
    {
        public Type Handled { get; init; }
        public int Level { get; init; }

        public EventAttribute(Type events, int level)
        {
            Handled = events;
            Level = level;
        }
    }
}
