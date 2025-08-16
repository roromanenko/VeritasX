using AutoMapper;
using Infrastructure.Persistence.Entities;
using Core.Domain;
using MongoDB.Bson;

namespace Infrastructure.Mapping.Profiles;

public class DataCollectionJobProfile : Profile
{
	public DataCollectionJobProfile()
	{
		CreateMap<DataCollectionJob, DataCollectionJobDocument>()
			.ConstructUsing(s => new DataCollectionJobDocument
			{
				Id = ObjectId.Parse(s.Id),
				UserId = ObjectId.Parse(s.UserId),
				Symbol = s.Symbol,
				FromUtc = s.FromUtc.UtcDateTime,
				ToUtc = s.ToUtc.UtcDateTime,
				Interval = s.Interval,
				CollectionName = s.CollectionName,
				State = s.State,
				TotalChunks = s.TotalChunks,
				CompletedChunks = s.CompletedChunks,
				CreatedAt = s.CreatedAt.UtcDateTime,
				StartedAt = s.StartedAt.HasValue ? s.StartedAt.Value.UtcDateTime : (DateTime?)null,
				CompletedAt = s.CompletedAt.HasValue ? s.CompletedAt.Value.UtcDateTime : (DateTime?)null,
				ErrorMessage = s.ErrorMessage,
				Chunks = s.Chunks.Select(c => new DataChunkDocument
				{
					FromUtc = c.FromUtc.UtcDateTime,
					ToUtc = c.ToUtc.UtcDateTime,
					State = c.State,
					RetryCount = c.RetryCount,
					ErrorMessage = c.ErrorMessage,
				}).ToList(),
			});

		CreateMap<DataCollectionJobDocument, DataCollectionJob>()
			.ConstructUsing(d => new DataCollectionJob
			{
				Id = d.Id.ToString(),
				UserId = d.UserId.ToString(),
				Symbol = d.Symbol,
				FromUtc = new DateTimeOffset(DateTime.SpecifyKind(d.FromUtc, DateTimeKind.Utc)),
				ToUtc = new DateTimeOffset(DateTime.SpecifyKind(d.ToUtc, DateTimeKind.Utc)),
				Interval = d.Interval,
				CollectionName = d.CollectionName,
				State = d.State,
				TotalChunks = d.TotalChunks,
				CompletedChunks = d.CompletedChunks,
				CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(d.CreatedAt, DateTimeKind.Utc)),
				StartedAt = d.StartedAt == null ? null : new DateTimeOffset(DateTime.SpecifyKind(d.StartedAt.Value, DateTimeKind.Utc)),
				CompletedAt = d.CompletedAt == null ? null : new DateTimeOffset(DateTime.SpecifyKind(d.CompletedAt.Value, DateTimeKind.Utc)),
				ErrorMessage = d.ErrorMessage,
				Chunks = d.Chunks.Select(c => new DataChunk
				{
					FromUtc = new DateTimeOffset(DateTime.SpecifyKind(c.FromUtc, DateTimeKind.Utc)),
					ToUtc = new DateTimeOffset(DateTime.SpecifyKind(c.ToUtc, DateTimeKind.Utc)),
					State = c.State,
					RetryCount = c.RetryCount,
					ErrorMessage = c.ErrorMessage
				}).ToList()
			});
	}
}