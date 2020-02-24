using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EventStore
{   
    public abstract class Aggregate
    {
        public Guid Uid { get; set; }
        public long LastEventId { get; set; }
        public long EventCount { get; set; }
        private readonly List<Event> uncommittedEvents = new List<Event>();

        private static ConcurrentDictionary<Type, Action<object, object>> handlers =
            new ConcurrentDictionary<Type, Action<object, object>>();

        public Aggregate()
        {
            LastEventId = 0;
        }

        public List<Event> GetUncommittedEventStream()
        {
            return uncommittedEvents.ToList();
        }

        public void ResetEventStream()
        {
            uncommittedEvents.Clear();
        }

        public void Apply(Event @event)
        {
            @event.AggregateUid = Uid;
            @event.Timestamp = DateTime.Now;
            Dispatch(@event);
            uncommittedEvents.Add(@event);
        }

        public void Rehydrate(List<Event> events)
        {
            foreach (var @event in events)
            {
                Dispatch(@event);
            }

            EventCount += events.Count;
        }

        private void Dispatch(Event @event)
        {
            if (!handlers.TryGetValue(@event.GetType(), out var handler))
            {
                handler = HandlerBuilder(GetType(), @event.GetType());
                handlers.TryAdd(@event.GetType(), handler);
            }

            handler(this, @event);
        }

        private static Action<object, object> HandlerBuilder(Type handlerType, Type eventType)
        {
            var instance = Expression.Parameter(typeof(object), "handler");
            var eventParam = Expression.Parameter(typeof(object), "@event");

            var method = handlerType
                .GetMethod("On", new Type[] { eventType });

            if(method == null)
            {
                throw new InvalidOperationException($"The Method: 'On({eventType.Name} @event)' not found.  Make sure an event handler exists in '{handlerType.Name}'");
            }

            var methodCall = Expression.Call(
                Expression.Convert(instance, handlerType),
                method,
                Expression.Convert(eventParam, eventType));

            return Expression.Lambda<Action<object, object>>(
                methodCall,
                instance,
                eventParam
                ).Compile();
        }
    }
}
