using System;
using System.Collections.Generic;
using System.Text;

namespace EventStore
{
    public abstract class Event
    {
        public Guid Uid { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
