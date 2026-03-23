using Core.Interfaces;
using Infrastructure.Persistence.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace Infrastructure.Persistence.MongoDb;

/// <summary>
/// Hosted service that creates required MongoDB indexes on application startup.
/// Ensures indexes exist for bot trade records and user strategy library lookups.
/// </summary>
public class MongoIndexInitializer : IHostedService
{
	private readonly IServiceScopeFactory _scopeFactory;

	public MongoIndexInitializer(IServiceScopeFactory scopeFactory)
	{
		_scopeFactory = scopeFactory;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		using var scope = _scopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<IMongoDbContext>();

		await CreateBotTradeRecordIndexes(dbContext, cancellationToken);
		await CreateUserStrategyLibraryIndexes(dbContext, cancellationToken);
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

	private static async Task CreateBotTradeRecordIndexes(IMongoDbContext dbContext, CancellationToken ct)
	{
		var collection = dbContext.GetCollection<BotTradeRecordDocument>();
		var indexKeys = Builders<BotTradeRecordDocument>.IndexKeys
			.Ascending(d => d.BotId)
			.Ascending(d => d.ExecutedAt);

		var indexModel = new CreateIndexModel<BotTradeRecordDocument>(indexKeys);
		await collection.Indexes.CreateOneAsync(indexModel, cancellationToken: ct);
	}

	private static async Task CreateUserStrategyLibraryIndexes(IMongoDbContext dbContext, CancellationToken ct)
	{
		var collection = dbContext.GetCollection<UserStrategyLibraryDocument>();
		var indexKeys = Builders<UserStrategyLibraryDocument>.IndexKeys
			.Ascending(d => d.UserId)
			.Ascending(d => d.StrategyId);

		var indexModel = new CreateIndexModel<UserStrategyLibraryDocument>(indexKeys);
		await collection.Indexes.CreateOneAsync(indexModel, cancellationToken: ct);
	}
}
