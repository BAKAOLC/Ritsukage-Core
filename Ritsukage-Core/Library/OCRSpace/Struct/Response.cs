using Ritsukage.Library.OCRSpace.Enum;
using System.Collections.Generic;

namespace Ritsukage.Library.OCRSpace.Struct
{
    public class Response
    {
        public List<ParsedResult> ParsedResults { get; set; }
        public OCRExitCode OCRExitCode { get; set; }
        public bool IsErroredOnProcessing { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorDetails { get; set; }
        public string ProcessingTimeInMilliseconds { get; set; }
    }
}
