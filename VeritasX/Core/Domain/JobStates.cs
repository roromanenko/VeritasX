using VeritasX.Infrastructure.Persistence.Entities;

namespace VeritasX.Core.Domain;

public enum CollectionState
{
	Pending,
	InProgress,
	Completed,
	Failed,
	Cancelled
}

public enum ChunkState
{
	Pending,
	InProgress,
	Completed,
	Failed
}