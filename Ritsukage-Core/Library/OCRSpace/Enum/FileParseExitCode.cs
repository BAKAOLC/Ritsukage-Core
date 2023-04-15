using Ritsukage.Library.OCRSpace.Attribute;

namespace Ritsukage.Library.OCRSpace.Enum
{
    public enum FileParseExitCode
    {
        [Description("File not found")]
        NotFound = 0,
        [Description("Success")]
        Success = 1,
        [Description("OCR Engine Parse Error")]
        Error = -10,
        [Description("Timeout")]
        Timeout = -20,
        [Description("Validation Error")]
        ValidationError = -30,
        [Description("Unknown Error")]
        UnknownError = -99,
    }
}