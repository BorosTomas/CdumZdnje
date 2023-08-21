using System.Collections.Generic;
using System;

namespace CodiumZadanie
{
    internal class EventData
    {
        public int ProviderEventID { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public IList<OddsData> OddsList { get; set; }
    }
}