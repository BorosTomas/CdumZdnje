using System;

namespace CodiumZadanie
{
    internal class Message
    {
        public Guid MessageID { get; set; }
        public DateTime GeneratedDate { get; set; }
        public EventData Event { get; set; }
    }
}