using Api.DTO;
using AutoMapper;
using Core.Domain;
using Core.Interfaces;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeritasX.Api.Controllers;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BotsController : BaseController
{
	private readonly IBotService _botService;
	private readonly IMapper _mapper;
	private readonly ILogger<BotsController> _logger;

	public BotsController(
		IBotService botService,
		IMapper mapper,
		ILogger<BotsController> logger)
	{
		_botService = botService;
		_mapper = mapper;
		_logger = logger;
	}

	/// <summary>
	/// Gets all bots for the current user.
	/// </summary>
	[HttpGet]
	[ProducesResponseType(typeof(ApiResponse<IEnumerable<BotDto>>), StatusCodes.Status200OK)]
	public async Task<ActionResult<ApiResponse<IEnumerable<BotDto>>>> GetBots()
	{
		try
		{
			var bots = await _botService.GetBots(UserId!);
			var botsDto = _mapper.Map<IEnumerable<BotDto>>(bots);
			return Ok(new ApiResponse<IEnumerable<BotDto>>(true, "Bots retrieved successfully", botsDto));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting bots for user {UserId}", UserId);
			return StatusCode(500, new ApiResponse<IEnumerable<BotDto>>(false, "Internal server error"));
		}
	}

	/// <summary>
	/// Gets a specific bot by id.
	/// </summary>
	[HttpGet("{id}")]
	[ProducesResponseType(typeof(ApiResponse<BotDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<ApiResponse<BotDto>>> GetBot(string id)
	{
		try
		{
			var bot = await _botService.GetBot(id, UserId!);
			return Ok(new ApiResponse<BotDto>(true, "Bot found", _mapper.Map<BotDto>(bot)));
		}
		catch (KeyNotFoundException ex)
		{
			return Ok(new ApiResponse<BotDto>(false, ex.Message));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting bot {BotId}", id);
			return StatusCode(500, new ApiResponse<BotDto>(false, "Internal server error"));
		}
	}

	/// <summary>
	/// Creates a new bot.
	/// </summary>
	[HttpPost]
	[ProducesResponseType(typeof(ApiResponse<BotDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<ApiResponse<BotDto>>> CreateBot([FromBody] CreateBotRequest request)
	{
		try
		{
			var bot = _mapper.Map<BotConfiguration>(request);
			var created = await _botService.CreateBot(UserId!, bot);
			return Ok(new ApiResponse<BotDto>(true, "Bot created successfully", _mapper.Map<BotDto>(created)));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating bot for user {UserId}", UserId);
			return StatusCode(500, new ApiResponse<BotDto>(false, "Internal server error"));
		}
	}

	/// <summary>
	/// Deletes a bot.
	/// </summary>
	[HttpDelete("{id}")]
	[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
	public async Task<ActionResult<ApiResponse<string>>> DeleteBot(string id)
	{
		try
		{
			await _botService.DeleteBot(id, UserId!);
			return Ok(new ApiResponse<string>(true, "Bot deleted successfully"));
		}
		catch (KeyNotFoundException ex)
		{
			return Ok(new ApiResponse<string>(false, ex.Message));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting bot {BotId}", id);
			return StatusCode(500, new ApiResponse<string>(false, "Internal server error"));
		}
	}

	/// <summary>
	/// Starts a bot.
	/// </summary>
	[HttpPost("{id}/start")]
	[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
	public async Task<ActionResult<ApiResponse<string>>> StartBot(string id)
	{
		try
		{
			await _botService.StartBot(id, UserId!);
			return Ok(new ApiResponse<string>(true, "Bot started successfully"));
		}
		catch (KeyNotFoundException ex)
		{
			return Ok(new ApiResponse<string>(false, ex.Message));
		}
		catch (InvalidOperationException ex)
		{
			return Ok(new ApiResponse<string>(false, ex.Message));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error starting bot {BotId}", id);
			return StatusCode(500, new ApiResponse<string>(false, "Internal server error"));
		}
	}

	/// <summary>
	/// Stops a bot.
	/// </summary>
	[HttpPost("{id}/stop")]
	[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
	public async Task<ActionResult<ApiResponse<string>>> StopBot(string id)
	{
		try
		{
			await _botService.StopBot(id, UserId!);
			return Ok(new ApiResponse<string>(true, "Bot stopped successfully"));
		}
		catch (KeyNotFoundException ex)
		{
			return Ok(new ApiResponse<string>(false, ex.Message));
		}
		catch (InvalidOperationException ex)
		{
			return Ok(new ApiResponse<string>(false, ex.Message));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error stopping bot {BotId}", id);
			return StatusCode(500, new ApiResponse<string>(false, "Internal server error"));
		}
	}

	/// <summary>
	/// Gets trade history for a bot.
	/// </summary>
	[HttpGet("{id}/trades")]
	[ProducesResponseType(typeof(ApiResponse<IEnumerable<BotTradeRecordDto>>), StatusCodes.Status200OK)]
	public async Task<ActionResult<ApiResponse<IEnumerable<BotTradeRecordDto>>>> GetTrades(
		string id,
		[FromQuery] int limit = 100)
	{
		try
		{
			var trades = await _botService.GetTradeHistory(id, UserId!, limit);
			var tradesDto = _mapper.Map<IEnumerable<BotTradeRecordDto>>(trades);
			return Ok(new ApiResponse<IEnumerable<BotTradeRecordDto>>(true, "Trades retrieved successfully", tradesDto));
		}
		catch (KeyNotFoundException ex)
		{
			return Ok(new ApiResponse<IEnumerable<BotTradeRecordDto>>(false, ex.Message));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting trades for bot {BotId}", id);
			return StatusCode(500, new ApiResponse<IEnumerable<BotTradeRecordDto>>(false, "Internal server error"));
		}
	}
}
