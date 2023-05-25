using Ritsukage.Library.FFXIV.Attribute;
using System.Runtime.Serialization;

namespace Ritsukage.Library.FFXIV.Enum
{
    public enum TheTwelve
    {
        [Description("Halone",      "哈罗妮")]
        Halone,
        [Description("Menphina",    "梅茵菲娜")]
        Menphina,
        [Description("Thaliak",     "沙利亚克")]
        Thaliak,
        [Description("Nymeia",      "妮美雅")]
        Nymeia,
        [Description("Llymlaen",    "利姆莱茵")]
        Llymlaen,
        [Description("Oschon",      "奥修昂")]
        Oschon,
        [Description("Byregot",     "比尔格")]
        Byregot,
        [Description("Rhalgr",      "拉尔戈")]
        Rhalgr,
        [Description("Azeyma",      "阿泽玛")]
        Azeyma,
        [Description("Nald'Thal",   "纳尔札尔")]
        [EnumMember(Value = "Nald'thal")]
        Nald_Thal,
        [Description("Nophica",     "诺菲卡")]
        Nophica,
        [Description("Althyk",      "阿尔基克")]
        Althyk,
    }
}
