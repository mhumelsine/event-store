use merlin;

go

create schema EventStore;

GO

--alter database merlin set auto_close off

alter database merlin add filegroup EventStore
	contains memory_optimized_data;

alter database merlin add file (
	name='EventStore', filename='C:\Program Files\Microsoft SQL Server\MSSQL13.MSSQLSERVER\MSSQL\DATA\events')
	to filegroup EventStore;

alter database merlin
	set memory_optimized_elevate_to_snapshot = on;

go

create table EventStore.EVENT_STREAM(
	UID_AGGREGATE uniqueidentifier not null index idx_aggregate_uid nonclustered hash with (bucket_count=10000000),
	ID_EVENT bigint not null identity,
	DT_EVENT datetime2 not null,
	NM_EVENT varchar(255) not null,
	JS_EVENT varchar(max) not null,
	constraint PK_EVENT primary key nonclustered(ID_EVENT)
) with (memory_optimized=on);

create table EventStore.STATE_SNAPSHOT(
	UID_AGGREGATE uniqueidentifier not null index idx_aggregate_uid nonclustered hash with (bucket_count=10000000),
	JS_SNAPSHOT varchar(max) not null,
	constraint PK_EVENT_SNAPSHOT primary key nonclustered(UID_AGGREGATE)
) with (memory_optimized=on);

go