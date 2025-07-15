using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeritasX.Core.Interfaces;
using MongoDB.Bson;
using System.Security.Claims;


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
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!ObjectId.TryParse(userIdStr, out var userId))
            return Unauthorized();
        
        fromUtc ??= DateTime.UtcNow.AddDays(-1);
        toUtc ??= DateTime.UtcNow;
        
        var jobId = await _dataCollectionService.QueueDataCollectionAsync(
            symbol, fromUtc.Value, toUtc.Value, TimeSpan.FromMinutes(intervalMinutes), userId);
        
        return Ok(new { JobId = jobId.ToString() });
    }

    [HttpGet("jobs/{jobId}")]
    [Authorize]
    public async Task<IActionResult> GetJob(string jobId)
    {
        if (!ObjectId.TryParse(jobId, out var id)) return BadRequest("Invalid job ID");
        var job = await _dataCollectionService.GetJobAsync(id);
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
    public async Task<IActionResult> GetJobData(string jobId)
    {
        if (!ObjectId.TryParse(jobId, out var id)) return BadRequest("Invalid job ID");
        var candles = await _candleChunkService.GetCandlesByJobIdAsync(id);
        return Ok(candles);
    }

    [HttpDelete("jobs/{jobId}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CancelJob(string jobId)
    {
        if (!ObjectId.TryParse(jobId, out var id)) return BadRequest("Invalid job ID");
        await _dataCollectionService.CancelJobAsync(id);
        return Ok();
    }
} 