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
        private readonly IEventSerializer serializer;

        public DapperEventStore(string connectionString, IEventSerializer serializer)
        {
            this.connectionString = connectionString;
            this.serializer = serializer;
        }
        public async Task<List<Event>> GetEventStream(Guid uid)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var sql =
@"select JS_EVENT
from events.EVENT_META 
where UID = @uid
order by ID_EVENT asc";

                await connection.OpenAsync();

                var serializedEvents = await connection.QueryAsync<string>(sql, new { uid });

                return serializedEvents
                    .Select(e => serializer.Deserialize(e))
                    .ToList();
            }
        }

        const string sql = "SET NOCOUNT ON; insert into events.EVENT_META(UID, DT_EVENT, NM_EVENT, JS_EVENT) values(@Uid, @Timestamp, @EventName, @Data);";

        public async Task SaveEventStream(List<Event> stream)
        {
            //var sw = new System.Diagnostics.Stopwatch();
            //sw.Start();
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                foreach (var e in stream)
                {
                    //var sql = "SET NOCOUNT ON; insert into events.EVENT_META(UID, DT_EVENT, NM_EVENT) values(@Uid, @Timestamp, @EventName);insert into events.EVENT_DATA(ID_EVENT, JS_EVENT) values((select SCOPE_IDENTITY()), @Data);";
                    
                    try
                    {
                        //var cw = new System.Diagnostics.Stopwatch();
                        //cw.Start();
                        await connection.ExecuteAsync(sql, new
                        {
                            Uid = e.Uid,
                            Timestamp = e.Timestamp,
                            EventName = e.GetType().Name,
                            Data = serializer.Serialize(e)
                        });
                        //cw.Stop();
                        //Console.WriteLine($"Command: {cw.Elapsed.TotalMilliseconds}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }

            //sw.Stop();
            //Console.WriteLine(sw.Elapsed.TotalMilliseconds);
        }


    }
}
