using Ritsukage.Library.OCRSpace.Enum;

namespace Ritsukage.Library.OCRSpace.Struct
{
    public class ParsedResult
    {
        public TextOverlay TextOverlay { get; set; }
        public FileParseExitCode FileParseExitCode { get; set; }
        public string ParsedText { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorDetails { get; set; }
    }
}