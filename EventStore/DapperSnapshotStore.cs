using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventStore
{
    public class DapperSnapshotStore : ISnapshotStore
    {
        private readonly string connectionString;
        private readonly ISerializer serializer;
        private const string LOAD_STATEMENT = "select UID_AGGREGATE AggregateUid, JS_SNAPSHOT SerializedState from EventStore.STATE_SNAPSHOT where UID_AGGREGATE = @uid;";
        private const string INSERT_STATEMENT = "SET NOCOUNT ON; update EventStore.STATE_SNAPSHOT set JS_SNAPSHOT = @SerializedState where UID_AGGREGATE = @AggregateUid if @@rowcount = 0 insert into EventStore.STATE_SNAPSHOT(UID_AGGREGATE, JS_SNAPSHOT) values(@AggregateUid, @SerializedState);";

        public DapperSnapshotStore(string connectionString, ISerializer serializer)
        {
            this.connectionString = connectionString;
            this.serializer = serializer;
        }
        public async Task<TAggregate> LoadSnapshot<TAggregate>(Guid uid) where TAggregate : Aggregate
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var snapshot = await connection.QuerySingleOrDefaultAsync<Snapshot>(LOAD_STATEMENT, new { uid });

                connection.Close();

                if (snapshot == null)
                {
                    return null;
                }

                var aggregate = serializer.Deserialize<TAggregate>(snapshot.SerializedState);

                return aggregate;
            }
        }

        public async Task SaveSnapshot<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var snapshot = new Snapshot
                {
                    AggregateUid = aggregate.Uid,
                    SerializedState = serializer.Serialize(aggregate)
                };

                await connection.OpenAsync();
                await connection.ExecuteAsync(INSERT_STATEMENT, snapshot);
                connection.Close();
            }
        }
    }
}
