using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeritasX.Core.Interfaces;
using MongoDB.Bson;
using System.Security.Claims;
using VeritasX.Infrastructure.Persistence.Entities;
using VeritasX.Core.Domain;
using VeritasX.Core.DTO;
using VeritasX.Core.Constants;
using AutoMapper;

namespace VeritasX.Api.Controllers;

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
    public async Task<ActionResult<QueueJobResponse>> QueueDataCollection(
        string symbol = "BTCUSDT",
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        int intervalMinutes = 1)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();
        
        fromUtc ??= DateTime.UtcNow.AddDays(-1);
        toUtc ??= DateTime.UtcNow;
        
        var jobId = await _dataCollectionService.QueueDataCollectionAsync(
            symbol, fromUtc.Value, toUtc.Value, TimeSpan.FromMinutes(intervalMinutes), userId);
        
        return Ok(new QueueJobResponse { JobId = jobId.ToString() });
    }

	[HttpGet("jobs")]
	[Authorize]
	public async Task<ActionResult<IEnumerable<DataCollectionJobDto>>> GetJobs()
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

		var jobs = await _dataCollectionService.GetJobsAsync(userId!);
		var jobsDto = _mapper.Map<IEnumerable<DataCollectionJobDto>>(jobs);

		return jobsDto != null ? Ok(jobsDto) : NotFound();
	}

	[HttpGet("jobs/{jobId}")]
    [Authorize]
    public async Task<ActionResult<DataCollectionJobDto>> GetJob(string jobId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var job = await _dataCollectionService.GetJobAsync(jobId, userId!);
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
    [Authorize]
    public async Task<ActionResult<IEnumerable<CandleDto>>> GetJobData(string jobId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        var candles = await _candleChunkService.GetCandlesByJobIdAsync(jobId, userId!, userRole!);
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