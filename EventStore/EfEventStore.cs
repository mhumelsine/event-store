//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace EventStore
//{
//    public class EfEventDbContext : DbContext
//    {
//        public DbSet<DbEventMeta> EventMetas { get; set; }
//        public DbSet<DbEventMeta> Events { get; set; }


//        public EfEventDbContext(DbContextOptions<EfEventDbContext> options)
//            : base(options)
//        {
//        }

//        public EfEventDbContext()
//        {

//        }

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            modelBuilder.HasDefaultSchema("events");

//            modelBuilder.Entity<DbEventMeta>(entity =>
//            {
//                entity.HasKey(x => x.EventId)
//                    .IsClustered(true);

//                entity.ToTable("EVENT_META");

//                entity.Property(e => e.AggregateUid)
//                    .HasColumnName("UID");

//                entity.Property(e => e.EventId)
//                    .HasColumnName("ID_EVENT")
//                    .UseIdentityColumn(1, 1);

//                entity.Property(e => e.Timestamp)
//                    .HasColumnName("DT_EVENT");

//                entity.Property(e => e.Type)
//                    .HasColumnName("NM_EVENT");

//                entity.HasOne(e => e.Event)
//                    .WithOne(e => e.Meta);
//            });

//            modelBuilder.Entity<DbEvent>(entity =>
//            {
//                entity.ToTable("EVENT_DATA");

//                entity.HasKey(x => x.EventId)
//                    .IsClustered(false);

//                entity.Property(e => e.EventId)
//                    .HasColumnName("ID_EVENT");

//                entity.Property(e => e.Data)
//                    .HasColumnName("JS_EVENT");
//            });
//        }
//    }


//    public class DbEventMeta
//    {
//        public Guid AggregateUid { get; set; }
//        public int EventId { get; set; }
//        public DateTime Timestamp { get; set; }
//        public string Type { get; set; }
//        public DbEvent Event { get; set; }
//    }

//    public class DbEvent
//    {
//        public int EventId { get; set; }
//        public string Data { get; set; }

//        public DbEventMeta Meta { get; set; }
//    }

//    public static class DbEventFactory
//    {
//        public static DbEventMeta Create(this Event @event, IEventSerializer serializer)
//        {
//            var dbEvent = new DbEventMeta
//            {
//                AggregateUid = @event.CorrelationId,
//                Timestamp = @event.Timestamp,
//                Type = @event.GetType().Name,
//                Event = new DbEvent
//                {
//                    Data = serializer.Serialize(@event)
//                }
//            };

//            return dbEvent;
//        }

//        public static Event Create(this DbEvent @event, IEventSerializer serializer)
//        {
//            return serializer.Deserialize(@event.Data);
//        }
//    }

//    public class EfEventStore : IEventStore
//    {
//        private readonly EfEventDbContext context;
//        private readonly IEventSerializer serializer;
//        public EfEventStore(EfEventDbContext context, IEventSerializer serializer)
//        {
//            this.context = context;
//            this.serializer = serializer;
//        }

//        public async Task<List<Event>> GetEventStream(Guid uid)
//        {
//            var stream = await context.EventMetas
//                 .Include(m => m.Event)
//                 .Where(m => m.AggregateUid == uid)
//                 .OrderBy(m => m.EventId)
//                 .ToListAsync();

//            return stream
//                .Select(e => e.Event.Create(serializer))
//                .ToList();
//        }

//        public async Task SaveEventStream(List<Event> stream)
//        {
//            var events = stream.Select(e => e.Create(serializer));

//            await context.EventMetas.AddRangeAsync(events);

//            try
//            {

//                await context.SaveChangesAsync();
//            }catch(Exception ex)
//            {
//                Console.WriteLine(ex);
//            }
//        }
//    }
//}
