using Api.DTO;
using AutoMapper;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeritasX.Api.Controllers;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DataCollectionController : BaseController
{
	private readonly IDataCollectionService _dataCollectionService;
	private readonly ICandleChunkService _candleChunkService;
	private readonly IMapper _mapper;

	public DataCollectionController(
		IDataCollectionService dataCollectionService,
		ICandleChunkService candleChunkService,
		IMapper mapper)
	{
		_dataCollectionService = dataCollectionService;
		_candleChunkService = candleChunkService;
		_mapper = mapper;
	}

	[HttpPost("queue")]
	public async Task<ActionResult<QueueJobResponse>> QueueDataCollection([FromBody] QueueDataCollectionRequest request)
	{
		var fromUtc = request.FromUtc ?? DateTime.UtcNow.AddDays(-1);
		var toUtc = request.ToUtc ?? DateTime.UtcNow;

		var jobId = await _dataCollectionService.QueueDataCollectionAsync(request.Symbol, fromUtc, toUtc, TimeSpan.FromMinutes(request.IntervalMinutes), UserId!);

		return Ok(new QueueJobResponse { JobId = jobId.ToString() });
	}

	[HttpGet("jobs")]
	public async Task<ActionResult<IEnumerable<DataCollectionJobDto>>> GetJobs()
	{
		var jobs = await _dataCollectionService.GetJobsAsync(UserId!);
		var jobsDto = _mapper.Map<IEnumerable<DataCollectionJobDto>>(jobs);

		return jobsDto != null ? Ok(jobsDto) : NotFound();
	}

	[HttpGet("jobs/{jobId}")]
	public async Task<ActionResult<DataCollectionJobDto>> GetJob(string jobId)
	{
		var job = await _dataCollectionService.GetJobAsync(jobId, UserId!);
		var jobDto = _mapper.Map<DataCollectionJobDto>(job);

		return jobDto != null ? Ok(jobDto) : NotFound();
	}

	[HttpGet("jobs/active")]
	[Authorize(Roles = "admin")]
	public async Task<ActionResult<IEnumerable<DataCollectionJobDto>>> GetActiveJobs()
	{
		var jobs = await _dataCollectionService.GetActiveJobsAsync();
		var jobsDto = _mapper.Map<IEnumerable<DataCollectionJobDto>>(jobs);

		return Ok(jobsDto);
	}

	[HttpGet("data/{jobId}")]
	public async Task<ActionResult<IEnumerable<CandleDto>>> GetJobData(string jobId)
	{

		var candles = await _candleChunkService.GetCandlesByJobIdAsync(jobId, UserId!, "admin");
		var candlesDto = _mapper.Map<IEnumerable<CandleDto>>(candles);

		return Ok(candlesDto);
	}

	[HttpDelete("jobs/{jobId}")]
	[Authorize(Roles = "admin")]
	public async Task<IActionResult> CancelJob(string jobId)
	{
		await _dataCollectionService.CancelJobAsync(jobId);
		return Ok();
	}
}