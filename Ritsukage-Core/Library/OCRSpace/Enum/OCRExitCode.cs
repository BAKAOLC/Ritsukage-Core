using Ritsukage.Library.OCRSpace.Attribute;

namespace Ritsukage.Library.OCRSpace.Enum
{
    public enum OCRExitCode
    {
        [Description("Parsed Successfully (Image / All pages parsed successfully)")]
        Success = 1,
        [Description("Parsed Partially (Only few pages out of all the pages parsed successfully)")]
        Partially = 2,
        [Description("Image / All the PDF pages failed parsing (This happens mainly because the OCR engine fails to parse an image)")]
        Failed = 3,
        [Description("Error occurred when attempting to parse (This happens when a fatal error occurs during parsing )")]
        Error = 4,
    }
}
