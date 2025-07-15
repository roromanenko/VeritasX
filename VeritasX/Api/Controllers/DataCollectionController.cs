using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeritasX.Core.Interfaces;
using MongoDB.Bson;
using System.Security.Claims;
using VeritasX.Infrastructure.Persistence.Entities;
using VeritasX.Core.Domain;
using VeritasX.Core.DTO;
using VeritasX.Core.Constants;

namespace VeritasX.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DataCollectionController : BaseController
{
    private readonly IDataCollectionService _dataCollectionService;
    private readonly ICandleChunkService _candleChunkService;

    public DataCollectionController(
        IDataCollectionService dataCollectionService,
        ICandleChunkService candleChunkService)
    {
        _dataCollectionService = dataCollectionService;
        _candleChunkService = candleChunkService;
    }

    [HttpPost("queue")]
    [Authorize(Roles = "admin")]
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

    [HttpGet("jobs/{jobId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<DataCollectionJob>>> GetJob(string jobId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var job = await _dataCollectionService.GetJobAsync(jobId, userId!);
        return job != null ? Ok(job) : NotFound();
    }

    [HttpGet("jobs/active")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<IEnumerable<DataCollectionJob>>> GetActiveJobs()
    {
        var jobs = await _dataCollectionService.GetActiveJobsAsync();
        return Ok(jobs);
    }

    [HttpGet("data/{jobId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Candle>>> GetJobData(string jobId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        var candles = await _candleChunkService.GetCandlesByJobIdAsync(jobId, userId!, userRole!);
        return Ok(candles);
    }

    [HttpDelete("jobs/{jobId}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CancelJob(string jobId)
    {
        await _dataCollectionService.CancelJobAsync(jobId);
        return Ok();
    }
} 