using System;

namespace Ritsukage.QQ.Events
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EventAttribute : Attribute
    {
        public Type Handled { get; init; }

        public bool HandleOthers { get; init; }

        public bool HandleSelf { get; init; }

        public EventAttribute(Type events, bool handleSelf = false, bool handleOthers = true)
        {
            Handled = events;
            HandleSelf = handleSelf;
            HandleOthers = handleOthers;
        }
    }
}
