using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventStore
{
    public interface ISnapshotStore
    {
        Task<TAggregate> LoadSnapshot<TAggregate>(Guid uid) where TAggregate : Aggregate;
        Task SaveSnapshot<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate;
    }
}
