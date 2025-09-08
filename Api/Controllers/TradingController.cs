using Api.DTO;
using AutoMapper;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime;
using Trading;
using Trading.Processors;
using Trading.Strategies;
using VeritasX.Api.Controllers;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TradingController : BaseController
{
	private readonly ICandleChunkService _candleChunkService;
	private readonly IDataCollectionService _dataCollectionService;
	private readonly IMapper _mapper;

	public TradingController(ICandleChunkService candleChunkService, IDataCollectionService dataCollectionService,
		IMapper mapper)
	{
		_candleChunkService = candleChunkService;
		_dataCollectionService = dataCollectionService;
		_mapper = mapper;
	}

	[HttpPost("startHistoryCheck")]
	public async Task<ActionResult<TradingResultDto>> TradeOnHistoryData([FromBody] TradeOnHistoryDataRequest request)
	{
		var job = await _dataCollectionService.GetJobAsync(request.JobId, UserId!);
		if (job is null)
		{
			return NotFound($"Job {request.JobId} not found");
		}

		var strategy = new RebalanceToTargetStrategy(new RebalanceConfig
		{
			Asset = job!.BaseAsset,
			TargetWeight = request.TargetWeight,
			Threshold = request.Threshold,
			MinQty = request.MinQty,
			MinNotional = request.MinNotional
		});

		var candles = await _candleChunkService.GetCandlesByJobIdAsync(job.Id);
		ITradingProcessor processor = new TestTradingProcessor(strategy,
			candles, job, request.InitBaselineQuantity);

		var result = await processor.Start(HttpContext.RequestAborted);
		return Ok(_mapper.Map<TradingResultDto>(result)); 
	}
}
