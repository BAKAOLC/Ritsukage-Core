using System;

namespace Ritsukage.Library.OCRSpace.Enum
{
    public enum OCREngine
    {
        Engine1 = 1,
        [Obsolete("Please use Engine5")]
        Engine2 = 2,
        Engine3 = 3,
        Engine5 = 5,
        Default = Engine1
    }
}