using System;
using System.Reflection;

namespace Ritsukage.Library.OCRSpace.Attribute
{
    [AttributeUsage(AttributeTargets.All)]
    public class DescriptionAttribute : System.Attribute
    {
        public string Description { get; set; }

        public DescriptionAttribute(string description)
        {
            Description = description;
        }
    }

    public static class DescriptionAttributeExtension
    {
        static readonly Type DescriptionAttribute = typeof(DescriptionAttribute);

        public static string GetDescription(this object target)
        {
            FieldInfo field = target.GetType().GetField(target.ToString());
            if (field.IsDefined(DescriptionAttribute))
                return field.GetCustomAttribute<DescriptionAttribute>().Description;
            return target.ToString();
        }
    }
}