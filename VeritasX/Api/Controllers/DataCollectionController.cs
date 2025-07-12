using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeritasX.Core.Interfaces;

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
    public async Task<IActionResult> QueueDataCollection(
        string symbol = "BTCUSDT",
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        int intervalMinutes = 1)
    {
        fromUtc ??= DateTime.UtcNow.AddDays(-1);
        toUtc ??= DateTime.UtcNow;
        
        var jobId = await _dataCollectionService.QueueDataCollectionAsync(
            symbol, fromUtc.Value, toUtc.Value, TimeSpan.FromMinutes(intervalMinutes));
        
        return Ok(new { JobId = jobId });
    }

    [HttpGet("jobs/{jobId}")]
    [Authorize]
    public async Task<IActionResult> GetJob(Guid jobId)
    {
        var job = await _dataCollectionService.GetJobAsync(jobId);
        return job != null ? Ok(job) : NotFound();
    }

    [HttpGet("jobs/active")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetActiveJobs()
    {
        var jobs = await _dataCollectionService.GetActiveJobsAsync();
        return Ok(jobs);
    }

    [HttpGet("data/{jobId}")]
    [Authorize]
    public async Task<IActionResult> GetJobData(Guid jobId)
    {
        var candles = await _candleChunkService.GetCandlesByJobIdAsync(jobId);
        return Ok(candles);
    }

    [HttpDelete("jobs/{jobId}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CancelJob(Guid jobId)
    {
        await _dataCollectionService.CancelJobAsync(jobId);
        return Ok();
    }
} 