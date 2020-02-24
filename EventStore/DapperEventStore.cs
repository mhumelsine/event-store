using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventStore
{
    public class DapperEventStore : IEventStore
    {
        private readonly string connectionString;
        private readonly ISerializer serializer;
        private const string LOAD_STATEMENT = "select JS_EVENT from events.EVENT_META where UID_AGGREGATE = @uid and ID_EVENT > @eventId order by ID_EVENT asc;";
        private const string INSERT_STATEMENT = "SET NOCOUNT ON; insert into events.EVENT_META(UID_AGGREGATE, DT_EVENT, NM_EVENT, JS_EVENT) values(@AggregateUid, @Timestamp, @EventName, @Data) select scope_identity();";

        public DapperEventStore(string connectionString, ISerializer serializer)
        {
            this.connectionString = connectionString;
            this.serializer = serializer;
        }
        public async Task<List<Event>> GetEventStream(Guid uid, long eventId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var serializedEvents = await connection.QueryAsync<string>(LOAD_STATEMENT, new { uid, eventId });

                connection.Close();

                return serializedEvents
                    .Select(e => serializer.Deserialize<Event>(e))
                    .ToList();
            }
        }

        public async Task SaveEventStream(List<Event> stream)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                foreach (var e in stream)
                {
                    e.Id = await connection.ExecuteScalarAsync<long>(INSERT_STATEMENT, new
                    {
                        AggregateUid = e.AggregateUid,
                        Timestamp = e.Timestamp,
                        EventName = e.GetType().Name,
                        Data = serializer.Serialize(e)
                    });
                }
                connection.Close();
            }
        }
    }
}
