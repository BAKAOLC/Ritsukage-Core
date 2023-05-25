using Ritsukage.Library.FFXIV.Attribute;
using System;
using System.Reflection;

namespace Ritsukage.Library.FFXIV.Enum
{
    public enum DescriptionLanguage
    {
        English,
        Chinese
    }

    public static class DescriptionAttributeExtension
    {
        static readonly Type DescriptionAttribute = typeof(DescriptionAttribute);

        public static string GetDescription(this DescriptionLanguage language, object target)
        {
            FieldInfo field = target.GetType().GetField(target.ToString());
            if (field.IsDefined(DescriptionAttribute))
                return language switch
                {
                    DescriptionLanguage.Chinese => field.GetCustomAttribute<DescriptionAttribute>().Chinese,
                    _ => field.GetCustomAttribute<DescriptionAttribute>().English,
                };
            return target.ToString();
        }
    }
}
