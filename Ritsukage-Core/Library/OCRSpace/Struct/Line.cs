using System.Collections.Generic;

namespace Ritsukage.Library.OCRSpace.Struct
{
    public class Line
    {
        public List<Word> Words { get; set; }
        public double MaxHeight { get; set; }
        public double MinTop { get; set; }
    }
}
