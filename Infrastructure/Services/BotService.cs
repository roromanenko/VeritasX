using AutoMapper;
using Core.Domain;
using Core.Interfaces;
using Infrastructure.Interfaces;
using Infrastructure.Persistence.Entities;
using MongoDB.Bson;

namespace Infrastructure.Services;

public class BotService : IBotService
{
	private readonly IBotRepository _botRepository;
	private readonly IBotTradeRepository _botTradeRepository;
	private readonly IUserStrategyLibraryRepository _userStrategyLibraryRepository;
	private readonly IMapper _mapper;

	public BotService(
		IBotRepository botRepository,
		IBotTradeRepository botTradeRepository,
		IUserStrategyLibraryRepository userStrategyLibraryRepository,
		IMapper mapper)
	{
		_botRepository = botRepository;
		_botTradeRepository = botTradeRepository;
		_userStrategyLibraryRepository = userStrategyLibraryRepository;
		_mapper = mapper;
	}

	public async Task<BotConfiguration> CreateBot(string userId, BotConfiguration config)
	{
		if (!ObjectId.TryParse(userId, out var userObjectId))
			throw new ArgumentException("Invalid user ID");

		var libraryEntry = await _userStrategyLibraryRepository
			.GetByUserAndStrategy(userObjectId, ObjectId.Parse(config.StrategyId));

		var snapshot = libraryEntry?.SavedDslSnapshot
			?? throw new KeyNotFoundException($"Strategy '{config.StrategyId}' not found in user's library");

		var document = new BotConfigurationDocument
		{
			UserId = userObjectId,
			Name = config.Name,
			Exchange = config.Exchange,
			Symbol = config.Symbol,
			BaseAsset = config.BaseAsset,
			QuoteAsset = config.QuoteAsset,
			StrategyId = config.StrategyId,
			StrategySnapshot = snapshot,
			StrategyVersion = libraryEntry.SavedVersion,
			ParameterOverrides = config.ParameterOverrides ?? new(),
			RiskParameters = _mapper.Map<RiskParametersDocument>(config.RiskParameters)
		};

		var created = await _botRepository.CreateBot(document);
		return _mapper.Map<BotConfiguration>(created);
	}

	public async Task<BotConfiguration> GetBot(string botId, string userId)
	{
		if (!ObjectId.TryParse(botId, out var botObjectId))
			throw new ArgumentException("Invalid user bot ID");

		if (!ObjectId.TryParse(userId, out var userObjectId))
			throw new ArgumentException("Invalid user user ID");

		var document = await _botRepository.GetBotById(botObjectId, userObjectId);
		return document is null
			? throw new KeyNotFoundException($"Bot '{botId}' not found")
			: _mapper.Map<BotConfiguration>(document);
	}

	public async Task<IEnumerable<BotConfiguration>> GetBots(string userId)
	{
		if (!ObjectId.TryParse(userId, out var userObjectId))
			throw new ArgumentException("Invalid user ID");

		var documents = await _botRepository.GetBotsByUserId(userObjectId);
		return _mapper.Map<IEnumerable<BotConfiguration>>(documents);
	}

	public async Task UpdateBot(BotConfiguration bot)
	{
		if (bot.Status is BotStatus.Active or BotStatus.Pending)
			throw new InvalidOperationException("Cannot update a bot that is active or pending");

		var document = _mapper.Map<BotConfigurationDocument>(bot);
		await _botRepository.UpdateBot(document);
	}

	public async Task DeleteBot(string botId, string userId)
	{
		if (!ObjectId.TryParse(botId, out var botObjectId))
			throw new ArgumentException("Invalid bot ID");

		if (!ObjectId.TryParse(userId, out var userObjectId))
			throw new ArgumentException("Invalid user ID");

		await _botRepository.DeleteBot(botObjectId, userObjectId);
	}

	public async Task StartBot(string botId, string userId)
	{
		var bot = await GetBot(botId, userId);

		if (bot.Status == BotStatus.Active || bot.Status == BotStatus.Pending)
			throw new InvalidOperationException($"Bot '{botId}' is already running or pending");

		bot.Status = BotStatus.Pending;
		bot.ErrorMessage = null;
		var document = _mapper.Map<BotConfigurationDocument>(bot);
		await _botRepository.UpdateBot(document);
	}

	public async Task StopBot(string botId, string userId)
	{
		var bot = await GetBot(botId, userId);

		if (bot.Status == BotStatus.Stopped)
			throw new InvalidOperationException($"Bot '{botId}' is already stopped");

		bot.Status = BotStatus.Stopped;
		bot.StoppedAt = DateTimeOffset.UtcNow;
		var document = _mapper.Map<BotConfigurationDocument>(bot);
		await _botRepository.UpdateBot(document);
	}

	public async Task<IEnumerable<BotTradeRecord>> GetTradeHistory(string botId, string userId, int limit = 100)
	{
		if (!ObjectId.TryParse(botId, out var botObjectId))
			throw new ArgumentException("Invalid bot ID");
		if (!ObjectId.TryParse(userId, out var userObjectId))
			throw new ArgumentException("Invalid user ID");

		var bot = await _botRepository.GetBotById(botObjectId, userObjectId)
		?? throw new KeyNotFoundException($"Bot '{botId}' not found");

		var documents = await _botTradeRepository.GetTradesByBotId(botObjectId, limit);
		return _mapper.Map<IEnumerable<BotTradeRecord>>(documents);
	}
}
