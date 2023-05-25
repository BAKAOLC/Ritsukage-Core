using System;

namespace Ritsukage.Library.FFXIV.XivAPI.Attribute
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ApiHostUrlAttribute : System.Attribute
    {
        public string Url { get; set; }
        public ApiHostUrlAttribute(string url)
        {
            Url = url;
        }
    }
}
