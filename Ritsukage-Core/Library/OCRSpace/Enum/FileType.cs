using Ritsukage.Library.OCRSpace.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.Library.OCRSpace.Enum
{
    public enum FileType
    {
        Auto,
        [Description(".jpg")]
        JPG,
        [Description(".png")]
        PNG,
        [Description(".bmp")]
        BMP,
        [Description(".gif")]
        GIF,
        [Description(".tiff")]
        TIF,
        [Description(".pdf")]
        PDF
    }
}
