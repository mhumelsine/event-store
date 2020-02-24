using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventStore
{
    public interface IEventStore
    {
        Task<List<Event>> GetEventStream(Guid uid, long eventId);
        Task SaveEventStream(List<Event> stream);
    }
}
