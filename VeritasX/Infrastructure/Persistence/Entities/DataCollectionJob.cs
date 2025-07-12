using VeritasX.Core.Domain;

namespace VeritasX.Infrastructure.Persistence.Entities;

public record DataCollectionJob
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Symbol { get; init; }
    public required DateTime FromUtc { get; init; }
    public required DateTime ToUtc { get; init; }
    public required TimeSpan Interval { get; init; }
    public string? CollectionName { get; init; }
    public CollectionState State { get; set; } = CollectionState.Pending;
    public int TotalChunks { get; set; }
    public int CompletedChunks { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public List<DataChunk> Chunks { get; init; } = new();
}

public record DataChunk
{
    public required DateTime FromUtc { get; init; }
    public required DateTime ToUtc { get; init; }
    public ChunkState State { get; set; } = ChunkState.Pending;
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
}

public record CandleChunk
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid JobId { get; init; }
    public required string Symbol { get; init; }
    public required DateTime FromUtc { get; init; }
    public required DateTime ToUtc { get; init; }
    public required TimeSpan Interval { get; init; }
    public required List<Candle> Candles { get; init; } = new();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public int CandleCount => Candles.Count;
}