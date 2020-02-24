using System;
using System.Collections.Generic;
using System.Text;

namespace EventStore
{
    public class Snapshot
    {
        public Guid AggregateUid { get; set; }
        public string SerializedState { get; set; }
    }
}
