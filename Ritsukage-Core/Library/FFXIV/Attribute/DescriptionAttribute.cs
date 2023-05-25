using System;

namespace Ritsukage.Library.FFXIV.Attribute
{
    [AttributeUsage(AttributeTargets.All)]
    public class DescriptionAttribute : System.Attribute
    {
        public string English { get; set; }
        public string Chinese { get; set; }

        public DescriptionAttribute(string english, string chinese = null)
        {
            English = english;
            Chinese = chinese ?? english;
        }
    }
}
