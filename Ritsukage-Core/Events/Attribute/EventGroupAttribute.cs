using System;

namespace Ritsukage.Events
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EventGroupAttribute : Attribute
    {
    }
}
