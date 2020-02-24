using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventStore
{
    public class AggregateRepository<TAggregate> where TAggregate : Aggregate, new()
    {
        private readonly IEventStore eventStore;
        private readonly IBus bus;
        private readonly ISnapshotStore snapshotStore;
        private readonly int snapshotInterval;

        public AggregateRepository(IEventStore eventStore, IBus bus, ISnapshotStore snapshotStore, int snapshotInterval = 100)
        {
            this.eventStore = eventStore;
            this.bus = bus;
            this.snapshotStore = snapshotStore;
            this.snapshotInterval = snapshotInterval;
        }

        public async Task<TAggregate> Get(Guid uid)
        {
            var aggretateFromSnapshot = await snapshotStore.LoadSnapshot<TAggregate>(uid);

            var aggregate = aggretateFromSnapshot ?? new TAggregate { Uid = uid };

            var events = await eventStore.GetEventStream(uid, aggregate.LastEventId);

            aggregate.Rehydrate(events);

            return aggregate;
        }

        public async Task Save(TAggregate aggregate)
        {
            var stream = aggregate.GetUncommittedEventStream();

            if (!stream.Any())
            {
                return;
            }

            await eventStore.SaveEventStream(stream);

            aggregate.EventCount += stream.Count;

            aggregate.LastEventId = stream.Last().Id;

            if (aggregate.EventCount % snapshotInterval == 0)
            {
                await snapshotStore.SaveSnapshot(aggregate);
            }

            aggregate.ResetEventStream();

            foreach (var @event in stream)
            {
                //this cannot be an abstract class because type is used for routing
                //need to invoke the generic type here
                await bus.Publish(@event, @event.GetType());
            }
        }
    }
}
