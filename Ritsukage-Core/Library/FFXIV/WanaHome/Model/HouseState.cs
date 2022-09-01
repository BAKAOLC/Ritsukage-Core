using System.Collections.Generic;

namespace Ritsukage.Library.FFXIV.WanaHome.Model
{
    public class HouseState
    {
        public House Data { get; init; }
        
        public List<Changes> Changes { get; init; }
    }
}
