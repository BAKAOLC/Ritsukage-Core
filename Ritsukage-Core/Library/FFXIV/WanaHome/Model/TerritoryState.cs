using System;
using System.Collections.Generic;

namespace Ritsukage.Library.FFXIV.WanaHome.Model
{
    public class TerritoryState
    {
        public List<House> OnSale { get; init; }
        
        public List<Changes> Changes { get; init; }

        public DateTimeOffset LastUpdate { get; init; }
    }
}
