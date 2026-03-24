using Api.DTO;
using AutoMapper;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeritasX.Api.Controllers;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatisticsController : BaseController
{
	private readonly IBotStatisticsService _statisticsService;
	private readonly IMapper _mapper;
	private readonly ILogger<StatisticsController> _logger;

	public StatisticsController(
		IBotStatisticsService statisticsService,
		IMapper mapper,
		ILogger<StatisticsController> logger)
	{
		_statisticsService = statisticsService;
		_mapper = mapper;
		_logger = logger;
	}

	/// <summary>
	/// Gets statistics for a specific bot.
	/// </summary>
	[HttpGet("bots/{botId}")]
	[ProducesResponseType(typeof(ApiResponse<GetBotStatisticsResponse>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ApiResponse<GetBotStatisticsResponse>>> GetBotStatistics(
		string botId,
		[FromQuery] DateOnly? from,
		[FromQuery] DateOnly? to,
		CancellationToken ct)
	{
		if (from.HasValue && to.HasValue && from.Value > to.Value)
			return BadRequest(new ApiResponse<GetBotStatisticsResponse>(false, "from must be less than or equal to to"));

		try
		{
			var stats = await _statisticsService.GetBotStatisticsAsync(botId, UserId!, from, to, ct);
			if (stats is null)
				return NotFound(new ApiResponse<GetBotStatisticsResponse>(false, "Bot not found"));

			var dto = _mapper.Map<GetBotStatisticsResponse>(stats);
			return Ok(new ApiResponse<GetBotStatisticsResponse>(true, "Statistics retrieved successfully", dto));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting statistics for bot {BotId}", botId);
			return StatusCode(500, new ApiResponse<GetBotStatisticsResponse>(false, "Internal server error"));
		}
	}

	/// <summary>
	/// Gets aggregated statistics for the current user's account.
	/// </summary>
	[HttpGet("account")]
	[ProducesResponseType(typeof(ApiResponse<GetAccountStatisticsResponse>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<ApiResponse<GetAccountStatisticsResponse>>> GetAccountStatistics(
		[FromQuery] DateOnly? from,
		[FromQuery] DateOnly? to,
		CancellationToken ct)
	{
		if (from.HasValue && to.HasValue && from.Value > to.Value)
			return BadRequest(new ApiResponse<GetAccountStatisticsResponse>(false, "from must be less than or equal to to"));

		try
		{
			var stats = await _statisticsService.GetAccountStatisticsAsync(UserId!, from, to, ct);
			var dto = _mapper.Map<GetAccountStatisticsResponse>(stats);
			return Ok(new ApiResponse<GetAccountStatisticsResponse>(true, "Account statistics retrieved successfully", dto));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting account statistics for user {UserId}", UserId);
			return StatusCode(500, new ApiResponse<GetAccountStatisticsResponse>(false, "Internal server error"));
		}
	}
}
