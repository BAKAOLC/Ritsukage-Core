using Ritsukage.Library.FFXIV.XivAPI.Attribute;
using System.ComponentModel;

namespace Ritsukage.Library.FFXIV.XivAPI.Enum
{
    [DefaultValue(Original)]
    public enum ApiHost
    {
        [ApiHostUrl("https://xivapi.com"), ]
        Original,
        [ApiHostUrl("https://cafemaker.wakingsands.com")]
        FFCafe,
    }
}
