using System.Collections.Generic;

namespace Ritsukage.Library.OCRSpace.Struct
{
    public class TextOverlay
    {
        public List<Line> Lines { get; set; }
        public bool HasOverlay { get; set; }
        public string Message { get; set; }
    }
}
