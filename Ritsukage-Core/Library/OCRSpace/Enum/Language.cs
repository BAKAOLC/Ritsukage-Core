using Ritsukage.Library.OCRSpace.Attribute;

namespace Ritsukage.Library.OCRSpace.Enum
{
    public enum Language
    {
        [Description("Arabic")]
        ara,
        [Description("Bulgarian")]
        bul,
        [Description("Chinese Simplified")]
        chs,
        [Description("Chinese Traditional")]
        cht,
        [Description("Croatian")]
        hrv,
        [Description("Czech")]
        cze,
        [Description("Danish")]
        dan,
        [Description("Dutch")]
        dut,
        [Description("English")]
        eng,
        [Description("Finnish")]
        fin,
        [Description("French")]
        fre,
        [Description("German")]
        ger,
        [Description("Greek")]
        gre,
        [Description("Hungarian")]
        hun,
        [Description("Korean")]
        kor,
        [Description("Italian")]
        ita,
        [Description("Japanese")]
        jpn,
        [Description("Polish")]
        pol,
        [Description("Portuguese")]
        por,
        [Description("Russian")]
        rus,
        [Description("Slovenian")]
        slv,
        [Description("Spanish")]
        spa,
        [Description("Swedish")]
        swe,
        [Description("Turkish")]
        tur,
        #region Engine3 Support
        [Description("Hindi")]
        hin,
        [Description("Kannada")]
        kan,
        [Description("Persian (Fari)")]
        per,
        [Description("Telugu")]
        tel,
        [Description("Tamil")]
        tam,
        [Description("Thai")]
        tai,
        [Description("Vietnamese")]
        vie,
        #endregion
        Default = eng
    }
}
